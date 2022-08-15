using System;

namespace IceEngine
{
    /// <summary>
    /// 用于标注端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class IceprintInportAttribute : Attribute { }

    /// <summary>
    /// 标记一个IceprintNode的菜单路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IceprintMenuItem : Attribute
    {
        public string Path { get; set; }
        public IceprintMenuItem(string path) { Path = path; }
    }
}