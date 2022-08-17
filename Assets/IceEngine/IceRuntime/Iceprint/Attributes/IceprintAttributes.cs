using System;

namespace IceEngine
{
    /// <summary>
    /// 用于标注端口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class IceprintPortAttribute : Attribute { }

    /// <summary>
    /// 标记一个IceprintNode的菜单路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IceprintMenuItemAttribute : Attribute
    {
        public string Path { get; set; }
        public IceprintMenuItemAttribute(string path) { Path = path; }
    }
}