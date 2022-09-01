using System;

namespace IceEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ToolbarGUICallbackAttribute : Attribute
    {
        public ToolbarGUIPosition Position { get; private set; }
        public ToolbarGUICallbackAttribute(ToolbarGUIPosition position)
        {
            Position = position;
        }
    }
}
