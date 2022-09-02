using System;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Events/Base Events"), RuntimeConst]
    public class NodeBaseEvents : IceprintNode
    {
        [IceprintPort] public Action onStart;
        [IceprintPort] public Action onUpdate;
        [IceprintPort] public Action onDestroy;
    }
}
