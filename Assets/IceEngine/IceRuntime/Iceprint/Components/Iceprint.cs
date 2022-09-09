using System;
using System.Collections.Generic;

using UnityEngine;

using IceEngine.Framework;
using IceEngine.IceprintNodes;
using IceEngine.Internal;

namespace IceEngine
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Ice/Iceprint")]
    public sealed class Iceprint : MonoBehaviour
    {
        #region Cache
        [NonSerialized] public List<IceprintNode> nodeList = new();

        void OnDeserialized()
        {
            // Step1: initialize node data
            for (int ni = 0; ni < nodeList.Count; ++ni) InitializeNodeData(nodeList[ni], ni);

            // Step2: repair connection references
            foreach (var node in nodeList)
            {
                int opCount = node.outports.Count;
                for (int i = 0; i < opCount; ++i)
                {
                    var op = node.outports[i];
                    var cd = node.connectionData[i];
                    op.connectedPorts = cd;
                    for (int pi = 0; pi < cd.Count; ++pi)
                    {
                        var pd = cd[pi];
                        var ip = nodeList[pd.nodeId].inports[pd.portId];

                        // 去掉无效连接
                        if (!IsConnectable(ip, op))
                        {
                            cd.RemoveAt(pi);
                            --pi;
                            continue;
                        }

                        cd[pi] = ip.data;
                        ip.connectedPorts.Add(op);
                    }
                }

                // remove redundant connectionData
                for (int i = node.connectionData.Count - 1; i >= opCount; --i)
                {
                    node.connectionData.RemoveAt(i);
                }
            }
        }
        void InitializeNodeData(IceprintNode node, int id)
        {
            node.graph = this;
            node.id = id;
            foreach (var ip in node.inports) ip.data.nodeId = id;
            if (id == _fieldMap.Count)
            {
                _fieldMap.Add(new FieldTable());
                MarkDirty();
            }
            node.Initialize();
        }
        #endregion

        #region Serialized Data

        #region Graph Data
        public byte[] graphData = null;
        public byte[] Serialize()
        {
            graphData = IceBinaryUtility.ToBytes(nodeList, withHeader: true, withExtraInfo: true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            return graphData;
        }
        public void Deserialize(byte[] data = null)
        {
            if (data is null) data = graphData;
            if (graphData != data)
            {
                graphData = data;
                MarkDirty();
            }
            try
            {
                IceBinaryUtility.FromBytesOverwrite(data, nodeList, withHeader: true, withExtraInfo: true);
                OnDeserialized();
            }
            catch (Exception ex)
            {
                nodeList = new();
                _fieldMap = new();
                Serialize();
                Debug.LogWarning("Deserialize failed, node list is reset.\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
        #endregion

        #region Node Fields
        [Serializable]
        public class FieldTable
        {
            [SerializeField] List<UnityEngine.Object> _tbl = new();
            public int Count => _tbl.Count;
            public UnityEngine.Object this[int id]
            {
                get => _tbl[id];
                set => _tbl[id] = value;
            }
            public void Add(UnityEngine.Object obj) => _tbl.Add(obj);
        }
        public List<FieldTable> _fieldMap = new();
        public T GetField<T>(int nodeId, ref int fieldId) where T : UnityEngine.Object
        {
            // Get table
            var table = _fieldMap[nodeId];

            // Calculate fieldId
            int count = table.Count;
            if (fieldId < 0)
            {
                // new field
                fieldId = count;
                table.Add(null);
                MarkDirty();
                return null;
            }
            else
            {
                if (fieldId >= count) throw new IndexOutOfRangeException($"Get field failed at {fieldId} out of {count}");
                // old field
                return table[fieldId] as T;
            }
        }
        public void SetField(int nodeId, int fieldId, UnityEngine.Object field)
        {
            if (_fieldMap[nodeId][fieldId] == field) return;

            _fieldMap[nodeId][fieldId] = field;
            MarkDirty();
        }
        #endregion

        #endregion

        #region Interface
        public IceprintNode AddNode(Type nodeType, Vector2 pos = default) => AddNode(Activator.CreateInstance(nodeType) as IceprintNode, pos);
        public IceprintNode AddNode<Node>(Vector2 pos = default) where Node : IceprintNode => AddNode(Activator.CreateInstance<Node>(), pos);
        public IceprintNode AddNode(IceprintNode node, Vector2 pos = default)
        {
            int index = nodeList.Count;
            node.position = pos;
            nodeList.Add(node);
            InitializeNodeData(node, index);
            return node;
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
                _fieldMap[i] = _fieldMap[end];
                foreach (var ip in newNode.inports) ip.data.nodeId = i;
            }
            node.graph = null;
            nodeList.RemoveAt(end);
            _fieldMap.RemoveAt(end);
        }
        #endregion

        #region Configuration
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
                    onStart += nbe.onStart;
                    onUpdate += nbe.onUpdate;
                    onDestroy += nbe.onDestroy;
                }
            }
        }
        void UnloadGraph()
        {
            onStart = null;
            onUpdate = null;
            onDestroy = null;
        }
        void Start()
        {
            LoadGraph();
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

        #region Editor
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        #endregion
    }
}
