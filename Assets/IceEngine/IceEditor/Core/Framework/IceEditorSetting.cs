using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEditorInternal;

using IceEngine;

namespace IceEditor.Framework
{
    namespace Internal
    {
        /// <summary>
        /// 冰屿系统编辑时系统配置的基类，此类用于获取反射信息
        /// </summary>
        public abstract class IceEditorSetting : ScriptableObject
        {
            // 默认配置项
        }
    }
    /// <summary>
    /// 冰屿系统编辑时系统配置的基类，文件存于自定义目录下，自动化的单例功能，并提供Project菜单中配置窗口
    /// 配置类命名必须以EditorSetting开头！
    /// 可以通过IceSettingPathAttribute来配置资源存储的目录
    /// </summary>
    public abstract class IceEditorSetting<T> : Internal.IceEditorSetting where T : ScriptableObject
    {
        static T _setting;
        public static T Setting
        {
            get
            {
                if (_setting == null)
                {
                    // 先尝试加载已有的
                    string filePath = GetPath();
                    InternalEditorUtility.LoadSerializedFileAndForget(filePath);

                    // 若没有再创建
                    if (_setting == null)
                    {
                        // 创建目录
                        filePath.TryCreateFolderOfPath();

                        // 创建资源
                        _setting = CreateInstance<T>();
                    }
                }
                return _setting;
            }
        }

        static bool justLoaded = false;
        protected IceEditorSetting()
        {
            if (_setting != null)
            {
                Debug.LogError($"{typeof(T).Name} already exists. Did you query the singleton in a constructor?");
                return;
            }
            _setting = (this as T);
            justLoaded = true;
        }

        static string GetPath()
        {
            var tT = typeof(T);

            var path = tT.GetCustomAttribute<IceSettingPathAttribute>()?.Path;
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith("Assets")) Debug.LogError($"{tT.Name}的路径不能放到Assets下!");
                else return $"{path}/{tT.Name}.asset";
            }

            // 默认路径
            return $"IceEditorSettings/{tT.Name}.asset";
        }

        void OnValidate()
        {
            // 刚载入时不保存
            if (justLoaded)
            {
                justLoaded = false;
                return;
            }
            Save();
        }
        public void Save()
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { _setting }, GetPath(), true);
        }
    }
}