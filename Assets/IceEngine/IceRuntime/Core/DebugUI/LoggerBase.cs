using System.Collections.Generic;
using UnityEngine;
using IceEngine.DebugUI;
using System.Text.RegularExpressions;

namespace IceEngine.DebugUI.Internal
{
    public abstract class LoggerBase : UIBase
    {
        #region Fields
        public int maxLogCount = 128;
        public GUIStyle labelStyle;
        #endregion
        protected abstract string RegexExpr { get; }

        readonly Queue<string> logQueue = new Queue<string>();
        readonly Queue<string> logQueueFull = new Queue<string>();
        Regex Reg => _reg ??= new Regex(RegexExpr); Regex _reg = null;
        bool bShowFullLog = false;
        bool ProcessLog(string message, out string res)
        {
            res = "";
            Match mat = Reg.Match(message);
            bool success = mat.Success;
            if (success) res = mat.Groups[0].Value;

            return success;
        }
        void OnLog(string message)
        {
            if (ProcessLog(message, out string res))
            {
                logQueue.Enqueue(res);
                while (logQueue.Count > maxLogCount) logQueue.Dequeue();

                logQueueFull.Enqueue(message);
                while (logQueueFull.Count > maxLogCount) logQueueFull.Dequeue();
            }
        }
        void Awake()
        {
            Ice.Island.LogAction += OnLog;
        }
        void OnDestroy()
        {
            Ice.Island.LogAction -= OnLog;
        }
        protected override void Reset()
        {
            base.Reset();
            SetStyle(ref labelStyle, "label");
        }
        protected override void OnUI()
        {
            using (HORIZONTAL)
            {
                Title(GetType().Name);
                if (Button(bShowFullLog ? "完整日志" : "基本日志")) bShowFullLog = !bShowFullLog;
            }

            using (ScrollVertical("Main Scroll"))
            {
                if (bShowFullLog)
                {
                    foreach (var msg in logQueueFull)
                    {
                        Label(msg, labelStyle);
                    }
                }
                else
                {
                    foreach (var msg in logQueue)
                    {
                        Label(msg, labelStyle);
                    }
                }
            }
        }
    }
}
