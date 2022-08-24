using System.Net;
using IceEngine.Networking.Framework;

namespace IceEngine.Networking
{
    public sealed class Server : ServerBase
    {
        static IceEngine.Internal.SettingNetwork Setting => Ice.Network.Setting;

        protected override IPAddress LocalIPAddress => Ice.Network.LocalIPAddress;
        protected override IPAddress BroadcastAddress => Ice.Network.BroadcastAddress;
        protected override int ServerUDPPort => Setting.serverUDPPort;
        protected override int ServerTCPPort => Setting.serverTCPPort;
        protected override int ClientUDPPort => Setting.clientUDPPort;
        protected override int LocalTCPPort => Ice.Network.LocalTCPPort;
        protected override int InitialBufferSize => Setting.initialBufferSize;
        protected override byte MagicByte => Setting.magicByte;

        protected override void CallLog(string message) => Ice.Network.CallLog(message);
        protected override void CallShutdownServer() => Ice.Network.CallMainThread(Ice.Network.ShutdownServer);

        protected override void CallUDPProcess(Pkt pkt, IPEndPoint remote) => Ice.Network.CallUDPProcess(pkt, remote);
        protected override void CallProcess(Pkt pkt, Connection connection) => Ice.Network.CallProcess(pkt, connection);
        protected override void CallServerConnection(Connection connection) => Ice.Network.CallServerConnection(connection);
        protected override void CallServerDisconnection(Connection connection) => Ice.Network.CallServerDisconnection(connection);
    }
}