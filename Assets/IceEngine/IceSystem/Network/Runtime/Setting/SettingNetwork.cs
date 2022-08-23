using UnityEngine;

using IceEngine.Framework;

namespace IceEngine.Internal
{
    [IceSettingPath("IceEngine/IceSystem/Network/Setting")]
    public class SettingNetwork : IceSetting<SettingNetwork>
    {
        public int initialBufferSize = 4096;
        public byte magicByte = 0xFF;

        public int serverUDPPort = 7857;
        public int serverTCPPort = 7858;
        public int clientUDPPort = 7856;
        public Vector2Int clientTCPPortRange = new Vector2Int(12306, 17851);
    }
}
