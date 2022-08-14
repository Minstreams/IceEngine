using System;

namespace IceEngine
{
    /// <summary>
    /// 用于标注端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class IceprintInportAttribute : Attribute { }
}