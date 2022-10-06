using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Utility/CallSubSystem")]
    public class NodeSubSystemCaller : IceprintNode
    {
        // Fields
        public string method;

        // Ports
        [IceprintPort]
        public void CallSubSystem()
        {
            Ice.Island.CallSubSystem(method);
        }
        [IceprintPort]
        public void CallSubSystem(string method)
        {
            Ice.Island.CallSubSystem(method);
        }
    }
}
