using UnityEngine;
using IceEngine;
using IceEngine.DebugUI;
using IceEngine.Networking.Framework;

namespace IceEngine.Networking.Internal
{
    public class PktString : Pkt
    {
        public string str;
    }

    /// <summary>
    /// 用于服务器向特定客户端发送和接收tcp消息
    /// </summary>
    [AddComponentMenu("[Network]/UI/ServerConnectionAgent")]
    public class ServerConnectionAgent : UIBase
    {
        public string message;
        public int connectionIndex;

        ServerBase.Connection connection;
        [ContextMenu("Init")]
        public void Init()
        {
            Init(Ice.Network.Server.GetConnection(connectionIndex));
        }
        public void Init(ServerBase.Connection connection)
        {
            this.connection = connection;
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
            Ice.Network.Server.UDPSend(new PktString() { str = message }, connection.UDPEndPoint);
        }

        [ContextMenu("UDPBroadcast")]
        public void UDPBroadcast() => Ice.Network.Server.UDPBroadcast(new PktString() { str = message });

        protected override void OnUI()
        {
            Title("Server Agent");

            message = _TextField("Message", message);
            connectionIndex = _IntField("Connection Index", connectionIndex);

            if (GUILayout.Button("Init")) Init();
            if (connection != null)
            {
                if (GUILayout.Button("Send")) Send();
                if (GUILayout.Button("UDPSend")) UDPSend();
            }
            if (GUILayout.Button("UDPBroadcast")) UDPBroadcast();
        }
    }
}