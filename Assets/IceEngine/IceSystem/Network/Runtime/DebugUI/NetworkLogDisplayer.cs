using System.Collections.Generic;
using UnityEngine;
using IceEngine.DebugUI;

namespace IceEngine.Networking.Internal
{
    [AddComponentMenu("[Network]/UI/NetworkLogDisplayer")]
    public class NetworkLogDisplayer : UIBase
    {
        public int maxLogCount = 8;
        public GUIStyle labelStyle;

        readonly Queue<string> logQueue = new Queue<string>();

        void OnLog(string message)
        {
            logQueue.Enqueue(message);
            while (logQueue.Count > maxLogCount) logQueue.Dequeue();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            Ice.Island.LogAction += OnLog;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            Ice.Island.LogAction -= OnLog;
        }
        protected override void Reset()
        {
            base.Reset();
            SetStyle(ref labelStyle, "label");
        }
        protected override void OnUI()
        {
            if (logQueue.Count == 0)
            {
                GUILayout.Label("Network Log Displayer", labelStyle);
            }
            else
            {
                foreach (var msg in logQueue)
                {
                    GUILayout.Label(msg, labelStyle);
                }
            }
        }
    }
}
