using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// 图端口
    /// </summary>
    [Serializable]
    public class IceGraphPort
    {
        public const float PORT_SIZE = 16;
        public const float PORT_RADIUS = PORT_SIZE * 0.5f;

        // 临时数据
        [NonSerialized] public IceGraphNode node;
        [NonSerialized] public bool isOutport;
        [NonSerialized] public int portId;
        [NonSerialized] public List<IceGraphPort> targetPortList;
        [NonSerialized] public Type valueType;


        // 序列化数据
        public string name;
        public string valueTypeName;
        public List<int> targetNodeIdList = new();
        public List<int> targetPortIdList = new();
        public bool isMultiple;

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(valueTypeName)) valueType = Type.GetType(valueTypeName);
        }
        public void OnBeforeSerialize()
        {
            // 构造序列化数据
            valueTypeName = valueType?.FullName;

            targetNodeIdList.Clear();
            targetPortIdList.Clear();
            if (IsConnected)
            {
                for (int p = 0; p < targetPortList.Count; p++)
                {
                    var targetPort = targetPortList[p];
                    targetNodeIdList.Add(targetPort.node.nodeId);
                    targetPortIdList.Add(targetPort.portId);
                }
            }
        }

        public Color Color => node.graph.GetPortColor(this);
        public bool IsConnected => targetPortList?.Count > 0;

        public IceGraphPort(string name, Type valueType, IceGraphNode node, int portId, bool isOutport, bool isMultiple)
        {
            this.name = name;
            this.node = node;
            this.portId = portId;
            this.isOutport = isOutport;
            this.valueType = valueType;
            this.isMultiple = isMultiple;
            targetPortList = new();
        }

        public void ConnectTo(IceGraphPort port)
        {
            targetPortList.Add(port);
            port.targetPortList.Add(this);
        }
        public void DisconnectFrom(IceGraphPort port)
        {
            targetPortList.Remove(port);
            port.targetPortList.Remove(this);
        }
        public void DisconnectAll()
        {
            foreach (var port in targetPortList)
            {
                port.targetPortList.Remove(this);
            }
            targetPortList.Clear();
        }
    }
}
