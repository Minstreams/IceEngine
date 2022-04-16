using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    public static partial class IceIsland
    {
        // TODO: 放到Editor Setting里？

        /// <summary>
        /// 主题颜色
        /// </summary>
        internal static Color ThemeColor => new Color(0, 1, 1);

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
                prefix = $"【{"IceIsland".Color(ThemeColor)}】";
                skipFrames = 2;
            }
            string log = $"{prefix}{mid}{message}";
            LogAction?.Invoke($"{log}\n\n{new System.Diagnostics.StackTrace(skipFrames, true)}");
            return log;
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(object message, Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, null, prefix);
#if UNITY_EDITOR
            if (HideLog) return;
#endif
            if (context == null) Debug.Log(log);
            else Debug.Log(log, context);
        }
        public static void LogImportant(object message, Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, "[<color=#0FF>Important</color>]", prefix);

            if (context == null) Debug.Log(log);
            else Debug.Log(log, context);
        }
        public static void LogWarning(object message, Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, "[<color=#FC0>Warning</color>]", prefix);

            if (context == null) Debug.LogWarning(log);
            else Debug.LogWarning(log, context);
        }
        public static void LogError(object message, Object context = null, string prefix = null)
        {
            var log = _ProcessLog(message, "[<color=#F00>Error</color>]", prefix);

            if (context == null) Debug.LogError(log);
            else Debug.LogError(log, context);
        }
    }
}
