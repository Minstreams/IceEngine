using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace IceEngine.Networking.Framework
{
    /// <summary>
    /// Network object with net id
    /// </summary>
    public abstract class NetworkObject : NetworkBehaviour
    {
        public int ID
        {
            get => _id;
            set
            {
                if (_id == value) return;
                if (_id != 0)
                {
                    Ice.Network.StopListenUDPPacketToId(_id, UDPReceive);
                    Ice.Network.StopProcessUDPPacketFromId(_id, UDPProcess);
                    Ice.Network.StopListenPacketToId(_id, TCPReceive);
                    Ice.Network.StopProcessPacketFromId(_id, TCPProcess);
                }
                if (value != 0)
                {
                    Ice.Network.ListenUDPPacketToId(value, UDPReceive);
                    Ice.Network.ProcessUDPPacketFromId(value, UDPProcess);
                    Ice.Network.ListenPacketToId(value, TCPReceive);
                    Ice.Network.ProcessPacketFromId(value, TCPProcess);
                }
                _id = value;
            }
        }
        int _id = 0;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ID = 0;
        }

        /// <summary>
        /// for client to receive udp packets
        /// </summary>
        static readonly Dictionary<Type, Action<PktId, IPEndPoint>> udpDistributors = new();
        /// <summary>
        /// for server to process udp packets
        /// </summary>
        static readonly Dictionary<Type, Action<PktId, IPEndPoint>> udpProcessors = new();
        /// <summary>
        /// for client to receive tcp packets
        /// </summary>
        static readonly Dictionary<Type, Action<PktId>> tcpDistributors = new();
        /// <summary>
        /// for server to process tcp packets
        /// </summary>
        static readonly Dictionary<Type, Action<PktId, ServerBase.Connection>> tcpProcessors = new();


        void UDPReceive(PktId pktId, IPEndPoint remote)
        {
            var t = pktId.GetType();
            if (udpDistributors.ContainsKey(t)) udpDistributors[t]?.Invoke(pktId, remote);
        }
        void UDPProcess(PktId pktId, IPEndPoint remote)
        {
            var t = pktId.GetType();
            if (udpProcessors.ContainsKey(t)) udpProcessors[t]?.Invoke(pktId, remote);
        }
        void TCPReceive(PktId pktId)
        {
            var t = pktId.GetType();
            if (tcpDistributors.ContainsKey(t)) tcpDistributors[t]?.Invoke(pktId);
        }
        void TCPProcess(PktId pktId, ServerBase.Connection conn)
        {
            var t = pktId.GetType();
            if (tcpProcessors.ContainsKey(t)) tcpProcessors[t]?.Invoke(pktId, conn);
        }


        private protected override void HandleUDPReceiveWithId(Type t, MethodInfo m)
        {
            var action = Expression.Lambda<Action<PktId, IPEndPoint>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPktId, t), pIp), pPktId, pIp).Compile();

            if (!udpDistributors.ContainsKey(t)) udpDistributors.Add(t, null);
            udpDistributors[t] += action;
            _onDestroyEvent += () => udpDistributors[t] -= action;
        }
        private protected override void HandleUDPProcessWithId(Type t, MethodInfo m)
        {
            var action = Expression.Lambda<Action<PktId, IPEndPoint>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPktId, t), pIp), pPktId, pIp).Compile();

            if (!udpProcessors.ContainsKey(t)) udpProcessors.Add(t, null);
            udpProcessors[t] += action;
            _onDestroyEvent += () => udpProcessors[t] -= action;
        }
        private protected override void HandleTCPReceiveWithId(Type t, MethodInfo m)
        {
            var action = Expression.Lambda<Action<PktId>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPktId, t)), pPktId).Compile();

            if (!tcpDistributors.ContainsKey(t)) tcpDistributors.Add(t, null);
            tcpDistributors[t] += action;
            _onDestroyEvent += () => tcpDistributors[t] -= action;
        }
        private protected override void HandleTCPProcessWithId(Type t, MethodInfo m)
        {
            var action = Expression.Lambda<Action<PktId, ServerBase.Connection>>(Expression.Call(Expression.Constant(this), m, Expression.Convert(pPktId, t), pConn), pPktId, pConn).Compile();

            if (!tcpProcessors.ContainsKey(t)) tcpProcessors.Add(t, null);
            tcpProcessors[t] += action;
            _onDestroyEvent += () => tcpProcessors[t] -= action;
        }
    }
}
