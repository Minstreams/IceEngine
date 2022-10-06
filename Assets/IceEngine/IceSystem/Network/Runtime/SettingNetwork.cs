using System.Net;

namespace IceEngine.Internal
{
    public class SettingNetwork : Framework.IceSetting<SettingNetwork>
    {
        public SettingNetwork()
        {
            themeColor = new UnityEngine.Color(0.1259f, 0.4575f, 0.6509f);
        }

        public int initialBufferSize = 4096;
        public byte magicByte = 0xC9;

        public int serverUDPPort = 7857;
        public int serverTCPPort = 7858;
        public int clientUDPPort = 7856;

        public string defaultServerIPString = "127.0.0.1";
        public string defaultServerDomain = "minstreams.com";
        public IPAddress DefaultServerAddress => IPAddress.Parse(defaultServerIPString);
        public IPAddress DefaultServerDomain => Dns.GetHostEntry(defaultServerDomain).AddressList[0];
    }
}
