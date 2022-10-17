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
        public virtual NetIDMark ID => null;

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
        event Action _onDestroy;
        protected virtual void Awake() => IceNetworkUtility.HandleNetworkAttributes(this, ref _onDestroy, ID);
        protected virtual void OnDestroy()
        {
            _onDestroy?.Invoke();
            if (ID != null)
            {
                ID.ID = 0;
            }
        }
    }
}
