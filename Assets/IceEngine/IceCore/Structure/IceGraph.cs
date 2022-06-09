using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// 序列化的图结构
    /// </summary>
    [Serializable]
    public class IceGraph
    {
        public List<IceGraphNode> nodeList = new();
    }

    /// <summary>
    /// 序列化的图节点
    /// </summary>
    [Serializable]
    public class IceGraphNode
    {
        // gui位置信息
        public Vector2 position;
        public bool folded;

        public List<Port> outputPorts = new();
        public List<Port> inputPorts = new();

        public void AddOutputPort()
        {

        }

        [Serializable]
        public struct Port
        {
            public int targetId;
            public int targetPort;
        }
    }
}
