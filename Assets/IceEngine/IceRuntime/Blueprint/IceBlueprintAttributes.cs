using System;

namespace IceEngine.Blueprint
{
    /// <summary>
    /// 用于标注端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PortAttribute : Attribute { }
}