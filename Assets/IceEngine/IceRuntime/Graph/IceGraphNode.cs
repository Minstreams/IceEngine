using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.Graph
{
    [IcePacket]
    [Serializable]
    public class IceGraphNode
    {
        #region Cache
        [NonSerialized] public IceGraph graph;
        [NonSerialized] public int id;
        [NonSerialized] public List<IceGraphInport> inports = new();
        [NonSerialized] public List<IceGraphOutport> outports = new();
        #endregion

        #region Serialized Data
        public List<List<IceGraphInportData>> connectionData = new();
        #endregion

        #region Interface
        protected void AddInport(string name, bool allowMultiple = true, Type valueType = null)
        {
            var port = new IceGraphInport
            {
                valueType = valueType,
                name = name,
                isMultiple = allowMultiple,
                node = this,
                id = inports.Count,
            };
            port.data.nodeId = id;
            port.data.portId = inports.Count;
            inports.Add(port);
        }
        protected void AddInport<T>(string name, bool allowMultiple = true) => AddInport(name, allowMultiple, typeof(T));
        protected void AddOutport(string name, bool allowMultiple = true, Type valueType = null)
        {
            var port = new IceGraphOutport
            {
                valueType = valueType,
                name = name,
                isMultiple = allowMultiple,
                node = this,
                id = outports.Count,
            };
            connectionData.Add(port.connectedPorts);
            outports.Add(port);
        }
        protected void AddOutport<T>(string name, bool allowMultiple = true) => AddOutport(name, allowMultiple, typeof(T));

        public IceGraphNode()
        {
            AddOutport<float>("out10");
            AddOutport<int>("testOut2");
            AddInport<float>("V");
            AddInport<int>("F");
        }
        #endregion


        #region GUI

        #region 基本字段
        public Vector2 position;
        public bool folded;
        #endregion

        #endregion
    }
}
