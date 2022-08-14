using System;
using System.Collections.Generic;

using IceEngine.Internal;

namespace IceEngine
{
    namespace Internal
    {
        public abstract class IceprintPort
        {
            #region Cache
            [NonSerialized] public IceprintNode node;
            [NonSerialized] public int id;
            #endregion

            #region Serialized Data
            // Runtime
            public Type valueType;
            public bool isMultiple;

            // Editor
            public string name;
            #endregion

            #region Interface
            public abstract bool IsOutport { get; }
            public abstract bool IsConnected { get; }
            public abstract void ConnectTo(IceprintPort other);
            public abstract void DisconnectFrom(IceprintPort other);
            public abstract void DisconnectAll();
            #endregion
        }

        [IcePacket]
        public sealed class IceprintInportData
        {
            #region Cache
            [NonSerialized] public IceprintInport port;
            #endregion

            public int nodeId;
            public int portId;
        }
    }

    public sealed class IceprintInport : IceprintPort
    {
        #region Cache
        [NonSerialized] public IceprintInportData data = new();
        [NonSerialized] public List<IceprintOutport> connectedPorts = new();
        #endregion

        public IceprintInport()
        {
            data.port = this;
        }

        #region Interface
        public override bool IsOutport => false;
        public override bool IsConnected => connectedPorts.Count > 0;
        public override void ConnectTo(IceprintPort other)
        {
            if (other is IceprintOutport op)
            {
                op.connectedPorts.Add(data);
                connectedPorts.Add(op);
            }
            else throw new ArgumentException($"IceGraphInport {name} can not connect to {other}");
        }
        public override void DisconnectFrom(IceprintPort other)
        {
            if (other is IceprintOutport op)
            {
                op.connectedPorts.Remove(data);
                connectedPorts.Remove(op);
            }
            else throw new ArgumentException($"IceGraphInport {name} can not disconnect from {other}");
        }
        public override void DisconnectAll()
        {
            foreach (var op in connectedPorts)
            {
                op.connectedPorts.Remove(data);
            }
            connectedPorts.Clear();
        }
        #endregion
    }

    public sealed class IceprintOutport : IceprintPort
    {
        #region Serialized Data
        public List<IceprintInportData> connectedPorts = new();
        #endregion

        #region Interface
        public override bool IsOutport => true;
        public override bool IsConnected => connectedPorts.Count > 0;
        public override void ConnectTo(IceprintPort other)
        {
            if (other is IceprintInport ip)
            {
                ip.connectedPorts.Add(this);
                connectedPorts.Add(ip.data);
            }
            else throw new ArgumentException($"IceGraphOutport {name} can not connect to {other}");
        }
        public override void DisconnectFrom(IceprintPort other)
        {
            if (other is IceprintInport ip)
            {
                ip.connectedPorts.Remove(this);
                connectedPorts.Remove(ip.data);
            }
            else throw new ArgumentException($"IceGraphOutport {name} can not disconnect from {other}");

        }
        public override void DisconnectAll()
        {
            foreach (var ip in connectedPorts)
            {
                ip.port.connectedPorts.Remove(this);
            }
            connectedPorts.Clear();
        }
        #endregion
    }
}