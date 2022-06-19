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
    public class IceGraph : ISerializationCallbackReceiver
    {
        public List<IceGraphNode> nodeList = new();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // 解析序列化数据
            //Debug.Log("IceGraph".Color(Color.green) + ".OnAfterDeserialize");

            // TODO: 下放到node

            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                node.graph = this;
                node.nodeId = i;

                for (int ip = 0; ip < node.inports.Count; ip++)
                {
                    var port = node.inports[ip];
                    port.node = node;
                    port.isOutport = false;
                    port.portId = ip;
                    port.targetPortList = new List<IceGraphPort>();

                    if (port.targetNodeIdList.Count > 0)
                    {
                        port.targetPortList.Clear();

                        for (int p = 0; p < port.targetNodeIdList.Count; p++)
                        {
                            port.targetPortList.Add(nodeList[port.targetNodeIdList[p]].outports[port.targetPortIdList[p]]);
                        }
                    }

                    port.OnAfterDeserialize();
                }

                for (int op = 0; op < node.outports.Count; op++)
                {
                    var port = node.outports[op];
                    port.node = node;
                    port.isOutport = true;
                    port.portId = op;
                    port.targetPortList = new List<IceGraphPort>();

                    if (port.targetNodeIdList.Count > 0)
                    {
                        port.targetPortList.Clear();

                        for (int p = 0; p < port.targetNodeIdList.Count; p++)
                        {
                            port.targetPortList.Add(nodeList[port.targetNodeIdList[p]].inports[port.targetPortIdList[p]]);
                        }
                    }

                    port.OnAfterDeserialize();
                }
            }
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // 构造序列化数据
            //Debug.Log("IceGraph".Color(Color.green) + ".OnBeforeSerialize");

            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];

                foreach (var port in node.inports) port.OnBeforeSerialize();
                foreach (var port in node.outports) port.OnBeforeSerialize();
            }
        }

        public void AddNode(IceGraphNode node)
        {
            node.graph = this;
            node.nodeId = nodeList.Count;
            nodeList.Add(node);
        }
        public virtual bool IsPortsMatch(IceGraphPort p1, IceGraphPort p2)
        {
            if (p1.valueType == p2.valueType) return true;
            return false;
        }
        public virtual Color GetPortColor(IceGraphPort port)
        {
            Type t = port.valueType;
            if (t == typeof(int)) return Color.cyan;
            return Color.white;
        }
    }
}