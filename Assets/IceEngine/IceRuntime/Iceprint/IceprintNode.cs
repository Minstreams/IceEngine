using System;
using System.Collections.Generic;

using UnityEngine;

using IceEngine.Framework;
using IceEngine.Internal;

namespace IceEngine
{
    public abstract class IceprintNode : IcePacketBase
    {
        #region Cache
        [NonSerialized] public IceprintGraph graph;
        [NonSerialized] public int id;
        [NonSerialized] public List<IceprintInport> inports = new();
        [NonSerialized] public List<IceprintOutport> outports = new();
        #endregion

        #region Serialized Data
        public List<List<IceprintInportData>> connectionData = new();

        // GUI
        public Vector2 position;
        public bool folded;
        #endregion

        #region Interface
        protected void AddInport(string name, bool allowMultiple = true, Type valueType = null)
        {
            var port = new IceprintInport
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
            var port = new IceprintOutport
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
