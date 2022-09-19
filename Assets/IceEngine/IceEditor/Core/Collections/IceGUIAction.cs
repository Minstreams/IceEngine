using System.Linq.Expressions;
using System.Reflection;

using UnityEngine;
using UnityEngine.Events;

namespace IceEditor
{
    /// <summary>
    /// 序列化的Action
    /// </summary>
    [System.Serializable]
    public sealed class IceGUIAction
    {
        [SerializeField] Object _target;
        [SerializeField] string _methodName;

        MethodInfo Method => _method ??= _target == null ? null : _target.GetType().GetMethod(_methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); MethodInfo _method;

        public System.Action Action => _action ??= _target == null ? null : Expression.Lambda<System.Action>(Expression.Call(Expression.Constant(_target), Method)).Compile(); [System.NonSerialized] System.Action _action;
        public UnityAction UnityAction => _unityAction ??= _target == null ? null : Expression.Lambda<UnityAction>(Expression.Call(Expression.Constant(_target), Method)).Compile(); [System.NonSerialized] UnityAction _unityAction;

        public void Invoke() => Action?.Invoke();

        public IceGUIAction(System.Action action)
        {
            _target = action.Target as Object;
            _method = action.Method;
            _methodName = action.Method.Name;

            _action = action;
        }

        public IceGUIAction(UnityAction action)
        {
            _target = action.Target as Object;
            _method = action.Method;
            _methodName = action.Method.Name;

            _unityAction = action;
        }

        public static implicit operator IceGUIAction(System.Action action) => action == null ? null : new IceGUIAction(action);
    }
}
