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
        #endregion

        #region Serialized Data
        // Runtime
        public List<IceGraphInport> inports = new();
        public List<IceGraphOutport> outports = new();
        #endregion

        #region Interface
        public void AddInport(string name, bool allowMultiple = true, Type valueType = null)
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
        public void AddInport<T>(string name, bool allowMultiple = true) => AddInport(name, allowMultiple, typeof(T));
        public void AddOutport(string name, bool allowMultiple = true, Type valueType = null)
        {
            var port = new IceGraphOutport
            {
                valueType = valueType,
                name = name,
                isMultiple = allowMultiple,
                node = this,
                id = outports.Count,
            };
            outports.Add(port);
        }
        public void AddOutport<T>(string name, bool allowMultiple = true) => AddOutport(name, allowMultiple, typeof(T));
        #endregion


        #region GUI

        #region 基本字段
        public Vector2 position;
        public bool folded;
        #endregion

        #endregion
    }
}
