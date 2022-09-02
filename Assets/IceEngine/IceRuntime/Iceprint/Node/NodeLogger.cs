using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Utility/Logger")]
    public class NodeLogger : IceprintNode
    {
        // Field
        public string message;

        // Ports
        [IceprintPort]
        public void Log()
        {
            Debug.Log(message, graph);
        }
        [IceprintPort]
        public void Log(object obj)
        {
            Debug.Log(obj, graph);
        }
    }
}
