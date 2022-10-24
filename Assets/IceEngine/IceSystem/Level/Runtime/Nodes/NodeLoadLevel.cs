using IceEngine.Framework;
using Sys = Ice.Level;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Ice/Level/LoadLevel")]
    public class NodeLoadLevel : IceprintNode
    {
        // Ports
        [IceprintPort] public void LoadLevel(string sceneName) => Sys.LoadLevel(sceneName);
    }
}