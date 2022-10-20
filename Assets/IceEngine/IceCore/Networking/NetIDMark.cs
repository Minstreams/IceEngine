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
        int? _id = null;

        public int ID
        {
            get => _id ?? throw new NullReferenceException("ID is null. Use HasID to check null before get id");
            set => SetID(value);
        }
        public bool HasID => _id != null;
        public void SetID(int id)
        {
            if (_id == id) return;
            ClearID();
            ListenUDPPacketToId(id, UDPReceive);
            ProcessUDPPacketFromId(id, UDPProcess);
            ListenPacketToId(id, TCPReceive);
            ProcessPacketFromId(id, TCPProcess);
            _id = id;
        }
        public void ClearID()
        {
            if (!HasID) return;
            StopListenUDPPacketToId(ID, UDPReceive);
            StopProcessUDPPacketFromId(ID, UDPProcess);
            StopListenPacketToId(ID, TCPReceive);
            StopProcessPacketFromId(ID, TCPProcess);
            _id = null;
        }

        public static implicit operator int(NetIDMark idMark) => idMark.ID;


        ~NetIDMark() => ClearID();

        #region Attributes Handler
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

        internal void HandleUDPReceiveWithId(Type t, Action<PktId, IPEndPoint> action, ref Action _onDestroyEvent)
        {
            if (!udpDistributors.ContainsKey(t)) udpDistributors.Add(t, null);
            udpDistributors[t] += action;
            _onDestroyEvent += () => udpDistributors[t] -= action;
        }
        internal void HandleUDPProcessWithId(Type t, Action<PktId, IPEndPoint> action, ref Action _onDestroyEvent)
        {
            if (!udpProcessors.ContainsKey(t)) udpProcessors.Add(t, null);
            udpProcessors[t] += action;
            _onDestroyEvent += () => udpProcessors[t] -= action;
        }
        internal void HandleTCPReceiveWithId(Type t, Action<PktId> action, ref Action _onDestroyEvent)
        {
            if (!tcpDistributors.ContainsKey(t)) tcpDistributors.Add(t, null);
            tcpDistributors[t] += action;
            _onDestroyEvent += () => tcpDistributors[t] -= action;
        }
        internal void HandleTCPProcessWithId(Type t, Action<PktId, ServerBase.Connection> action, ref Action _onDestroyEvent)
        {
            if (!tcpProcessors.ContainsKey(t)) tcpProcessors.Add(t, null);
            tcpProcessors[t] += action;
            _onDestroyEvent += () => tcpProcessors[t] -= action;
        }
        #endregion
    }
}
