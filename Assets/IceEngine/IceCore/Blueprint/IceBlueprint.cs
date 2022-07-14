using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IceEngine.Internal;

namespace IceEngine.Blueprint
{
    /// <summary>
    /// 蓝图结构基类
    /// </summary>
    [System.Serializable]
    public class IceBlueprint : IceGraph
    {
        public override Color GetPortColor(IceGraphPort port)
        {
            return base.GetPortColor(port);
        }

        public override bool IsPortsMatch(IceGraphPort p1, IceGraphPort p2)
        {
            return base.IsPortsMatch(p1, p2);
        }
    }
}
