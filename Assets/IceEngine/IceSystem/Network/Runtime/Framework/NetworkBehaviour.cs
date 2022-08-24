using System;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using UnityEngine;

namespace IceEngine.Networking.Framework
{
    /// <summary>
    /// General network object(without id)
    /// </summary>
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        protected static bool IsHost => Ice.Network.IsHost;
        protected static bool IsConnected => Ice.Network.IsConnected;

        // Thread
        protected static void CallMainThread(Action action) => Ice.Network.CallMainThread(action);

        // Packet Send
        protected static void ClientSend(Pkt pkt) => Ice.Network.ClientSend(pkt);
        protected static void ClientUDPSend(Pkt pkt, IPEndPoint endPoint) => Ice.Network.ClientUDPSend(pkt, endPoint);
        protected static void ServerSend(Pkt pkt, ServerBase.Connection connection) => Ice.Network.ServerSend(pkt, connection);
        protected static void ServerBroadcast(Pkt pkt) => Ice.Network.ServerBroadcast(pkt);
        protected static void ServerUDPSend(Pkt pkt, IPEndPoint endPoint) => Ice.Network.ServerUDPSend(pkt, endPoint);
        protected static void ServerUDPBroadcast(Pkt pkt) => Ice.Network.ServerUDPBroadcast(pkt);

        // Inner Code
        protected private event Action _onDestroyEvent;
        protected virtual void OnDestroy() => _onDestroyEvent?.Invoke();
        protected virtual void Awake()
        {
            var ms = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var m in ms) HandleAttributes(m);
        }

        static readonly Type tConn = typeof(ServerBase.Connection);
        static readonly Type tPkt = typeof(Pkt);
        static readonly Type tIp = typeof(IPEndPoint);
        static readonly Type tPktId = typeof(PktId);
        private protected static readonly ParameterExpression pConn = Expression.Parameter(tConn);
        private protected static readonly ParameterExpression pPkt = Expression.Parameter(tPkt);
        private protected static readonly ParameterExpression pIp = Expression.Parameter(tIp);
        private protected static readonly ParameterExpression pPktId = Expression.Parameter(tPktId);

        /// <summary>
        /// Check if the parameter list is as expected. Throw exception if not.
        /// Enabled only in edit mode
        /// </summary>
        Type ParametersCheck(MethodInfo m, params Type[] parameters)
        {
            var ps = m.GetParameters();
            var l = parameters.Length;

#if DEBUG
            if (ps.Length != l) CheckFail();
            for (int i = 0; i < l; ++i)
            {
                var pt = ps[i].ParameterType;
                if (!pt.Equals(parameters[i]) && !parameters[i].IsAssignableFrom(pt)) CheckFail();
            }

            void CheckFail()
            {
                string errMsg = $"Parameters of {this.name}.{m.Name} does not match as expected.\nExpected as below(";
                for (int i = 0; i < parameters.Length; i++) errMsg += $"[{i}]{parameters[i].FullName},";
                errMsg += ").\nBut the parameters are(";
                for (int i = 0; i < ps.Length; i++) errMsg += $"[{i}]{ps[i].ParameterType.FullName},";
                errMsg += ").";
                throw new Exception(errMsg);
            }
#endif

            return l == 0 ? null : ps[0].ParameterType;
        }
        void HandleAttributes(MethodInfo m)
        {
            foreach (var attr in m.GetCustomAttributes(false))
            {
                if (attr is TCPConnectedAttribute)
                {
                    ParametersCheck(m);

                    var action = Expression.Lambda<Action>(Expression.Call(Expression.Constant(this), m)).Compile();

                    Ice.Network.OnConnection += action;
                    _onDestroyEvent += () => Ice.Network.OnConnection -= action;
                }
                else if (attr is TCPDisconnectedAttribute)
                {
                    ParametersCheck(m);

                    var action = Expression.Lambda<Action>(Expression.Call(Expression.Constant(this), m)).Compile();

                    Ice.Network.OnDisconnection += action;
                    _onDestroyEvent += () => Ice.Network.OnDisconnection -= action;
                }
                else if (attr is TCPServerConnectedAttribute)
                {
                    ParametersCheck(m, tConn);

                    var action = Expression.Lambda<Action<ServerBase.Connection>>(Expression.Call(Expression.Constant(this), m, pConn), pConn).Compile();

                    Ice.Network.OnServerConnection += action;
                    _onDestroyEvent += () => Ice.Network.OnServerConnection -= action;
                }
                else if (attr is TCPServerDisconnectedAttribute)
                {
                    ParametersCheck(m, tConn);

                    var action = Expression.Lambda<Action<ServerBase.Connection>>(Expression.Call(Expression.Constant(this), m, pConn), pConn).Compile();

                    Ice.Network.OnServerDisconnection += action;
                    _onDestroyEvent += () => Ice.Network.OnServerDisconnection -= action;
                }
                else if (attr is UDPReceiveAttribute)
                {
                    var t = ParametersCheck(m, tPkt, tIp);

                    if (tPktId.IsAssignableFrom(t)) HandleUDPReceiveWithId(t, m);
                    else
                    {
                        var action = Expression.Lambda<Action<Pkt, IPEndPoint>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPkt, t), pIp), pPkt, pIp).Compile();

                        Ice.Network.ListenUDPPacket(t, action);
                        _onDestroyEvent += () => Ice.Network.StopListenUDPPacket(t, action);
                    }
                }
                else if (attr is UDPProcessAttribute)
                {
                    var t = ParametersCheck(m, tPkt, tIp);

                    if (tPktId.IsAssignableFrom(t)) HandleUDPProcessWithId(t, m);
                    else
                    {
                        var action = Expression.Lambda<Action<Pkt, IPEndPoint>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPkt, t), pIp), pPkt, pIp).Compile();

                        Ice.Network.ProcessUDPPacket(t, action);
                        _onDestroyEvent += () => Ice.Network.StopProcessUDPPacket(t, action);
                    }
                }
                else if (attr is TCPReceiveAttribute)
                {
                    var t = ParametersCheck(m, tPkt);

                    if (tPktId.IsAssignableFrom(t)) HandleTCPReceiveWithId(t, m);
                    else
                    {
                        var action = Expression.Lambda<Action<Pkt>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPkt, t)), pPkt).Compile();

                        Ice.Network.ListenPacket(t, action);
                        _onDestroyEvent += () => Ice.Network.StopListenPacket(t, action);
                    }
                }
                else if (attr is TCPProcessAttribute)
                {
                    var t = ParametersCheck(m, tPkt, tConn);

                    if (tPktId.IsAssignableFrom(t)) HandleTCPProcessWithId(t, m);
                    else
                    {
                        var action = Expression.Lambda<Action<Pkt, ServerBase.Connection>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPkt, t), pConn), pPkt, pConn).Compile();

                        Ice.Network.ProcessPacket(t, action);
                        _onDestroyEvent += () => Ice.Network.StopProcessPacket(t, action);
                    }
                }
            }
        }

        // Vitual
        private protected virtual void HandleUDPReceiveWithId(Type t, MethodInfo m) => throw new Exception("NetworkBehaviour can not handle packets with id!");
        private protected virtual void HandleUDPProcessWithId(Type t, MethodInfo m) => throw new Exception("NetworkBehaviour can not handle packets with id!");
        private protected virtual void HandleTCPReceiveWithId(Type t, MethodInfo m) => throw new Exception("NetworkBehaviour can not handle packets with id!");
        private protected virtual void HandleTCPProcessWithId(Type t, MethodInfo m) => throw new Exception("NetworkBehaviour can not handle packets with id!");
    }
}
