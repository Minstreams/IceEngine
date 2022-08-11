using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IceEngine.Graph;
using IceEngine.Internal;

namespace IceEngine.Blueprint
{
    /// <summary>
    /// 蓝图结构基类
    /// </summary>
    [Serializable]
    public class IceBlueprint : IceGraph
    {
        protected override void OnDeserialized()
        {
            base.OnDeserialized();
        }
        public override Color GetPortColor(IceGraphPort port)
        {
            return base.GetPortColor(port);
        }
        public override bool IsConnectable(IceGraphPort p1, IceGraphPort p2)
        {
            return base.IsConnectable(p1, p2);
        }
    }

    [IcePacket]
    public class InputNode : IceGraphNode
    {
        public string name;

        public override void InitializePorts()
        {
            AddOutport<float>(name, true);
        }
    }

    [IcePacket]
    public class OutputNode : IceGraphNode
    {
        public string name;

        public override void InitializePorts()
        {
            AddInport<float>(name, true);
        }
    }
}
