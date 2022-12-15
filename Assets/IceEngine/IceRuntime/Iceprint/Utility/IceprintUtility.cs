﻿using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;

using IceEngine.Internal;
using System.Collections.Generic;

namespace IceEngine
{
    public static class IceprintUtility
    {
        internal static readonly Type[] unityEventTypes = new Type[]
        {
            typeof(UnityEvent),
            typeof(UnityEvent<>),
            typeof(UnityEvent<,>),
            typeof(UnityEvent<,,>),
            typeof(UnityEvent<,,,>),
        };
        internal static readonly Type[] unityActionTypes = new Type[]
        {
            typeof(UnityAction),
            typeof(UnityAction<>),
            typeof(UnityAction<,>),
            typeof(UnityAction<,,>),
            typeof(UnityAction<,,,>),
        };

        internal static MethodInfo[] OutportInvokeMethods
        {
            get
            {
                if (_outportInvokeMethods is null)
                {
                    _outportInvokeMethods = new MethodInfo[17];
                    var ms = typeof(IceprintOutport).GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var m in ms)
                    {
                        if (m.Name == "Invoke")
                        {
                            if (!m.IsGenericMethodDefinition)
                            {
                                _outportInvokeMethods[0] = m;
                            }
                            else
                            {
                                _outportInvokeMethods[m.GetGenericArguments().Length] = m;
                            }
                        }
                    }
                }
                return _outportInvokeMethods;
            }
        }
        static MethodInfo[] _outportInvokeMethods = null;

        static HashSet<Type> _nodeComponentTypeSet = null;
        internal static HashSet<Type> NodeComponentTypeSet
        {
            get
            {
                if (_nodeComponentTypeSet == null)
                {
                    _nodeComponentTypeSet = new HashSet<Type>();
                    var ts = UnityEditor.TypeCache.GetTypesWithAttribute<IceprintNodeAttribute>();
                    foreach (var t in ts) _nodeComponentTypeSet.Add(t);
                }
                return _nodeComponentTypeSet;
            }
        }
        public static bool IsIceprintNode(this MonoBehaviour self) => NodeComponentTypeSet.Contains(self.GetType());
    }
}
