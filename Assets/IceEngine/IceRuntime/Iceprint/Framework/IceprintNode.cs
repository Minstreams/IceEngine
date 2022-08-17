using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using IceEngine;
using IceEngine.Framework;
using IceEngine.Internal;

using static IceEngine.IceprintUtility;
using System.Linq.Expressions;

namespace IceEngine.Framework
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
        protected IceprintInport AddInport(string name, Type valueType = null)
        {
            var port = new IceprintInport
            {
                valueType = valueType,
                name = name,
                node = this,
                id = inports.Count,
            };
            port.data.nodeId = id;
            port.data.portId = port.id;
            inports.Add(port);
            return port;
        }
        protected IceprintOutport AddOutport(string name, Type valueType = null)
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
            return port;
        }

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
                var attr = m.GetCustomAttribute<IceprintPortAttribute>();
                if (attr == null) continue;

                // TODO: 支持一下模板函数
                if (m.IsGenericMethodDefinition)
                {
                    throw new Exception($"Iceprint Inport can not be generic for now! node:[{t.FullName}] port:[{m.Name}]");
                }

                // 这里解析Inport数据
                // 支持一下默认参数
                {
                    var ps = m.GetParameters();

                    if (ps.Length > 1) throw new Exception($"Iceprint Inport can not have more than 1 parameter! node:[{t.FullName}] port:[{m.Name}]");

                    if (ps.Length == 0)
                    {
                        var port = AddInport(m.Name);

                        var exp = Expression.Lambda(
                            actionType,
                            Expression.Call(Expression.Constant(this), m)
                            );

                        port.data.action = exp.Compile();
                    }
                    else
                    {
                        var pt = ps[0].ParameterType;
                        var port = AddInport(m.Name, pt);

                        var at = actionGenericType.MakeGenericType(pt);
                        var param = Expression.Parameter(pt);
                        var exp = Expression.Lambda(
                            at,
                            Expression.Call(Expression.Constant(this), m, param),
                            param
                            );

                        port.data.action = exp.Compile();
                    }
                }
            }
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = f.GetCustomAttribute<IceprintPortAttribute>();
                if (attr == null) continue;

                var fName = f.Name;
                if (fName.StartsWith("out")) fName = fName.Substring(3);

                var at = f.FieldType;
                if (at == actionType)
                {
                    var port = AddOutport(f.Name);

                    var m = typeof(IceprintUtility).GetMethod("InvokeVoid");
                    var exp = Expression.Lambda(
                        actionType,
                        Expression.Call(null, m, Expression.Constant(port))
                        );

                    f.SetValue(this, exp.Compile());
                }
                else if (at.IsGenericType && at.GetGenericTypeDefinition().Equals(actionGenericType))
                {
                    var ps = at.GetGenericArguments();
                    if (ps.Length > 1) throw new Exception($"Invalid IceprintPort! {at.Name} {f.Name}");

                    var pt = ps[0];
                    var port = AddOutport(f.Name, pt);

                    var m = typeof(IceprintUtility).GetMethod("InvokeValue").MakeGenericMethod(pt);
                    var param = Expression.Parameter(pt);
                    var exp = Expression.Lambda(
                            at,
                            Expression.Call(null, m, Expression.Constant(port), param),
                            param);

                    f.SetValue(this, exp.Compile());
                }
                else throw new Exception($"Invalid IceprintPort! {at.Name} {f.Name}");
            }
        }
        #endregion
    }
}

namespace IceEngine
{
    [IceprintMenuItem("Test/asdas")]
    public class TestNode : IceprintNode
    {
        [IceprintPort]
        Action outOO;

        [IceprintPort]
        public void InTest()
        {
            Debug.Log("Yes!");
        }
        [IceprintPort]
        public void InTest2(float f)
        {
            Debug.Log($"No!{f}");
        }
    }

    [IceprintMenuItem("Test/UpdateNode"), RuntimeConst]
    public class UpdateNode : IceprintNode
    {
        [IceprintPort]
        public Action onUpdate;
    }

    [IceprintMenuItem("Test/TestMidNode")]
    public class TestMidNode : IceprintNode
    {
        [IceprintPort]
        Action<float> outGo2;

        [IceprintPort]
        public void InGo()
        {
            outGo2?.Invoke(3.232323f);
        }
    }
}
