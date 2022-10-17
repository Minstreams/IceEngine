using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using IceEngine.Networking.Framework;

using static IceEngine.Networking.IceNetworkUtility;

namespace IceEngine.Networking
{
    /// <summary>
    /// ID Mark for a network object
    /// </summary>
    public class NetIDMark
    {
        public int ID
        {
            get => _id;
            set
            {
                if (_id == value) return;
                if (_id != 0)
                {
                    StopListenUDPPacketToId(_id, UDPReceive);
                    StopProcessUDPPacketFromId(_id, UDPProcess);
                    StopListenPacketToId(_id, TCPReceive);
                    StopProcessPacketFromId(_id, TCPProcess);
                }
                if (value != 0)
                {
                    ListenUDPPacketToId(value, UDPReceive);
                    ProcessUDPPacketFromId(value, UDPProcess);
                    ListenPacketToId(value, TCPReceive);
                    ProcessPacketFromId(value, TCPProcess);
                }
                _id = value;
            }
        }

        int _id = 0;
        ~NetIDMark() => ID = 0;

        /// <summary>
        /// for client to receive udp packets
        /// </summary>
        readonly Dictionary<Type, Action<PktId, IPEndPoint>> udpDistributors = new();
        /// <summary>
        /// for server to process udp packets
        /// </summary>
        readonly Dictionary<Type, Action<PktId, IPEndPoint>> udpProcessors = new();
        /// <summary>
        /// for client to receive tcp packets
        /// </summary>
        readonly Dictionary<Type, Action<PktId>> tcpDistributors = new();
        /// <summary>
        /// for server to process tcp packets
        /// </summary>
        readonly Dictionary<Type, Action<PktId, ServerBase.Connection>> tcpProcessors = new();

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

        public void HandleUDPReceiveWithId(Type t, MethodInfo m, Action<PktId, IPEndPoint> action, ref Action _onDestroyEvent)
        {
            if (!udpDistributors.ContainsKey(t)) udpDistributors.Add(t, null);
            udpDistributors[t] += action;
            _onDestroyEvent += () => udpDistributors[t] -= action;
        }
        public void HandleUDPProcessWithId(Type t, MethodInfo m, Action<PktId, IPEndPoint> action, ref Action _onDestroyEvent)
        {
            if (!udpProcessors.ContainsKey(t)) udpProcessors.Add(t, null);
            udpProcessors[t] += action;
            _onDestroyEvent += () => udpProcessors[t] -= action;
        }
        public void HandleTCPReceiveWithId(Type t, MethodInfo m, Action<PktId> action, ref Action _onDestroyEvent)
        {
            if (!tcpDistributors.ContainsKey(t)) tcpDistributors.Add(t, null);
            tcpDistributors[t] += action;
            _onDestroyEvent += () => tcpDistributors[t] -= action;
        }
        public void HandleTCPProcessWithId(Type t, MethodInfo m, Action<PktId, ServerBase.Connection> action, ref Action _onDestroyEvent)
        {
            if (!tcpProcessors.ContainsKey(t)) tcpProcessors.Add(t, null);
            tcpProcessors[t] += action;
            _onDestroyEvent += () => tcpProcessors[t] -= action;
        }

        public static implicit operator int(NetIDMark idMark) => idMark.ID;
    }
}
