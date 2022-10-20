using IceEngine.Networking.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace IceEngine.Networking
{
    /// <summary>
    /// Operation & distribution of packets
    /// </summary>
    public static class IceNetworkUtility
    {
        #region Client Ditributors
        /// <summary>
        /// for client to receive udp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt, IPEndPoint>> udpDistributors = new();
        /// <summary>
        /// for client to receive udp packets for given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId, IPEndPoint>> udpIdDistributors = new();
        /// <summary>
        /// for client to receive tcp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt>> tcpDistributors = new();
        /// <summary>
        /// for client to receive tcp packets for given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId>> tcpIdDistributors = new();

        public static event Action OnConnection;
        public static event Action OnDisconnection;

        #region For Objects
        public static void ListenUDPPacket(Type t, Action<Pkt, IPEndPoint> listener)
        {
            if (!udpDistributors.ContainsKey(t)) udpDistributors.Add(t, null);
            udpDistributors[t] += listener;
        }
        public static void StopListenUDPPacket(Type t, Action<Pkt, IPEndPoint> listener)
        {
            if (udpDistributors.ContainsKey(t))
            {
                udpDistributors[t] -= listener;
            }
        }
        public static void ListenUDPPacketToId(int id, Action<PktId, IPEndPoint> listener)
        {
            if (!udpIdDistributors.ContainsKey(id)) udpIdDistributors.Add(id, null);
            udpIdDistributors[id] += listener;
        }
        public static void StopListenUDPPacketToId(int id, Action<PktId, IPEndPoint> listener)
        {
            if (udpIdDistributors.ContainsKey(id))
            {
                udpIdDistributors[id] -= listener;
            }
        }
        public static void ListenPacket(Type t, Action<Pkt> listener)
        {
            if (!tcpDistributors.ContainsKey(t)) tcpDistributors.Add(t, null);
            tcpDistributors[t] += listener;
        }
        public static void StopListenPacket(Type t, Action<Pkt> listener)
        {
            if (tcpDistributors.ContainsKey(t))
            {
                tcpDistributors[t] -= listener;
            }
        }
        public static void ListenPacketToId(int id, Action<PktId> listener)
        {
            if (!tcpIdDistributors.ContainsKey(id)) tcpIdDistributors.Add(id, null);
            tcpIdDistributors[id] += listener;
        }
        public static void StopListenPacketToId(int id, Action<PktId> listener)
        {
            if (tcpIdDistributors.ContainsKey(id))
            {
                tcpIdDistributors[id] -= listener;
            }
        }
        #endregion

        #region For Sockets
        public static void CallConnection() => OnConnection?.Invoke();
        public static void CallDisconnection() => OnDisconnection?.Invoke();
        public static void CallUDPReceive(Pkt pkt, IPEndPoint remote)
        {
            if (pkt is null) return;
            if (pkt is PktId pktId)
            {
                if (udpIdDistributors.ContainsKey(pktId.id))
                {
                    udpIdDistributors[pktId.id]?.Invoke(pktId, remote);
                }
            }
            else
            {
                var t = pkt.GetType();
                if (udpDistributors.ContainsKey(t))
                {
                    udpDistributors[t]?.Invoke(pkt, remote);
                }
            }
        }
        public static void CallReceive(Pkt pkt)
        {
            if (pkt is null) return;
            if (pkt is PktId pktId)
            {
                if (tcpIdDistributors.ContainsKey(pktId.id))
                {
                    tcpIdDistributors[pktId.id]?.Invoke(pktId);
                }
            }
            else
            {
                var t = pkt.GetType();
                if (tcpDistributors.ContainsKey(t))
                {
                    tcpDistributors[t]?.Invoke(pkt);
                }
            }
        }
        #endregion

        #endregion

        #region Server Processor
        /// <summary>
        /// for server to process udp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt, IPEndPoint>> udpProcessors = new();
        /// <summary>
        /// for server to process udp packets from client with given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId, IPEndPoint>> udpIdProcessors = new();
        /// <summary>
        /// for server to process tcp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt, ServerBase.Connection>> tcpProcessors = new();
        /// <summary>
        /// for server to process tcp packets from client with given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId, ServerBase.Connection>> tcpIdProcessors = new();

        public static event Action<ServerBase.Connection> OnServerConnection;
        public static event Action<ServerBase.Connection> OnServerDisconnection;

        #region For Objects
        public static void ProcessUDPPacket(Type t, Action<Pkt, IPEndPoint> processor)
        {
            if (!udpProcessors.ContainsKey(t)) udpProcessors.Add(t, null);
            udpProcessors[t] += processor;
        }
        public static void StopProcessUDPPacket(Type t, Action<Pkt, IPEndPoint> processor)
        {
            if (udpProcessors.ContainsKey(t))
            {
                udpProcessors[t] -= processor;
            }
        }
        public static void ProcessUDPPacketFromId(int id, Action<PktId, IPEndPoint> processor)
        {
            if (!udpIdProcessors.ContainsKey(id)) udpIdProcessors.Add(id, null);
            udpIdProcessors[id] += processor;
        }
        public static void StopProcessUDPPacketFromId(int id, Action<PktId, IPEndPoint> processor)
        {
            if (udpIdProcessors.ContainsKey(id))
            {
                udpIdProcessors[id] -= processor;
            }
        }
        public static void ProcessPacket(Type t, Action<Pkt, ServerBase.Connection> processor)
        {
            if (!tcpProcessors.ContainsKey(t)) tcpProcessors.Add(t, null);
            tcpProcessors[t] += processor;
        }
        public static void StopProcessPacket(Type t, Action<Pkt, ServerBase.Connection> processor)
        {
            if (tcpProcessors.ContainsKey(t))
            {
                tcpProcessors[t] -= processor;
            }
        }
        public static void ProcessPacketFromId(int id, Action<PktId, ServerBase.Connection> processor)
        {
            if (!tcpIdProcessors.ContainsKey(id)) tcpIdProcessors.Add(id, null);
            tcpIdProcessors[id] += processor;
        }
        public static void StopProcessPacketFromId(int id, Action<PktId, ServerBase.Connection> processor)
        {
            if (tcpIdProcessors.ContainsKey(id))
            {
                tcpIdProcessors[id] -= processor;
            }
        }
        #endregion

        #region For Sockets
        public static void CallServerConnection(ServerBase.Connection connection) => OnServerConnection?.Invoke(connection);
        public static void CallServerDisconnection(ServerBase.Connection connection) => OnServerDisconnection?.Invoke(connection);
        public static void CallUDPProcess(Pkt pkt, IPEndPoint remote)
        {
            if (pkt is null) return;
            if (pkt is PktId pktId)
            {
                if (udpIdProcessors.ContainsKey(pktId.id))
                {
                    udpIdProcessors[pktId.id]?.Invoke(pktId, remote);
                }
            }
            else
            {
                var t = pkt.GetType();
                if (udpProcessors.ContainsKey(t))
                {
                    udpProcessors[t]?.Invoke(pkt, remote);
                }
            }
        }
        public static void CallProcess(Pkt pkt, ServerBase.Connection connection)
        {
            if (pkt is null) return;
            if (pkt is PktId pktId)
            {
                if (tcpIdProcessors.ContainsKey(pktId.id))
                {
                    tcpIdProcessors[pktId.id]?.Invoke(pktId, connection);
                }
            }
            else
            {
                var t = pkt.GetType();
                if (tcpProcessors.ContainsKey(t))
                {
                    tcpProcessors[t]?.Invoke(pkt, connection);
                }
            }
        }
        #endregion

        #endregion

        #region Attributes Handler
        static readonly Type tConn = typeof(ServerBase.Connection);
        static readonly Type tPkt = typeof(Pkt);
        static readonly Type tIp = typeof(IPEndPoint);
        static readonly Type tPktId = typeof(PktId);
        internal static readonly ParameterExpression pConn = Expression.Parameter(tConn);
        internal static readonly ParameterExpression pPkt = Expression.Parameter(tPkt);
        internal static readonly ParameterExpression pIp = Expression.Parameter(tIp);
        internal static readonly ParameterExpression pPktId = Expression.Parameter(tPktId);

        public static void HandleNetworkAttributes<T>(T instance, ref Action _onDestroyEvent, NetIDMark id = null)
        {
            var ms = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var m in ms) HandleAttributes(m, ref _onDestroyEvent);

            void HandleAttributes(MethodInfo m, ref Action onDestroy)
            {
                foreach (var attr in m.GetCustomAttributes(false))
                {
                    if (attr is TCPConnectedAttribute)
                    {
                        ParametersCheck(m);

                        var action = Expression.Lambda<Action>(Expression.Call(Expression.Constant(instance), m)).Compile();

                        OnConnection += action;
                        onDestroy += () => OnConnection -= action;
                    }
                    else if (attr is TCPDisconnectedAttribute)
                    {
                        ParametersCheck(m);

                        var action = Expression.Lambda<Action>(Expression.Call(Expression.Constant(instance), m)).Compile();

                        OnDisconnection += action;
                        onDestroy += () => OnDisconnection -= action;
                    }
                    else if (attr is TCPServerConnectedAttribute)
                    {
                        ParametersCheck(m, tConn);

                        var action = Expression.Lambda<Action<ServerBase.Connection>>(Expression.Call(Expression.Constant(instance), m, pConn), pConn).Compile();

                        OnServerConnection += action;
                        onDestroy += () => OnServerConnection -= action;
                    }
                    else if (attr is TCPServerDisconnectedAttribute)
                    {
                        ParametersCheck(m, tConn);

                        var action = Expression.Lambda<Action<ServerBase.Connection>>(Expression.Call(Expression.Constant(instance), m, pConn), pConn).Compile();

                        OnServerDisconnection += action;
                        onDestroy += () => OnServerDisconnection -= action;
                    }
                    else if (attr is UDPReceiveAttribute)
                    {
                        var t = ParametersCheck(m, tPkt, tIp);

                        if (tPktId.IsAssignableFrom(t))
                        {
                            var action = Expression.Lambda<Action<PktId, IPEndPoint>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPktId, t), pIp), pPktId, pIp).Compile();

                            id.HandleUDPReceiveWithId(t, action, ref onDestroy);
                        }
                        else
                        {
                            var action = Expression.Lambda<Action<Pkt, IPEndPoint>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPkt, t), pIp), pPkt, pIp).Compile();

                            ListenUDPPacket(t, action);
                            onDestroy += () => StopListenUDPPacket(t, action);
                        }
                    }
                    else if (attr is UDPProcessAttribute)
                    {
                        var t = ParametersCheck(m, tPkt, tIp);

                        if (tPktId.IsAssignableFrom(t))
                        {
                            var action = Expression.Lambda<Action<PktId, IPEndPoint>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPktId, t), pIp), pPktId, pIp).Compile();

                            id.HandleUDPProcessWithId(t, action, ref onDestroy);
                        }
                        else
                        {
                            var action = Expression.Lambda<Action<Pkt, IPEndPoint>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPkt, t), pIp), pPkt, pIp).Compile();

                            ProcessUDPPacket(t, action);
                            onDestroy += () => StopProcessUDPPacket(t, action);
                        }
                    }
                    else if (attr is TCPReceiveAttribute)
                    {
                        var t = ParametersCheck(m, tPkt);

                        if (tPktId.IsAssignableFrom(t))
                        {
                            var action = Expression.Lambda<Action<PktId>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPktId, t)), pPktId).Compile();

                            id.HandleTCPReceiveWithId(t, action, ref onDestroy);
                        }
                        else
                        {
                            var action = Expression.Lambda<Action<Pkt>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPkt, t)), pPkt).Compile();

                            ListenPacket(t, action);
                            onDestroy += () => StopListenPacket(t, action);
                        }
                    }
                    else if (attr is TCPProcessAttribute)
                    {
                        var t = ParametersCheck(m, tPkt, tConn);

                        if (tPktId.IsAssignableFrom(t))
                        {
                            var action = Expression.Lambda<Action<PktId, ServerBase.Connection>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPktId, t), pConn), pPktId, pConn).Compile();

                            id.HandleTCPProcessWithId(t, action, ref onDestroy);
                        }
                        else
                        {
                            var action = Expression.Lambda<Action<Pkt, ServerBase.Connection>>(Expression.Call(Expression.Constant(instance), m, Expression.Convert(pPkt, t), pConn), pPkt, pConn).Compile();

                            ProcessPacket(t, action);
                            onDestroy += () => StopProcessPacket(t, action);
                        }
                    }
                }
            }
            /// <summary>
            /// Check if the parameter list is as expected. Throw exception if not.
            /// Enabled only in edit mode
            /// </summary>
            static Type ParametersCheck(MethodInfo m, params Type[] parameters)
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
                    string errMsg = $"Parameters of {m.DeclaringType}.{m.Name} does not match as expected.\nExpected as below(";
                    for (int i = 0; i < parameters.Length; i++) errMsg += $"[{i}]{parameters[i].FullName},";
                    errMsg += ").\nBut the parameters are(";
                    for (int i = 0; i < ps.Length; i++) errMsg += $"[{i}]{ps[i].ParameterType.FullName},";
                    errMsg += ").";
                    throw new Exception(errMsg);
                }
#endif

                return l == 0 ? null : ps[0].ParameterType;
            }
        }
        #endregion
    }

}
