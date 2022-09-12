using System.Net;
using UnityEngine;
using IceEngine.DebugUI;

namespace IceEngine.Networking.Internal
{
    /// <summary>
    /// 代理网络系统的各种功能
    /// </summary>
    [AddComponentMenu("[Network]/UI/NetworkSystemAgent")]
    public class NetworkSystemAgent : UIBase
    {
        public string serverAddress;

        //Input
        [ContextMenu("LaunchServer")]
        public void LaunchServer() => Ice.Network.LaunchServer();
        [ContextMenu("LaunchClient")]
        public void LaunchClient() => Ice.Network.LaunchClient();
        [ContextMenu("ShutdownServer")]
        public void ShutdownServer() => Ice.Network.ShutdownServer();
        [ContextMenu("ShutdownClient")]
        public void ShutdownClient() => Ice.Network.ShutdownClient();
        [ContextMenu("ServerOpenTCP")]
        public void ServerOpenTCP() => Ice.Network.ServerOpenTCP();
        [ContextMenu("ServerCloseTCP")]
        public void ServerCloseTCP() => Ice.Network.ServerCloseTCP();
        [ContextMenu("ClientOpenUDP")]
        public void ClientOpenUDP() => Ice.Network.ClientOpenUDP();
        [ContextMenu("ClientCloseUDP")]
        public void ClientCloseUDP() => Ice.Network.ClientCloseUDP();
        [ContextMenu("ClientConnectTo")]
        public void ClientConnectTo() => Ice.Network.ClientConnectTo(IPAddress.Parse(serverAddress));
        [ContextMenu("ClientDisconnect")]
        public void ClientDisconnect() => Ice.Network.ClientDisconnect();
        [ContextMenu("DetectLocalIPAddress")]
        public void DetectLocalIPAddress()
        {
            Ice.Network.DetectLocalIPAddress();
            serverAddress = Ice.Network.LocalIPAddressList[0].ToString();
        }

        protected override void OnUI()
        {
            Title("Network System");

            Ice.Network.Setting.serverUDPPort = _IntField("Server UDP Port", Ice.Network.Setting.serverUDPPort);
            Ice.Network.Setting.clientUDPPort = _IntField("Client UDP Port", Ice.Network.Setting.clientUDPPort);
            Ice.Network.Setting.serverTCPPort = _IntField("Server TCP Port", Ice.Network.Setting.serverTCPPort);
            serverAddress = _TextField("Server Address", serverAddress);

            if (GUILayout.Button("LaunchServer")) LaunchServer();
            if (GUILayout.Button("LaunchClient")) LaunchClient();
            if (GUILayout.Button("ShutdownServer")) ShutdownServer();
            if (GUILayout.Button("ShutdownClient")) ShutdownClient();
            if (GUILayout.Button("ServerOpenTCP")) ServerOpenTCP();
            if (GUILayout.Button("ServerCloseTCP")) ServerCloseTCP();
            if (GUILayout.Button("ClientOpenUDP")) ClientOpenUDP();
            if (GUILayout.Button("ClientCloseUDP")) ClientCloseUDP();
            if (GUILayout.Button("ClientConnectTo")) ClientConnectTo();
            if (GUILayout.Button("ClientDisconnect")) ClientDisconnect();
            if (GUILayout.Button("DetectLocalIPAddress")) DetectLocalIPAddress();
        }
    }
}