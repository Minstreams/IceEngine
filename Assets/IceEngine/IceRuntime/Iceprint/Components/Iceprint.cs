using System;
using System.Collections.Generic;

using UnityEngine;

using IceEngine.Framework;
using IceEngine.IceprintNodes;
using IceEngine.Internal;

namespace IceEngine
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
        public byte[] graphData = null;
        public byte[] Serialize()
        {
            graphData = IceBinaryUtility.ToBytes(nodeList, withHeader: true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            return graphData;
        }
        public void Deserialize(byte[] data = null)
        {
            if (data == null) data = graphData;
            try
            {
                IceBinaryUtility.FromBytesOverride(data, nodeList, withHeader: true);
                OnDeserialized();
            }
            catch (Exception ex)
            {
                nodeList = new();
                Serialize();
                Debug.LogException(ex);
            }
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
        public Color GetColor(IceprintPort port)
        {
            Color c = Color.white;
            for (int i = 0; i < port.ParamsList.Count; ++i)
            {
                c = Color.LerpUnclamped(GetColor(port.ParamsList[i]), c, i / ((float)(i + 1)));
            }
            return c;
        }
        public Color GetColor(Type paramType)
        {
            if (paramType == typeof(int)) return Color.cyan;
            if (paramType == typeof(float)) return new Color(1.0f, 0.6f, 0.2f);
            if (paramType == typeof(string)) return new Color(1.0f, 0.7f, 0.1f);
            return Color.white;
        }
        public string GetParamTypeName(Type paramType)
        {
            if (paramType is null) return "void";
            if (paramType == typeof(int)) return "int";
            if (paramType == typeof(float)) return "float";
            if (paramType == typeof(string)) return "string";
            return paramType.Name;
        }
        public bool IsConnectable(IceprintPort p1, IceprintPort p2)
        {
            if (p1.node == p2.node) return false;
            if (p1.IsOutport == p2.IsOutport) return false;
            if (!p1.isMultiple && p1.IsConnected) return false;
            if (!p2.isMultiple && p2.IsConnected) return false;

            (IceprintPort pin, IceprintPort pout) = p1.IsOutport ? (p2, p1) : (p1, p2);
            if ((pin as IceprintInport).connectedPorts.Contains(pout as IceprintOutport)) return false;
            if (pin.paramsHash == pout.paramsHash) return true;
            if (pin.paramsHash == 0) return true;
            if (pout.paramsHash == 0) return false;
            if (pin.ParamsList.Count == pout.ParamsList.Count)
            {
                for (int i = 0; i < pin.ParamsList.Count; i++)
                {
                    if (!pin.ParamsList[i].IsAssignableFrom(pout.ParamsList[i])) return false;
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Runtime
        public void ReloadGraph()
        {
            UnloadGraph();
            LoadGraph();
        }

        public Action onAwake;
        public Action onStart;
        public Action onUpdate;
        public Action onDestroy;
        void LoadGraph()
        {
            Deserialize();
            foreach (var node in nodeList)
            {
                if (node is NodeBaseEvents nbe)
                {
                    onAwake += nbe.onAwake;
                    onStart += nbe.onStart;
                    onUpdate += nbe.onUpdate;
                    onDestroy += nbe.onDestroy;
                }
            }
        }
        void UnloadGraph()
        {
            onUpdate = null;
        }

        void Awake()
        {
            LoadGraph();
            onAwake?.Invoke();
        }
        void Start()
        {
            onStart?.Invoke();
        }
        void Update()
        {
            onUpdate?.Invoke();
        }
        void OnDestroy()
        {
            onDestroy?.Invoke();
        }
        #endregion
    }
}
