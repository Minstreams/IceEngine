using UnityEngine;

using IceEngine.Framework;
using System.Net;

namespace IceEngine.Internal
{
    [IceSettingPath("IceEngine/IceSystem/Network/Setting")]
    public class SettingNetwork : IceSetting<SettingNetwork>
    {
        public int initialBufferSize = 4096;
        public byte magicByte = 0xC9;

        public int serverUDPPort = 7857;
        public int serverTCPPort = 7858;
        public int clientUDPPort = 7856;

        public string defaultServerIPString = "127.0.0.1";
        public IPAddress DefaultServerAddress => IPAddress.Parse(defaultServerIPString);
    }
}
