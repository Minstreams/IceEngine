using System;
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
    /// <summary>
    /// 冰屿，IceEngine的核心
    /// </summary>
    public static partial class IceIsland
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
    }
}
