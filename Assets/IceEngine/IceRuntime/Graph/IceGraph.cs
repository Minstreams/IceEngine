using System;
using System.Collections.Generic;

using UnityEngine;

namespace IceEngine.Graph
{
    [Serializable]
    public class IceGraph
    {
        #region Cache
        [NonSerialized] public List<IceGraphNode> nodeList = new();
        protected virtual void OnDeserialized()
        {
            // Step1: initialize ports
            foreach (var node in nodeList)
            {
                node.InitializePorts();
            }

            // Step2: repair connection references
            foreach (var node in nodeList)
            {
                for (int i = 0; i < node.outports.Count; ++i)
                {
                    var op = node.outports[i];
                    op.connectedPorts = node.connectionData[i];
                    for (int pi = 0; pi < op.connectedPorts.Count; ++pi)
                    {
                        var pd = op.connectedPorts[pi];
                        var ip = nodeList[pd.nodeId].inports[pd.portId];
                        op.connectedPorts[pi] = ip.data;
                        ip.connectedPorts.Add(op);
                    }
                }
            }

            // Step3: update node cache
            for (int ni = 0; ni < nodeList.Count; ++ni) UpdateNodeCache(nodeList[ni], ni);
        }
        void UpdateNodeCache(IceGraphNode node, int id)
        {
            node.graph = this;
            node.id = id;
            foreach (var ip in node.inports) ip.data.nodeId = id;
        }
        #endregion

        #region Serialized Data
        public byte[] data = null;
        public void Serialize()
        {
            data = IceBinaryUtility.ToBytes(nodeList, withHeader: true);
        }
        public void Deserialize()
        {
            IceBinaryUtility.FromBytesOverride(data, nodeList, withHeader: true);
            OnDeserialized();
        }
        #endregion

        #region Interface
        public void AddNode(Type nodeType) => AddNode(Activator.CreateInstance(nodeType) as IceGraphNode);
        public void AddNode<Node>() where Node : IceGraphNode => AddNode(Activator.CreateInstance<Node>());
        void AddNode(IceGraphNode node)
        {
            int index = nodeList.Count;
            node.InitializePorts();
            UpdateNodeCache(node, index);
            nodeList.Add(node);
        }
        public void RemoveNode(IceGraphNode node) => RemoveNodeAt(node.id);
        public void RemoveNodeAt(int i)
        {
            var node = nodeList[i];
            foreach (var ip in node.inports) ip.DisconnectAll();
            foreach (var op in node.outports) op.DisconnectAll();

            int end = nodeList.Count - 1;
            if (end != i)
            {
                var newNode = nodeList[end];
                nodeList[i] = newNode;
                newNode.id = i;
                foreach (var ip in newNode.inports) ip.data.nodeId = i;
            }
            nodeList.RemoveAt(end);
        }
        #endregion

        #region Configuration
        public virtual Color GetPortColor(IceGraphPort port)
        {
            Type t = port.valueType;
            if (t == typeof(int)) return Color.cyan;
            return Color.white;
        }
        public virtual bool IsConnectable(IceGraphPort p1, IceGraphPort p2)
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
        #endregion
    }
}
