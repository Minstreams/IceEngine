using System;
using System.Collections.Generic;

namespace IceEngine.Graph
{
    public abstract class IceGraphPort
    {
        #region Cache
        [NonSerialized] public IceGraphNode node;
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
        public abstract void ConnectTo(IceGraphPort other);
        public abstract void DisconnectFrom(IceGraphPort other);
        public abstract void DisconnectAll();
        #endregion
    }

    [IcePacket]
    [Serializable]
    public sealed class IceGraphInportData
    {
        #region Cache
        [NonSerialized] public IceGraphInport port;
        #endregion

        public int nodeId;
        public int portId;
    }

    [IcePacket]
    [Serializable]
    public sealed class IceGraphInport : IceGraphPort
    {
        #region Cache
        [NonSerialized] public IceGraphInportData data = new();
        [NonSerialized] public List<IceGraphOutport> connectedPorts = new();
        #endregion

        public IceGraphInport()
        {
            data.port = this;
        }

        #region Interface
        public override bool IsOutport => false;
        public override bool IsConnected => connectedPorts.Count > 0;
        public override void ConnectTo(IceGraphPort other)
        {
            if (!this.IsConnectableTo(other)) throw new Exception($"IceGraphInport {name} can not connect to {other}");

            if (other is IceGraphOutport op)
            {
                op.connectedPorts.Add(data);
                connectedPorts.Add(op);
            }
            else throw new ArgumentException($"IceGraphInport {name} can not connect to {other}");
        }
        public override void DisconnectFrom(IceGraphPort other)
        {
            if (other is IceGraphOutport op)
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

    [IcePacket]
    [Serializable]
    public sealed class IceGraphOutport : IceGraphPort
    {
        #region Serialized Data
        // Runtime
        public List<IceGraphInportData> connectedPorts = new();
        #endregion

        #region Interface
        public override bool IsOutport => true;
        public override bool IsConnected => connectedPorts.Count > 0;
        public override void ConnectTo(IceGraphPort other)
        {
            if (!this.IsConnectableTo(other)) throw new Exception($"IceGraphOutport {name} can not connect to {other}");

            if (other is IceGraphInport ip)
            {
                ip.connectedPorts.Add(this);
                connectedPorts.Add(ip.data);
            }
            else throw new ArgumentException($"IceGraphOutport {name} can not connect to {other}");
        }
        public override void DisconnectFrom(IceGraphPort other)
        {
            if (other is IceGraphInport ip)
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