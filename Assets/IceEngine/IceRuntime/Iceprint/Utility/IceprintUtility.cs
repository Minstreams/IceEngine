﻿using System;
using System.Linq.Expressions;
using IceEngine.Framework;

namespace IceEngine
{
    public static class IceprintUtility
    {
        internal readonly static Type actionType = typeof(Action);
        internal readonly static Type actionGenericType = typeof(Action<>);

        public static void InvokeVoid(this IceprintOutport port)
        {
            foreach (var pd in port.connectedPorts) (pd.action as Action)?.Invoke();
        }
        public static void InvokeValue<T>(this IceprintOutport port, T value)
        {
            foreach (var pd in port.connectedPorts)
            {
                if (pd.action is Action act) act?.Invoke();
                else (pd.action as Action<T>)?.Invoke(value);
            }
        }
    }
}
