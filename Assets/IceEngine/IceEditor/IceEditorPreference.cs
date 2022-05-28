using UnityEditor;
using UnityEngine;

namespace IceEditor
{
    public abstract class IceEditorConfig<T> : ScriptableObject
    {
    }

    /// <summary>
    /// Global config for IceEditor
    /// </summary>
    public sealed class IceEditorPreference : ScriptableObject
    {
        #region Sinleton Instance
        const string configPath = "Assets/IceEngine/IceEditor/Internal/Config/IceEditorConfig.asset";
        /// <summary>
        /// Current editor config instance
        /// </summary>
        public static IceEditorPreference Config => _config != null ? _config : _config = AssetDatabase.LoadAssetAtPath<IceEditorPreference>(configPath); static IceEditorPreference _config;
        public static IceEditorPreference CreateConfig()
        {
            _config = CreateInstance<IceEditorPreference>();
            AssetDatabase.CreateAsset(_config, configPath);
            return _config;
        }
        [SettingsProvider] static SettingsProvider GetSettingProvider() => IceGUIUtility.GetSettingProvider("Preferences/IceEngine/1", "Editor", IceEditorPreference.Config, IceEditorPreference.CreateConfig);
        #endregion

        #region Config Fields
        #endregion
    }
}