using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using IceEngine.Framework;
using IceEngine.Internal;

namespace IceEngine
{
    public abstract class IceprintNode : IcePacketBase
    {
        #region Cache
        [NonSerialized] public Iceprint graph;
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
        protected void AddInport(string name, Action action)
        {
            var port = new IceprintInport
            {
                valueType = null,
                name = name,
                node = this,
                id = inports.Count,
            };
            port.data.action = action;
            port.data.nodeId = id;
            port.data.portId = port.id;
            inports.Add(port);
        }
        protected void AddInport(string name, Type valueType, Action<object> action)
        {
            var port = new IceprintInport
            {
                valueType = valueType,
                name = name,
                node = this,
                id = inports.Count,
            };
            port.data.action = action;
            port.data.nodeId = id;
            port.data.portId = port.id;
            inports.Add(port);
        }
        protected void AddOutport(string name, Type valueType = null)
        {
            var port = new IceprintOutport
            {
                valueType = valueType,
                name = name,
                node = this,
                id = outports.Count,
            };
            if (connectionData.Count == port.id) connectionData.Add(port.connectedPorts);
            outports.Add(port);
        }
        protected void AddOutport<T>(string name) => AddOutport(name, typeof(T));

        public void InvokeOutput(int id)
        {
            var port = outports[id];
            foreach (var pd in port.connectedPorts) (pd.action as Action)?.Invoke();
        }
        public void InvokeOutput(int id, object value)
        {
            var port = outports[id];
            foreach (var pd in port.connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<object>)?.Invoke(value);
            }
        }
        #endregion

        #region Configuration
        public virtual void InitializePorts()
        {
            var t = GetType();
            foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<IceprintInportAttribute>();
                if (attr == null) continue;

                if (m.IsGenericMethod)
                {
                    throw new Exception($"Iceprint Inport can not be generic! node:[{t.FullName}] port:[{m.Name}]");
                }
                var ps = m.GetParameters();
                if (ps.Length > 1)
                {
                    throw new Exception($"Iceprint Inport can not have more than 1 parameter! node:[{t.FullName}] port:[{m.Name}]");
                }
                if (ps.Length == 0)
                {
                    AddInport(m.Name, () => m.Invoke(this, null));
                }
                else
                {
                    var d = m.CreateDelegate(typeof(Action<>).MakeGenericType(ps[0].ParameterType), this);
                    AddInport(m.Name, ps[0].ParameterType, param => m.Invoke(this, new object[] { param }));
                }
            }
        }
        #endregion
    }

    [IceprintMenuItem("Test/asdas")]
    public class TestNode : IceprintNode
    {
        [IceprintInport]
        public void InTest()
        {
            Debug.Log("Yes!");
        }
        [IceprintInport]
        public void InTest2(float f)
        {
            Debug.Log($"No!{f}");
        }
        public override void InitializePorts()
        {
            base.InitializePorts();
            AddOutport("Out");
        }
    }

    [RuntimeConst]
    [IceprintMenuItem("Test/UpdateNode")]
    public class UpdateNode : IceprintNode
    {
        public override void InitializePorts()
        {
            base.InitializePorts();
            AddOutport("Update");
        }
    }

    [IceprintMenuItem("Test/TestMidNode")]
    public class TestMidNode : IceprintNode
    {
        [IceprintInport]
        public void InGo()
        {
            InvokeOutput(0, 1.2f);
        }
        public override void InitializePorts()
        {
            base.InitializePorts();
            AddOutport<float>("Go");
        }
    }
}
