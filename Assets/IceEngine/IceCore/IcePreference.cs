using UnityEditor;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// Global config for runtime preference
    /// </summary>
    public class IcePreference : ScriptableObject
    {
        #region Sinleton Instance
        /// <summary>
        /// Current config instance
        /// </summary>
        public static IcePreference Config => _config != null ? _config : _config = Resources.Load<IcePreference>("IcePreference"); static IcePreference _config;
        public static IcePreference CreateConfig()
        {
            _config = CreateInstance<IcePreference>();
            AssetDatabase.CreateAsset(_config, "Assets/IceEngine/Resources/IcePreference.asset");
            return _config;
        }
        #endregion

        #region Config Fields
        public Color themeColor = new Color(1, 0.6f, 0);
        #endregion
    }
}