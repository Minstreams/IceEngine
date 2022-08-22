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
        #endregion

        #region Invoke Methods
        public void Invoke()
        {
            foreach (var pd in connectedPorts) (pd.action as Action)?.Invoke();
        }
        public void Invoke<T>(T value)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T>)?.Invoke(value);
            }
        }
        public void Invoke<T1, T2>(T1 v1, T2 v2)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2>)?.Invoke(v1, v2);
            }
        }
        public void Invoke<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3>)?.Invoke(v1, v2, v3);
            }
        }
        public void Invoke<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4>)?.Invoke(v1, v2, v3, v4);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5>)?.Invoke(v1, v2, v3, v4, v5);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6>)?.Invoke(v1, v2, v3, v4, v5, v6);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7>)?.Invoke(v1, v2, v3, v4, v5, v6, v7);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10, T11 v11)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10, T11 v11, T12 v12)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10, T11 v11, T12 v12, T13 v13)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10, T11 v11, T12 v12, T13 v13, T14 v14)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10, T11 v11, T12 v12, T13 v13, T14 v14, T15 v15)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15);
            }
        }
        public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10, T11 v11, T12 v12, T13 v13, T14 v14, T15 v15, T16 v16)
        {
            foreach (var pd in connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>)?.Invoke(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16);
            }
        }
        #endregion
    }
}
