using System.Net;
using IceEngine.Networking.Framework;

namespace IceEngine.Networking
{
    public sealed class Client : ClientBase
    {
        static IceEngine.Internal.SettingNetwork Setting => Ice.Network.Setting;

        protected override IPAddress LocalIPAddress => Ice.Network.LocalIPAddress;
        protected override int ServerTCPPort => Setting.serverTCPPort;
        protected override int ClientUDPPort => Setting.clientUDPPort;
        protected override int InitialBufferSize => Setting.initialBufferSize;
        protected override byte MagicByte => Setting.magicByte;

        protected override void CallLog(string message) => Ice.Network.CallLog(message);
        protected override void CallShutdownClient() => Ice.Network.ShutdownClient();

        protected override void CallConnection() => Ice.Network.CallConnection();
        protected override void CallDisconnection() => Ice.Network.CallDisconnection();
        protected override void CallReceive(Pkt pkt) => Ice.Network.CallReceive(pkt);
        protected override void CallUDPReceive(Pkt pkt, IPEndPoint remote) => Ice.Network.CallUDPReceive(pkt, remote);
    }
}
