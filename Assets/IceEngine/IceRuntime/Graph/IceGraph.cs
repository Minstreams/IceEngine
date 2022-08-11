using System;
using System.Collections.Generic;

using UnityEngine;

namespace IceEngine.Graph
{
    [IcePacket]
    public class IceGraph
    {
        #region Cache
        public virtual void OnDeserialized()
        {
            // Step1: redirect outport data
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

            // Step2: do update cache
            for (int ni = 0; ni < nodeList.Count; ++ni) UpdateNodeCache(nodeList[ni], ni);
        }
        void UpdateNodeCache(IceGraphNode node, int id)
        {
            node.graph = this;
            node.id = id;
            for (int pi = 0; pi < node.inports.Count; ++pi)
            {
                var port = node.inports[pi];
                //port.node = node;
                //port.id = pi;

                var pd = port.data;
                pd.nodeId = id;
                //pd.portId = pi;
            }
            //for (int pi = 0; pi < node.outports.Count; ++pi)
            //{
            //    var port = node.outports[pi];
            //    port.node = node;
            //    port.id = pi;
            //}
        }
        #endregion

        #region Serialized Data
        [SerializeField] public List<IceGraphNode> nodeList = new();
        #endregion

        #region Interface
        public void AddNode(IceGraphNode node)
        {
            int index = nodeList.Count;
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
