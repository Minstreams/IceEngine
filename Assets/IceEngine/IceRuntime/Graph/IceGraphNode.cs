using System;
using System.Collections.Generic;

using UnityEngine;

using IceEngine.Graph.Internal;
using IceEngine.Framework;

namespace IceEngine.Graph
{
    public abstract class IceGraphNode : IcePacketBase
    {
        #region Cache
        [NonSerialized] public IceGraph graph;
        [NonSerialized] public int id;
        [NonSerialized] public List<IceGraphInport> inports = new();
        [NonSerialized] public List<IceGraphOutport> outports = new();
        #endregion

        #region Serialized Data
        public List<List<IceGraphInportData>> connectionData = new();

        // GUI
        public Vector2 position;
        public bool folded;
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
            port.data.portId = port.id;
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
            if (connectionData.Count == port.id) connectionData.Add(port.connectedPorts);
            outports.Add(port);
        }
        protected void AddOutport<T>(string name, bool allowMultiple = true) => AddOutport(name, allowMultiple, typeof(T));
        #endregion

        #region Configuration
        public abstract void InitializePorts();
        #endregion
    }
}
