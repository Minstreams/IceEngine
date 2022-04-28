using System.IO;
using UnityEditor;
using UnityEngine;
using IceEngine;

namespace IceEditor
{
    /// <summary>
    /// Global config for IceEditor
    /// </summary>
    public class IceEditorConfig : ScriptableObject
    {
        #region Sinleton Instance
        const string configPath = "Assets/IceEngine/IceEditor/Internal/IceEditorConfig.asset";
        /// <summary>
        /// Current editor config instance
        /// </summary>
        public static IceEditorConfig Config => _config != null ? _config : _config = AssetDatabase.LoadAssetAtPath<IceEditorConfig>(configPath); static IceEditorConfig _config;
        static IceEditorConfig CreateConfig()
        {
            _config = CreateInstance<IceEditorConfig>();
            AssetDatabase.CreateAsset(_config, configPath);
            return _config;
        }
        [SettingsProvider]
        static SettingsProvider GetSettingProvider() => IceGUI.GetSettingProvider<IceEditorConfig>("IceEngine/Editor", Config, CreateConfig);
        [SettingsProvider]
        static SettingsProvider GetRuntimeSettingProvider() => IceGUI.GetSettingProvider<IceConfig>("IceEngine/Runtime", IceConfig.Config, IceConfig.CreateConfig);
        #endregion

        #region Config Fields
        #endregion
    }
}