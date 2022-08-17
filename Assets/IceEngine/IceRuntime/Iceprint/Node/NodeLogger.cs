using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Utility/Logger")]
    public class NodeLogger : IceprintNode
    {
        // Field
        public string defaultMessage;

        // Cache
        string Message => message ??= defaultMessage;
        [NonSerialized] string message = null;

        // Ports
        [IceprintPort]
        public void SetMessage(string message)
        {
            this.message = message;
        }
        [IceprintPort]
        public void Log()
        {
            Debug.Log(Message);
        }
        [IceprintPort]
        public void LogWarning()
        {
            Debug.LogWarning(Message);
        }
        [IceprintPort]
        public void LogError()
        {
            Debug.LogError(Message);
        }
    }
}
