using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Utility/MonoAgent")]
    public class NodeMonoAgent : IceprintNode
    {
        // Fields
        public Type targetType;
        public int targetGUID;
        [NonSerialized] public MonoBehaviour target;

        public override void InitializePorts() { }
    }
}
