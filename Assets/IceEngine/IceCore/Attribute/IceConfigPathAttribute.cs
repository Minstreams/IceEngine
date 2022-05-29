using System;

namespace IceEngine
{
    /// <summary>
    /// 标记运行时系统配置的路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IceConfigPathAttribute : Attribute
    {
        /// <summary>
        /// 标记运行时系统配置的路径
        /// </summary>
        /// <param name="path">资源父目录相对Assets的路径</param>
        public IceConfigPathAttribute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// 资源父目录相对Assets的路径
        /// </summary>
        public string Path { get; set; }
    }
}