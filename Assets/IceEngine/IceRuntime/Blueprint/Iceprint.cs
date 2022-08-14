using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IceEngine.Graph;
using IceEngine.Internal;

namespace IceEngine.Blueprint
{

    public class Iceprint : MonoBehaviour
    {
        public IceprintGraph print;


    }
    /// <summary>
    /// 冰图基本节点
    /// </summary>
    public abstract class IceprintNode : IceGraphNode
    {

    }
    public class TestprintNode : IceprintNode
    {
        public override void InitializePorts()
        {
            // TODO
            AddInport("input");
            AddOutport("output");
        }
    }
    /// <summary>
    /// 冰图
    /// </summary>
    public class IceprintGraph : IceGraph<IceprintNode>
    {
        public override Color GetPortColor(IceGraphPort port)
        {
            return base.GetPortColor(port);
        }

        public override bool IsConnectable(IceGraphPort p1, IceGraphPort p2)
        {
            return base.IsConnectable(p1, p2);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void OnDeserialized()
        {
            base.OnDeserialized();
        }
    }
}
