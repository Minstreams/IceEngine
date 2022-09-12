using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEditor.AnimatedValues;

using IceEngine;

namespace IceEditor
{
    /// <summary>
    /// 序列化的GUI临时数据
    /// </summary>
    [Serializable]
    public sealed class IceGUIAutoPack : ISerializationCallbackReceiver
    {
        /// <summary>
        /// 默认ThemeColor的GUI临时数据
        /// </summary>
        public IceGUIAutoPack(Action onAnimValueChange = null, Action onThemeColorChange = null) : this(IceGUIUtility.DefaultThemeColor, onAnimValueChange, onThemeColorChange) { }
        /// <summary>
        /// 序列化的GUI临时数据
        /// </summary>
        public IceGUIAutoPack(Color themeColor, Action onAnimValueChange = null, Action onThemeColorChange = null)
        {
            SetColor("ThemeColor", themeColor);
            _onAnimValueChange = onAnimValueChange;
            _onThemeColorChange = onThemeColorChange;
        }

        #region 主题颜色
        [SerializeField] IceGUIAction _onThemeColorChange;
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
            _stlSubAreaSeparator = null;
            _onThemeColorChange?.Invoke();
        }
        /// <summary>
        /// 主题颜色表达式
        /// </summary>
        public string ThemeColorExp => string.IsNullOrEmpty(_themeColorExp) ? _themeColorExp = $"#{ColorUtility.ToHtmlStringRGB(ThemeColor)}" : _themeColorExp; string _themeColorExp = null;
        #endregion

        #region 临时数据托管
        [SerializeField] IceGUIAction _onAnimValueChange;
        UnityAction OnAnimValueChange => _onAnimValueChange?.UnityAction;


        [SerializeField] internal IceDictionary<string, Color> _stringColorMap = new();
        public Color GetColor(string key) => GetColor(key, Color.white);
        public Color GetColor(string key, Color defaultVal) => _stringColorMap.TryGetValue(key, out Color val) ? val : _stringColorMap[key] = defaultVal;
        public Color SetColor(string key, Color value) => _stringColorMap[key] = value;


        [SerializeField] internal IceDictionary<string, bool> _stringBoolMap = new();
        public bool GetBool(string key, bool defaultVal = false) => _stringBoolMap.TryGetValue(key, out bool res) ? res : _stringBoolMap[key] = defaultVal;
        public bool SetBool(string key, bool value) => _stringBoolMap[key] = value;


