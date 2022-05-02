using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.Internal
{
    /// <summary>
    /// 冰屿系统配置基类<br/>
    /// 子系统命名必须以Setting开头！参考<see cref="IceSystem{SubSetting}.TypeName"/>
    /// </summary>
    public abstract class IceSetting : ScriptableObject
    {
        /// <summary>
        /// 主题颜色
        /// </summary>
        internal virtual Color ThemeColor => IcePreference.Config.themeColor;
    }

    /// <summary>
    /// 冰屿系统的子系统基类，此类用于计算反射
    /// </summary>.
    public abstract class IceSystem { }

    /// <summary>
    /// 所有冰屿系统的子系统的父类。<br/>
    /// 子系统的特殊静态方法会被特定生命周期调用：
    /// <list type="bullet">
    /// <item><c>Awake()</c> 进入游戏调用，用于初始化系统</item>
    /// <item><c>Start()</c> 所有系统初始化完成开始调用，用于加载流程</item>
    /// <item><c>Quitting()</c> 退出游戏前调用，用于析构</item>
    /// </list>
    /// </summary>
    /// <typeparam name="SubSetting">子系统配置类，必须继承自IceSetting</typeparam>
    public abstract class IceSystem<SubSetting> : IceSystem where SubSetting : IceSetting
    {
        #region Config & Setting
        /// <summary>
        /// 配置
        /// </summary>
        public static SubSetting Setting => _Setting != null ? _Setting : (_Setting = Resources.Load<SubSetting>(TypeName)); static SubSetting _Setting;
        #endregion


        #region 协程控制
        /// <summary>
        /// 开始协程
        /// </summary>
        public static LinkedListNode<Coroutine> StartCoroutine(IEnumerator routine) => IceIsland.StartCoroutine(routine, typeof(SubSetting));
        /// <summary>
        /// 停止所有协程
        /// </summary>
        public static void StopAllCoroutines() => IceIsland.StopAllCoroutines(typeof(SubSetting));
        /// <summary>
        /// 停止指定协程
        /// </summary>
        public static void StopCoroutine(LinkedListNode<Coroutine> routine) => IceIsland.StopCoroutine(routine);
        #endregion


        #region Log & Dialog
        static string TypeName => _typeName ??= typeof(SubSetting).Name.Substring(7/*Setting的长度为7个字母*/); static string _typeName = null;
        static string DebugPrefix => $"【{TypeName.Color(Setting.ThemeColor)}】";
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(object message, Object context = null) => IceIsland.Log(message, context, DebugPrefix);
        public static void LogImportant(object message, Object context = null) => IceIsland.LogImportant(message, context, DebugPrefix);
        public static void LogWarning(object message, Object context = null) => IceIsland.LogWarning(message, context, DebugPrefix);
        public static void LogError(object message, Object context = null) => IceIsland.LogError(message, context, DebugPrefix);
        #endregion
    }
}
