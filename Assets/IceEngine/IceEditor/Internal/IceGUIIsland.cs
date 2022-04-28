using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.AnimatedValues;
using IceEngine;

namespace IceEditor.Internal
{
    /// <summary>
    /// 序列化的GUI临时数据
    /// </summary>
    [System.Serializable]
    internal class IceGUIIsland
    {
        /// <summary>
        /// 默认ThemeColor的GUI临时数据
        /// </summary>
        public IceGUIIsland() : this(IceConfig.Config.themeColor, null) { }
        /// <summary>
        /// 序列化的GUI临时数据
        /// </summary>
        public IceGUIIsland(Color themeColor, UnityAction onAnimValueChange)
        {
            SetColor("ThemeColor", themeColor);
            _onAnimValueChange = onAnimValueChange;
        }

        #region 主题颜色
        /// <summary>
        /// 主题颜色
        /// </summary>
        public Color ThemeColor
        {
            get => GetColor("ThemeColor");
            set
            {
                if (ThemeColor != value)
                {
                    SetColor("ThemeColor", value);
                    RefreshThemeColor();
                }
            }
        }
        /// <summary>
        /// 手动刷新颜色表达式和样式
        /// </summary>
        public void RefreshThemeColor()
        {
            _themeColorExp = null;
            _stlSectionHeader = null;
            _stlPrefix = null;
            _stlSeparator = null;
            _stlSeparatorOn = null;
        }
        /// <summary>
        /// 主题颜色表达式
        /// </summary>
        public string ThemeColorExp => string.IsNullOrEmpty(_themeColorExp) ? _themeColorExp = $"#{ColorUtility.ToHtmlStringRGB(ThemeColor)}" : _themeColorExp; string _themeColorExp = null;
        #endregion

        #region 临时数据托管
        UnityAction _onAnimValueChange;

        [SerializeField] IceDictionary<string, Color> _stringColorMap = new IceDictionary<string, Color>();
        public Color GetColor(string key, Color defaultVal) => _stringColorMap.TryGetValue(key, out Color val) ? val : _stringColorMap[key] = defaultVal;
        public Color GetColor(string key) => GetColor(key, Color.white);
        public Color SetColor(string key, Color value) => _stringColorMap[key] = value;


        [SerializeField] IceDictionary<string, bool> _stringBoolMap = new IceDictionary<string, bool>();
        public bool GetBool(string key, bool defaultVal = false) => _stringBoolMap.TryGetValue(key, out bool res) ? res : _stringBoolMap[key] = defaultVal;
        public bool SetBool(string key, bool value) => _stringBoolMap[key] = value;


        [SerializeField] IceDictionary<string, AnimBool> _stringAnimBoolMap = new IceDictionary<string, AnimBool>();
        public AnimBool GetAnimBool(string key, bool defaultVal = false)
        {
            if (_stringAnimBoolMap.TryGetValue(key, out AnimBool res))
            {
                if (_onAnimValueChange != null && res.valueChanged.GetPersistentEventCount() == 0) res.valueChanged.AddListener(_onAnimValueChange);
                return res;
            }
            return _stringAnimBoolMap[key] = _onAnimValueChange != null ? new AnimBool(defaultVal, _onAnimValueChange) : new AnimBool(defaultVal);
        }
        public bool GetAnimBoolValue(string key, bool defaultVal = false) => GetAnimBool(key, defaultVal).value;
        public bool GetAnimBoolTarget(string key, bool defaultVal = false) => GetAnimBool(key, defaultVal).target;
        public float GetAnimBoolFaded(string key, bool defaultVal = false) => GetAnimBool(key, defaultVal).faded;
        public bool SetAnimBoolValue(string key, bool value) => GetAnimBool(key).value = value;
        public bool SetAnimBoolTarget(string key, bool value) => GetAnimBool(key).target = value;


        [SerializeField] IceDictionary<string, int> _stringIntMap = new IceDictionary<string, int>();
        public int GetInt(string key, int defaultVal = 0) => _stringIntMap.TryGetValue(key, out int res) ? res : _stringIntMap[key] = defaultVal;
        public int SetInt(string key, int value) => _stringIntMap[key] = value;


        [SerializeField] IceDictionary<string, float> _stringFloatMap = new IceDictionary<string, float>();
        public float GetFloat(string key, float defaultVal = 0) => _stringFloatMap.TryGetValue(key, out float res) ? res : _stringFloatMap[key] = defaultVal;
        public float SetFloat(string key, float value) => _stringFloatMap[key] = value;


        [SerializeField] IceDictionary<string, string> _stringStringMap = new IceDictionary<string, string>();
        public string GetString(string key, string defaultVal = "") => _stringStringMap.TryGetValue(key, out string res) ? res : _stringStringMap[key] = defaultVal;
        public string SetString(string key, string value) => _stringStringMap[key] = value;


        [SerializeField] IceDictionary<string, Vector2> _stringVec2Map = new IceDictionary<string, Vector2>();
        public Vector2 GetVector2(string key) => GetVector2(key, Vector2.zero);
        public Vector2 GetVector2(string key, Vector2 defaultVal) => _stringVec2Map.TryGetValue(key, out Vector2 res) ? res : _stringVec2Map[key] = defaultVal;
        public Vector2 SetVector2(string key, Vector2 value) => _stringVec2Map[key] = value;


        [SerializeField] IceDictionary<string, Vector3> _stringVec3Map = new IceDictionary<string, Vector3>();
        public Vector3 GetVector3(string key) => GetVector3(key, Vector3.zero);
        public Vector3 GetVector3(string key, Vector3 defaultVal) => _stringVec3Map.TryGetValue(key, out Vector3 res) ? res : _stringVec3Map[key] = defaultVal;
        public Vector3 SetVector3(string key, Vector3 value) => _stringVec3Map[key] = value;


        [SerializeField] IceDictionary<string, Vector4> _stringVec4Map = new IceDictionary<string, Vector4>();
        public Vector4 GetVector4(string key) => GetVector4(key, Vector4.zero);
        public Vector4 GetVector4(string key, Vector4 defaultVal) => _stringVec4Map.TryGetValue(key, out Vector4 res) ? res : _stringVec4Map[key] = defaultVal;
        public Vector4 SetVector4(string key, Vector4 value) => _stringVec4Map[key] = value;

        // TODO: 添加Vector234Int类型
        #endregion

        #region 用到的样式
        public GUIStyle StlSectionHeader => _stlSectionHeader?.Check() ?? (_stlSectionHeader = IceGUI.GetStlSectionHeader(ThemeColor)); GUIStyle _stlSectionHeader;
        public GUIStyle StlPrefix => _stlPrefix?.Check() ?? (_stlPrefix = IceGUI.GetStlPrefix(ThemeColor)); GUIStyle _stlPrefix;
        public GUIStyle StlSeparator => _stlSeparator?.Check() ?? (_stlSeparator = IceGUI.GetStlSeparator(ThemeColor)); GUIStyle _stlSeparator;
        public GUIStyle StlSeparatorOn => _stlSeparatorOn?.Check() ?? (_stlSeparatorOn = IceGUI.GetStlSeparatorOn(ThemeColor)); GUIStyle _stlSeparatorOn;
        #endregion
    }
}