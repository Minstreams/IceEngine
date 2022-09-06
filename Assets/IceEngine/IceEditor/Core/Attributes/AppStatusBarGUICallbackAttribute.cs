using System;

namespace IceEditor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AppStatusBarGUICallbackAttribute : Attribute
    {
        public bool IsRight { get; private set; } = true;
        public AppStatusBarGUICallbackAttribute(bool bRight = true)
        {
            IsRight = bRight;
        }
    }
}
