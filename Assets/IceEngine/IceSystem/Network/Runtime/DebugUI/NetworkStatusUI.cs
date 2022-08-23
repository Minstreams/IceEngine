using UnityEngine;
using IceEngine.DebugUI;

namespace IceEngine.Networking.Internal
{
    [AddComponentMenu("[Network]/UI/NetworkStatusUI")]
    public class NetworkStatusUI : UIBase
    {
        protected override void OnUI()
        {
            TitleLabel("NetworkStatusUI");
            GUILayout.Label("LocalIP:" + Ice.Network.LocalIPAddress);
            GUILayout.Label("ServerIP:" + Ice.Network.ServerIPAddress);
            GUILayout.Label("Latency Override:" + Ice.Network.latencyOverride);
            GUILayout.Label("Id: " + Ice.Network.NetId);
        }
    }
}
