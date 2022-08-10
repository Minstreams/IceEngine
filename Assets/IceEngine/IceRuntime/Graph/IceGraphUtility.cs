using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.Graph
{
    public static class IceGraphUtility
    {
        public static bool IsConnectable(IceGraphPort p1, IceGraphPort p2)
        {
            if (p1.node == p2.node) return false;
            if (p1.IsOutport == p2.IsOutport) return false;
            if (!p1.isMultiple && p1.IsConnected) return false;
            if (!p2.isMultiple && p2.IsConnected) return false;
            (IceGraphPort pin, IceGraphPort pout) = p1.IsOutport ? (p2, p1) : (p1, p2);
            if ((pin as IceGraphInport).connectedPorts.Contains(pout as IceGraphOutport)) return false;
            if (pin.valueType.IsAssignableFrom(pout.valueType)) return true;
            return false;
        }
        public static bool IsConnectableTo(this IceGraphPort self, IceGraphPort other) => IsConnectable(self, other);
    }
}
