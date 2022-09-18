using System;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace IceEngine.Framework
{
    namespace Internal
    {
        /// <summary>
        /// 冰屿系统运行时系统配置的基类，此类用于获取反射信息
        /// </summary>
        public abstract class IceSetting : ScriptableObject
        {
            // 默认配置项
            public Color themeColor = new Color(1, 0.6f, 0);
        }
    }

    /// <summary>
    /// 冰屿系统运行时系统配置的基类，文件存于Resources目录下，自动化的单例功能，并提供Project菜单中配置窗口
    /// 配置类命名必须以Setting开头！参考<see cref="IceSystem.TypeName"/>
    /// 可以通过IceSettingPathAttribute来配置资源存储的目录
    /// </summary>
    public abstract class IceSetting<T> : Internal.IceSetting where T : ScriptableObject
    {
        static T _setting;
        public static T Setting
        {
            get
            {
                if (_setting == null)
                {
                    // 先尝试加载已有的
                    var tT = typeof(T);
                    var tName = tT.Name;
                    _setting = Resources.Load(tName) as T;

                    // 若没有再创建或抛异常
                    if (_setting == null)
                    {
#if UNITY_EDITOR
                        // 编辑时创建

                        // 计算path
                        string filePath = "Assets";

                        var path = tT.GetCustomAttribute<IceSettingPathAttribute>()?.Path;
                        if (!string.IsNullOrEmpty(path)) filePath += $"/{path}";

                        filePath += $"/Resources/{tName}.asset";

                        // 创建目录
                        filePath.TryCreateFolderOfPath();

                        // 创建资源
                        _setting = CreateInstance<T>();
                        UnityEditor.AssetDatabase.CreateAsset(_setting, filePath);
#else            
                        // 运行时直接抛异常
                        throw new Exception($"{typeof(T).FullName}的Setting资源不存在！");
#endif
                    }
                }

                return _setting;
            }
        }
    }
}