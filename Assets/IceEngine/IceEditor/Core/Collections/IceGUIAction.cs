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

        MethodInfo Method => _method ??= _target == null ? null : _target.GetType().GetMethod(_methodName); MethodInfo _method;

        public System.Action Action => _action ??= _target == null ? null : () => Method.Invoke(_target, null); System.Action _action;
        public UnityAction UnityAction => _unityAction ??= _target == null ? null : () => Method.Invoke(_target, null); UnityAction _unityAction;

        public void Invoke() => UnityAction?.Invoke();

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
