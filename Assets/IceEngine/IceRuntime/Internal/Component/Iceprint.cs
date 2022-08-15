using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace IceEngine.Internal
{
    public sealed class Iceprint : MonoBehaviour
    {
        #region Cache
        [NonSerialized] public List<IceprintNode> nodeList = new();

        void OnDeserialized()
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
        void UpdateNodeCache(IceprintNode node, int id)
        {
            node.graph = this;
            node.id = id;
            foreach (var ip in node.inports) ip.data.nodeId = id;
        }
        #endregion

        #region Serialized Data
        public byte[] Serialize() => IceBinaryUtility.ToBytes(nodeList, withHeader: true);
        public void Deserialize(byte[] data)
        {
            IceBinaryUtility.FromBytesOverride(data, nodeList, withHeader: true);
            OnDeserialized();
        }
        #endregion

        #region Interface
        public void AddNode(Type nodeType) => AddNode(Activator.CreateInstance(nodeType) as IceprintNode);
        public void AddNode<Node>() where Node : IceprintNode => AddNode(Activator.CreateInstance<Node>());
        void AddNode(IceprintNode node)
        {
            int index = nodeList.Count;
            node.InitializePorts();
            UpdateNodeCache(node, index);
            nodeList.Add(node);
        }
        public void RemoveNode(IceprintNode node) => RemoveNodeAt(node.id);
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
        public Color GetPortColor(IceprintPort port)
        {
            Type t = port.valueType;
            if (t == typeof(int)) return Color.cyan;
            return Color.white;
        }
        public bool IsConnectable(IceprintPort p1, IceprintPort p2)
        {
            if (p1.node == p2.node) return false;
            if (p1.IsOutport == p2.IsOutport) return false;
            if (!p1.isMultiple && p1.IsConnected) return false;
            if (!p2.isMultiple && p2.IsConnected) return false;

            (IceprintPort pin, IceprintPort pout) = p1.IsOutport ? (p2, p1) : (p1, p2);
            if ((pin as IceprintInport).connectedPorts.Contains(pout as IceprintOutport)) return false;
            if (pin.valueType == pout.valueType) return true;
            if (pin.valueType is null || pout.valueType is null) return false;
            if (pin.valueType.IsAssignableFrom(pout.valueType)) return true;

            return false;
        }
        #endregion
    }
}
