using System;
using System.Collections.Generic;

namespace IceEngine.Internal
{
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
}
