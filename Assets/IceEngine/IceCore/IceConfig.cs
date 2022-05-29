using System;
using System.IO;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// 运行时系统配置的基类，文件存于Resources目录下，自动化的单例功能，并提供Project菜单中配置窗口
    /// 可以通过IceConfigAttribute来配置资源存储的目录
    /// </summary>
    public abstract class IceConfig<T> : ScriptableObject where T : ScriptableObject
    {
        static T _config;
        public static T Config
        {
            get
            {
                if (_config == null)
                {
                    // 先尝试加载已有的
                    var tT = typeof(T);
                    var tName = tT.Name;
                    _config = Resources.Load(tName) as T;

                    // 若没有再创建或抛异常
                    if (_config == null)
                    {
#if UNITY_EDITOR
                        // 编辑时创建

                        // 计算path
                        string filePath = "Assets";
                        var ats = tT.GetCustomAttributes(typeof(IceConfigPathAttribute), false);
                        if (ats.Length > 0)
                        {
                            var path = (ats[0] as IceConfigPathAttribute).Path;
                            if (!string.IsNullOrEmpty(path))
                            {
                                filePath += $"/{path}";
                            }
                        }
                        filePath += $"/Resources/{tName}.asset";

                        // 创建目录
                        string directoryName = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

                        // 创建资源
                        _config = CreateInstance<T>();
                        UnityEditor.AssetDatabase.CreateAsset(_config, filePath);
#else            
                        // 运行时直接抛异常
                        throw new Exception($"{typeof(T).FullName}的Config资源不存在！");
#endif
                    }
                }

                return _config;
            }
        }
    }
}