        [SerializeField] internal IceDictionary<string, AnimBool> _stringAnimBoolMap = new();
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (OnAnimValueChange != null)
            {
                foreach (var ab in _stringAnimBoolMap.Values) ab.valueChanged.AddListener(OnAnimValueChange);
            }
        }
        public AnimBool GetAnimBool(string key, bool defaultVal = false)
        {
            if (_stringAnimBoolMap.TryGetValue(key, out AnimBool res)) return res;
            return _stringAnimBoolMap[key] = OnAnimValueChange != null ? new AnimBool(defaultVal, OnAnimValueChange) : new AnimBool(defaultVal);
        }
        public bool GetAnimBoolValue(string key, bool defaultVal = false) => GetAnimBool(key, defaultVal).value;
        public bool GetAnimBoolTarget(string key, bool defaultVal = false) => GetAnimBool(key, defaultVal).target;
        public float GetAnimBoolFaded(string key, bool defaultVal = false) => GetAnimBool(key, defaultVal).faded;
        public bool SetAnimBoolValue(string key, bool value) => GetAnimBool(key).value = value;
        public bool SetAnimBoolTarget(string key, bool value) => GetAnimBool(key).target = value;


        [SerializeField] internal IceDictionary<string, int> _stringIntMap = new();
        public int GetInt(string key, int defaultVal = 0) => _stringIntMap.TryGetValue(key, out int res) ? res : _stringIntMap[key] = defaultVal;
        public int SetInt(string key, int value) => _stringIntMap[key] = value;


        [SerializeField] internal IceDictionary<string, float> _stringFloatMap = new();
        public float GetFloat(string key, float defaultVal = 0) => _stringFloatMap.TryGetValue(key, out float res) ? res : _stringFloatMap[key] = defaultVal;
        public float SetFloat(string key, float value) => _stringFloatMap[key] = value;


        [SerializeField] internal IceDictionary<string, string> _stringStringMap = new();
        public string GetString(string key, string defaultVal = "") => _stringStringMap.TryGetValue(key, out string res) ? res : _stringStringMap[key] = defaultVal;
        public string SetString(string key, string value) => _stringStringMap[key] = value;


        [SerializeField] internal IceDictionary<int, Vector2> _intVec2Map = new();
        public Vector2 GetVector2(int key, Vector2 defaultVal = default) => _intVec2Map.TryGetValue(key, out Vector2 res) ? res : _intVec2Map[key] = defaultVal;
        public Vector2 SetVector2(int key, Vector2 value) => _intVec2Map[key] = value;


        [SerializeField] internal IceDictionary<string, Vector2> _stringVec2Map = new();
        public Vector2 GetVector2(string key, Vector2 defaultVal = default) => _stringVec2Map.TryGetValue(key, out Vector2 res) ? res : _stringVec2Map[key] = defaultVal;
        public Vector2 SetVector2(string key, Vector2 value) => _stringVec2Map[key] = value;


        [SerializeField] internal IceDictionary<string, Vector3> _stringVec3Map = new();
        public Vector3 GetVector3(string key, Vector3 defaultVal = default) => _stringVec3Map.TryGetValue(key, out Vector3 res) ? res : _stringVec3Map[key] = defaultVal;
        public Vector3 SetVector3(string key, Vector3 value) => _stringVec3Map[key] = value;


        [SerializeField] internal IceDictionary<string, Vector4> _stringVec4Map = new();
        public Vector4 GetVector4(string key, Vector4 defaultVal = default) => _stringVec4Map.TryGetValue(key, out Vector4 res) ? res : _stringVec4Map[key] = defaultVal;
        public Vector4 SetVector4(string key, Vector4 value) => _stringVec4Map[key] = value;


        [SerializeField] internal IceDictionary<string, Vector2Int> _stringVec2IntMap = new();
        public Vector2Int GetVector2Int(string key, Vector2Int defaultVal = default) => _stringVec2IntMap.TryGetValue(key, out Vector2Int res) ? res : _stringVec2IntMap[key] = defaultVal;
        public Vector2Int SetVector2Int(string key, Vector2Int value) => _stringVec2IntMap[key] = value;


        [SerializeField] internal IceDictionary<string, Vector3Int> _stringVec3IntMap = new();
        public Vector3Int GetVector3Int(string key, Vector3Int defaultVal = default) => _stringVec3IntMap.TryGetValue(key, out Vector3Int res) ? res : _stringVec3IntMap[key] = defaultVal;
        public Vector3Int SetVector3Int(string key, Vector3Int value) => _stringVec3IntMap[key] = value;
        #endregion

        #region 用到的样式
        public GUIStyle StlSectionHeader => _stlSectionHeader?.Check() ?? (_stlSectionHeader = IceGUIUtility.GetStlSectionHeader(ThemeColor)); GUIStyle _stlSectionHeader;
        public GUIStyle StlPrefix => _stlPrefix?.Check() ?? (_stlPrefix = IceGUIUtility.GetStlPrefix(ThemeColor)); GUIStyle _stlPrefix;
        public GUIStyle StlSubAreaSeparator => _stlSubAreaSeparator?.Check() ?? (_stlSubAreaSeparator = IceGUIUtility.GetStlSubAreaSeparator(ThemeColor)); GUIStyle _stlSubAreaSeparator;
        #endregion
    }
}