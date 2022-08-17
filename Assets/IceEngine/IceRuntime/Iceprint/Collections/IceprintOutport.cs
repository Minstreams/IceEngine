using System;
using System.Collections.Generic;
using IceEngine.Internal;

namespace IceEngine
{
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
