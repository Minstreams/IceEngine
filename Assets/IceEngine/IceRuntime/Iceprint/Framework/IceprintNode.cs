﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using UnityEngine;
using UnityEngine.Events;

using IceEngine.Internal;

using static IceEngine.IceprintUtility;

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
        protected IceprintInport AddInport(string name, params Type[] paramTypes)
        {
            var port = new IceprintInport
            {
                ParamsList = new(paramTypes),
                name = name,
                node = this,
                id = inports.Count,
            };
            port.data.nodeId = id;
            port.data.portId = port.id;
            inports.Add(port);
            return port;
        }
        protected IceprintOutport AddOutport(string name, params Type[] paramTypes)
        {
            var port = new IceprintOutport
            {
                ParamsList = new(paramTypes),
                name = name,
                node = this,
                id = outports.Count,
            };
            if (connectionData.Count == port.id) connectionData.Add(port.connectedPorts);
            outports.Add(port);
            return port;
        }
        protected void InitializePorts(Type type, object instance)
        {
            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<IceprintPortAttribute>();
                if (attr is null) continue;

                // TODO: 支持一下模板函数
                if (m.IsGenericMethodDefinition)
                {
                    throw new Exception($"Iceprint Inport can not be generic for now! node:[{type.FullName}] port:[{m.Name}]");
                }

                // 这里解析Inport数据
                // 支持一下默认参数
                {
                    var ps = m.GetParameters();
                    int psCount = ps.Length;

                    if (psCount > 16) throw new Exception($"Iceprint Inport can not have more than 16 parameter! node:[{type.FullName}] port:[{m.Name}]");

                    if (psCount == 0)
                    {
                        var port = AddInport(m.Name);

#if UNITY_EDITOR
                        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                            if (instance != null)
                            {
                                var exp = Expression.Lambda(
                                    IceCoreUtility.actionTypes[0],
                                    Expression.Call(Expression.Constant(instance), m)
                                    );

                                port.data.action = exp.Compile();
                            }
                    }
                    else
                    {
                        var pts = ps.Select(p => p.ParameterType).ToArray();
                        var port = AddInport(m.Name, pts);

#if UNITY_EDITOR
                        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                            if (instance != null)
                            {
                                var at = IceCoreUtility.actionTypes[psCount].MakeGenericType(pts);
                                var prs = pts.Select(pt => Expression.Parameter(pt)).ToArray();
                                var exp = Expression.Lambda(
                                    at,
                                    Expression.Call(Expression.Constant(instance), m, prs),
                                    prs
                                    );

                                port.data.action = exp.Compile();
                            }
                    }
                }
            }
            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = f.GetCustomAttribute<IceprintPortAttribute>();
                if (attr is null) continue;

                var fName = f.Name;
                var labelAttr = f.GetCustomAttribute<LabelAttribute>();
                if (labelAttr != null) fName = labelAttr.Label;
                else if (fName.StartsWith("on")) fName = fName.Substring(2);

                var at = f.FieldType;
                bool isUnityEvent = false;
                if (at.IsGenericType)
                {
                    // 有参数
                    var pts = at.GetGenericArguments();
                    int psCount = pts.Length;

                    if (psCount < unityEventTypes.Length && at.GetGenericTypeDefinition().Equals(unityEventTypes[psCount]))
                    {
                        isUnityEvent = true;
                    }
                    else if (psCount < IceCoreUtility.actionTypes.Length && at.GetGenericTypeDefinition().Equals(IceCoreUtility.actionTypes[psCount]))
                    {
                        isUnityEvent = false;
                    }
                    else throw new Exception($"Invalid IceprintPort! {at.Name} {f.Name}");

                    var port = AddOutport(fName, pts);

#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                        if (instance != null)
                        {
                            var m = OutportInvokeMethods[psCount].MakeGenericMethod(pts);
                            var prs = pts.Select(pt => Expression.Parameter(pt)).ToArray();
                            var tAction = isUnityEvent ? unityActionTypes[psCount].MakeGenericType(at.GetGenericArguments()) : at;
                            var exp = Expression.Lambda(
                                    tAction,
                                    Expression.Call(Expression.Constant(port), m, prs),
                                    prs);

                            if (isUnityEvent) at.GetMethod("AddListener", BindingFlags.Instance | BindingFlags.Public).Invoke(f.GetValue(instance), new object[] { exp.Compile() });
                            else f.SetValue(instance, exp.Compile());
                        }
                }
                else
                {
                    // 无参数
                    if (at != IceCoreUtility.actionTypes[0])
                    {
                        if (unityEventTypes[0].IsAssignableFrom(at)) isUnityEvent = true;
                        else throw new Exception($"Invalid IceprintPort! {at.Name} {f.Name}");
                    }

                    var port = AddOutport(fName);

#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                        if (instance != null)
                        {
                            var m = OutportInvokeMethods[0];
                            var exp = Expression.Lambda(
                                isUnityEvent ? unityActionTypes[0] : IceCoreUtility.actionTypes[0],
                                Expression.Call(Expression.Constant(port), m)
                                );

                            if (isUnityEvent) (f.GetValue(instance) as UnityEvent).AddListener(exp.Compile() as UnityAction);
                            else f.SetValue(instance, exp.Compile());
                        }
                }
            }
        }
        static readonly Type nodeFieldType = typeof(NodeField);
        protected void InitializeAllFields(Type type, object instance)
        {
            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (nodeFieldType.IsAssignableFrom(f.FieldType))
                {
                    var nf = f.GetValue(instance) as NodeField;
                    nf.Initialize(this);
                }
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// 初始化时调用，此时没有连接，有id
        /// </summary>
        public virtual void Initialize()
        {
            var t = GetType();
            InitializeAllFields(t, this);
            InitializePorts(t, this);
        }
        #endregion
    }
}
