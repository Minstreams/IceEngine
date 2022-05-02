﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using IceEngine.Internal;

namespace IceEngine
{
    using IceCoroutine = LinkedListNode<Coroutine>;

    /// <summary>
    /// 冰屿，IceEngine的核心
    /// </summary>
    public static class IceIsland
    {
        /// <summary>
        /// 场景中的组件实例
        /// </summary>
        internal static IceIslandComponent Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    LogError("编辑时非法调用运行时代码！");
                    return null;
                }
                if (_instance == null)
                {
                    throw new System.NullReferenceException("Ice Island Instance 为空！");
                }
#endif
                return _instance;
            }
        }
        static IceIslandComponent _instance = null;

        #region 子系统控制
        /// <summary>
        /// 子系统表
        /// </summary>
        public static List<Type> SubSystemList => _subsystemList ??= Assembly.GetAssembly(typeof(IceIsland)).GetTypes().Where(t => !t.IsGenericType && t.IsSubclassOf(typeof(IceSystem))).ToList(); static List<Type> _subsystemList;
        /// <summary>
        /// 调用所有子系统上存在的同名静态方法
        /// </summary>
        public static void CallSubSystem(string methodName)
        {
            foreach (var s in SubSystemList) s.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }
        #endregion

        #region 生命周期控制
        /// <summary>
        /// 子系统在Quitting方法中设置此项为false可阻止应用关闭
        /// </summary>
        internal static bool CanQuit { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            _instance = new GameObject("Ice Island").AddComponent<IceIslandComponent>();
            UnityEngine.Object.DontDestroyOnLoad(_instance);
            {
                // Awake之后，Start之前的额外初始化操作
                Application.wantsToQuit += Quitting;
                CallSubSystem("LateAwake");
            }
        }
        static bool Quitting()
        {
            CanQuit = true;
            CallSubSystem("Quitting");
            return CanQuit;
        }
        #endregion

        #region 协程控制
        static readonly Dictionary<System.Type, LinkedList<Coroutine>> _routineMap = new Dictionary<System.Type, LinkedList<Coroutine>>();

        /// <summary>
        /// 开始协程
        /// </summary>
        /// <param name="routine">协程</param>
        /// <param name="key">作为索引的类型</param>
        public static IceCoroutine StartCoroutine(IEnumerator routine, System.Type key)
        {
            static IEnumerator SubCoroutine(IEnumerator inCoroutine, IceCoroutine node)
            {
                yield return inCoroutine;
                node.List.Remove(node);
            }

            if (!_routineMap.TryGetValue(key, out var linkedList))
            {
                _routineMap[key] = linkedList = new LinkedList<Coroutine>();
            }
            var node = new IceCoroutine(null);
            node.Value = Instance.StartCoroutine(SubCoroutine(routine, node));
            linkedList.AddLast(node);

            return node;
        }
        /// <summary>
        /// 停止一个类型的所有协程
        /// </summary>
        /// <param name="key">作为索引的类型</param>
        public static void StopAllCoroutines(System.Type key)
        {
            if (!_routineMap.TryGetValue(key, out var linkedList)) return;

            foreach (Coroutine c in linkedList)
            {
                Instance.StopCoroutine(c);
            }

            linkedList.Clear();
        }
        /// <summary>
        /// 停止一个StartCoroutine开始的协程
        /// </summary>
        /// <param name="node">要停止的协程对象</param>
        public static void StopCoroutine(IceCoroutine node)
        {
            if (node == null || node.List == null) return;
            Instance.StopCoroutine(node.Value);
            node.List.Remove(node);
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        /// <summary>
        /// 是否隐藏普通Log
        /// </summary>
        public static bool HideLog { get; set; } = false;
#endif

        /// <summary>
        /// 全局Log事件
        /// </summary>
        public static event System.Action<string> LogAction;
        static string _ProcessLog(object message, string mid, string prefix)
        {
            int skipFrames = 3;
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = $"【{"IceIsland".Color(IcePreference.Config.themeColor)}】";
                skipFrames = 2;
            }
            string log = $"{prefix}{mid}{message}";
            LogAction?.Invoke($"{log}\n\n{new System.Diagnostics.StackTrace(skipFrames, true)}");
            return log;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(object message, UnityEngine.Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, null, prefix);
#if UNITY_EDITOR
            if (HideLog) return;
#endif
            if (context == null) Debug.Log(log);
            else Debug.Log(log, context);
        }
        public static void LogImportant(object message, UnityEngine.Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, "[<color=#0FF>Important</color>]", prefix);

            if (context == null) Debug.Log(log);
            else Debug.Log(log, context);
        }
        public static void LogWarning(object message, UnityEngine.Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, "[<color=#FC0>Warning</color>]", prefix);

            if (context == null) Debug.LogWarning(log);
            else Debug.LogWarning(log, context);
        }
        public static void LogError(object message, UnityEngine.Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, "[<color=#F00>Error</color>]", prefix);

            if (context == null) Debug.LogError(log);
            else Debug.LogError(log, context);
        }
        #endregion
    }
}
