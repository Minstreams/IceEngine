using UnityEditor;
using UnityEngine;

namespace IceEditor
{
    /// <summary>
    /// Global config for IceEditor
    /// </summary>
    public sealed class IceEditorConfig : ScriptableObject
    {
        #region Sinleton Instance
        const string configPath = "Assets/IceEngine/IceEditor/Internal/IceEditorConfig.asset";
        /// <summary>
        /// Current editor config instance
        /// </summary>
        public static IceEditorConfig Config => _config != null ? _config : _config = AssetDatabase.LoadAssetAtPath<IceEditorConfig>(configPath); static IceEditorConfig _config;
        public static IceEditorConfig CreateConfig()
        {
            _config = CreateInstance<IceEditorConfig>();
            AssetDatabase.CreateAsset(_config, configPath);
            return _config;
        }
        #endregion

        #region Config Fields
        #endregion
    }
}