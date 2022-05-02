using UnityEditor;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// Global config for runtime project
    /// </summary>
    public class IceConfig : ScriptableObject
    {
        #region Sinleton Instance
        /// <summary>
        /// Current config instance
        /// </summary>
        public static IceConfig Config => _config != null ? _config : _config = Resources.Load<IceConfig>("IceConfig"); static IceConfig _config;
        public static IceConfig CreateConfig()
        {
            _config = CreateInstance<IceConfig>();
            AssetDatabase.CreateAsset(_config, "Assets/IceEngine/Resources/IceConfig.asset");
            return _config;
        }
        #endregion

        #region Config Fields

        #endregion
    }
}