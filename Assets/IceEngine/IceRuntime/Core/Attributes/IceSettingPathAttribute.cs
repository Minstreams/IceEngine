using System;

namespace IceEngine
{
    /// <summary>
    /// 标记系统配置的路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IceSettingPathAttribute : Attribute
    {
        /// <summary>
        /// 标记系统配置的路径
        /// </summary>
        /// <param name="path">资源父目录相对Assets的路径</param>
        public IceSettingPathAttribute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// 资源父目录路径。（运行时为相对Assets的路径，编辑时为相对Project的路径）
        /// </summary>
        public string Path { get; set; }
    }
}