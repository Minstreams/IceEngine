using System;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.Internal
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

        public void InvokeValue<T>(T value)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T>)?.Invoke(value);
            }
        }
        public void InvokeValue<T1, T2>(T1 v1, T2 v2)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2>)?.Invoke(v1, v2);
            }
        }
        #endregion
    }
}
