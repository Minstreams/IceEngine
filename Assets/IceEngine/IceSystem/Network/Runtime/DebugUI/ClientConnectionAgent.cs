using System.Net;
using UnityEngine;
using IceEngine.DebugUI;
using IceEngine.Networking;

namespace IceEngine.Networking.Internal
{
    /// <summary>
    /// 用于客户端向服务器发送和接收tcp消息
    /// </summary>
    [AddComponentMenu("[Network]/UI/ClientConnectionAgent")]
    public class ClientConnectionAgent : UIBase
    {
        //Data
        public string message;

        Client connection;
        [ContextMenu("Init")]
        public void Init()
        {
            connection = Ice.Network.Client;
        }

        //Input
        [ContextMenu("Send")]
        public void Send() => Send(message);
        public void Send(string message)
        {
            if (connection == null)
            {
                Debug.LogWarning("No connection assigned");
                return;
            }
            connection.Send(new PktString() { str = message });
        }
        [ContextMenu("UDPSend")]
        public void UDPSend()
        {
            if (connection == null)
            {
                Debug.LogWarning("No connection assigned");
                return;
            }
            connection.UDPSend(new PktString() { str = message }, new IPEndPoint(Ice.Network.ServerIPAddress, Ice.Network.Setting.serverUDPPort));
        }
        [ContextMenu("UDPBroadcastToServer")]
        public void UDPBroadcastToServer()
        {
            if (connection == null)
            {
                Debug.LogWarning("No connection assigned");
                return;
            }
            connection.UDPSend(new PktString() { str = message }, new IPEndPoint(Ice.Network.BroadcastAddress, Ice.Network.Setting.serverUDPPort));
        }
        [ContextMenu("UDPBroadcastToClient")]
        public void UDPBroadcastToClient()
        {
            if (connection == null)
            {
                Debug.LogWarning("No connection assigned");
                return;
            }
            connection.UDPSend(new PktString() { str = message }, new IPEndPoint(Ice.Network.BroadcastAddress, Ice.Network.Setting.clientUDPPort));
        }

        protected override void OnUI()
        {
            Title("Client Agent");
            if (GUILayout.Button("Init")) Init();
            if (connection != null)
            {
                message = _TextField("Message", message);

                if (GUILayout.Button("Send")) Send();
                if (GUILayout.Button("UDPSend")) UDPSend();
                if (GUILayout.Button("UDPBroadcastToServer")) UDPBroadcastToServer();
                if (GUILayout.Button("UDPBroadcastToClient")) UDPBroadcastToClient();
            }
        }
    }
}