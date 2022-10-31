using UnityEngine;
using IceEngine.DebugUI;

namespace IceEngine.Networking.Internal
{
    [AddComponentMenu("[Network]/UI/LatencySimulator")]
    public class LatencySimulator : UIBase
    {
        float latencyMS;
        protected override void OnUI()
        {
            Title("LatencySimulator");
            latencyMS = _FloatField("Latency Override:", latencyMS);
            Ice.Network.LatencyOverrideMS = latencyMS;
        }
    }
}
