using UnityEngine;
using IceEngine.DebugUI;

namespace IceEngine.Networking.Internal
{
    [AddComponentMenu("[Network]/UI/LatencySimulator")]
    public class LatencySimulator : UIBase
    {
        int latencyMS;
        protected override void OnUI()
        {
            Title("LatencySimulator");
            latencyMS = _IntField("Latency Override:", latencyMS);
            Ice.Network.latencyOverride = latencyMS / 1000.0f;
        }
    }
}
