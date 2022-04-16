using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using IceEngine;
using System.Linq;

namespace IceEditor
{
    /// <summary>
    /// 包含基础功能的窗口类
    /// </summary>
    public abstract class IceEditorWindow : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        #region 【配置】

        #region 核心配置
        /// <summary>
        /// 默认标题
        /// </summary>
        public abstract GUIContent TitleContent { get; }
        /// <summary>
        /// GUI界面代码
        /// </summary>
        protected abstract void OnWindowGUI(Rect position);
        /// <summary>
        /// Debug界面代码
        /// </summary>
        protected abstract void OnDebugGUI(Rect position);
        #endregion

        #region 可选配置
        /// <summary>
        /// 重载此字符串来改变上下文菜单中“Debug模式”
        /// </summary>
        protected virtual string DebugModeName => "Debug";
        /// <summary>
        ///  Debug模式的默认标题
        /// </summary>
        public virtual GUIContent TitleContentDebug => new GUIContent(TitleContent) { text = TitleContent.text + $" - {DebugModeName}" };
        /// <summary>
        /// 重载此方法来添加自定义上下文菜单
        /// </summary>
        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"{DebugModeName}模式"), DebugMode, () => DebugMode = !DebugMode);
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/右"), _debugUIOrientation == UIOrientation.Right, () => { _debugUIOrientation = UIOrientation.Right; });
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/左"), _debugUIOrientation == UIOrientation.Left, () => { _debugUIOrientation = UIOrientation.Left; });
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/下"), _debugUIOrientation == UIOrientation.Bottom, () => { _debugUIOrientation = UIOrientation.Bottom; });
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/上"), _debugUIOrientation == UIOrientation.Top, () => { _debugUIOrientation = UIOrientation.Top; });
        }
        protected virtual void OnEnable()
        {
            RefreshTitleContent();
        }
        /// <summary>
        /// 额外的GUI（无Layout）
        /// </summary>
        protected virtual void OnExtraGUI(Rect position) { }
        /// <summary>
        /// 默认主题颜色
        /// </summary>
        protected virtual Color DefaultThemeColor => new Color(1, 0.6f, 0);
        /// <summary>
        /// 是否在 DebugUI 中显示 StatusUI
        /// </summary>
        protected virtual bool HasStatusUI => true;
        /// <summary>
        /// 反序列化事件
        /// </summary>
        public virtual void OnAfterDeserialize() => RefreshThemeColor();
        /// <summary>
        /// 序列化事件
        /// </summary>
        public virtual void OnBeforeSerialize() { }
        #endregion

        #endregion

        #region 【接口】

        #region 标题
        /// <summary>
        /// 当前标题内容，隐藏了基类titleContent
        /// </summary>
        public new GUIContent titleContent { get => base.titleContent; }
        /// <summary>
        /// 手动刷新标题
        /// </summary>
        protected void RefreshTitleContent()
        {
            base.titleContent = DebugMode ? TitleContentDebug : TitleContent;
        }
        #endregion

        #region Debug模式
        /// <summary>
        /// Debug模式
        /// </summary>
        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                if (_debugMode != value)
                {
                    _debugMode = value;
                    RefreshTitleContent();
                    Repaint();
                }
            }
        }
        [SerializeField] bool _debugMode;
        public enum UIOrientation
        {
            Right,
            Left,
            Bottom,
            Top,
        }
        /// <summary>
        /// DebugUI方向
        /// </summary>
        public UIOrientation DebugUIOrientation { get => _debugUIOrientation; set => _debugUIOrientation = value; }
        [SerializeField] UIOrientation _debugUIOrientation = UIOrientation.Right;
        #endregion

        #region 主题颜色
        /// <summary>
        /// 主题颜色
        /// </summary>
        public Color ThemeColor
        {
            get => GetColor("ThemeColor", DefaultThemeColor);
            set
            {
                if (ThemeColor != value)
                {
                    SetColor("ThemeColor", value);
                    _themeColorExp = null;
                    _stlSectionHeader = null;
                    _stlPrefix = null;
                    _stlSeparator = null;
                    _stlSeparatorOn = null;
                }
            }
        }
        /// <summary>
        /// 手动刷新颜色表达式和样式
        /// </summary>
        protected void RefreshThemeColor()
        {
            var c = ThemeColor;
            Log(string.Format("RefreshThemeColor ({0:0.000}, {1:0.000}, {2:0.000})", c.r, c.g, c.b));
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

        #region Log & Dialog
        /// <summary>
        /// 输出一个Debug模式下才会显示的Log
        /// </summary>
        protected void Log(string text, UnityEngine.Object context = null)
        {
            if (!DebugMode) return;
            text = $"【{titleContent.text.Color(ThemeColorExp)}】{text}";
            if (context == null) Debug.Log(text);
            else Debug.Log(text, context);
        }
        /// <summary>
        /// 输出一个普通模式下也会显示的Log
        /// </summary>
        protected void LogImportant(string text, UnityEngine.Object context = null)
        {
            text = $"【{titleContent.text.Color(ThemeColorExp)}】{text.Color(ThemeColorExp)}";
            if (context == null) Debug.Log(text);
            else Debug.Log(text, context);
        }
        /// <summary>
        /// 输出一个Warning
        /// </summary>
        /// <param name="text"></param>
        protected void LogWarning(string text, UnityEngine.Object context = null)
        {
            text = $"【{titleContent.text.Color(ThemeColorExp)}】{text}";
            if (context == null) Debug.LogWarning(text);
            else Debug.LogWarning(text, context);
        }
        /// <summary>
        /// 输出一个Error
        /// </summary>
        protected void LogError(string text, UnityEngine.Object context = null)
        {
            text = $"【{titleContent.text.Color(ThemeColorExp)}】{text.Color("#F00")}";
            if (context == null) Debug.LogError(text);
            else Debug.LogError(text, context);
        }
        /// <summary>
        /// 断言
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        protected void Assert(bool condition, string text = "Assertion failed", UnityEngine.Object context = null)
        {
            if (condition) return;
            text = $"【{titleContent.text.Color(ThemeColorExp)}】{text}";
            if (context == null) Debug.LogAssertion(text);
            else Debug.LogAssertion(text, context);
        }
        /// <summary>
        /// 显示一个Dialog对话框
        /// </summary>
        /// <param name="message">对话框中文本内容</param>
        /// <param name="ok">确定的文本</param>
        /// <param name="cancel">取消的文本</param>
        /// <returns>点击确定时返回true</returns>
        protected bool Dialog(string message, string ok = "确定", string cancel = "取消") => EditorUtility.DisplayDialog(titleContent.text, message, ok, cancel);
        /// <summary>
        /// 显示一个没有取消按钮的Dialog对话框
        /// </summary>
        /// <param name="message">对话框中文本内容</param>
        /// <param name="ok">确定的文本</param>
        /// <returns>点击确定时返回true</returns>
        protected bool DialogNoCancel(string message, string ok = "确定") => EditorUtility.DisplayDialog(titleContent.text, message, ok);
        /// <summary>
        /// 显示并更新一个ProgressBar
        /// </summary>
        /// <param name="info">显示的文字</param>
        /// <param name="progress">进度（0到1）</param>
        protected void UpdateProgressBar(string info, float progress) => EditorUtility.DisplayProgressBar(titleContent.text, info, progress);
        /// <summary>
        /// 清除ProgressBar
        /// </summary>
        protected void ClearProgressBar() => EditorUtility.ClearProgressBar();
        #endregion

        #region 临时数据托管
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
                if (res.valueChanged.GetPersistentEventCount() == 0) res.valueChanged.AddListener(Repaint);
                return res;
            }
            return _stringAnimBoolMap[key] = new AnimBool(defaultVal, Repaint);
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


        [SerializeField] IceDictionary<int, Vector2> _intVec2Map = new IceDictionary<int, Vector2>();
        public Vector2 GetVector2(int key) => GetVector2(key, Vector2.zero);
        public Vector2 GetVector2(int key, Vector2 defaultVal) => _intVec2Map.TryGetValue(key, out Vector2 res) ? res : _intVec2Map[key] = defaultVal;
        public Vector2 SetVector2(int key, Vector2 value) => _intVec2Map[key] = value;

        #endregion

        #endregion

        #region 【GUI Shortcut】

        #region 用到的样式
        protected static GUIStyle StlEmpty => _stlEmpty?.Check() ?? (_stlEmpty = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), }); static GUIStyle _stlEmpty;
        protected GUIStyle StlBackground => _stlBackground?.Check() ?? (_stlBackground = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), stretchHeight = true, }); GUIStyle _stlBackground;
        protected GUIStyle StlHeader => _stlHeader?.Check() ?? (_stlHeader = new GUIStyle("LODRendererRemove") { margin = new RectOffset(5, 3, 6, 2), fontSize = 12, fontStyle = FontStyle.Normal, richText = true, }); GUIStyle _stlHeader;
        protected GUIStyle StlSectionHeader => _stlSectionHeader?.Check() ?? (_stlSectionHeader = new GUIStyle("AnimationEventTooltip")
        {
            padding = new RectOffset(1, 8, 2, 2),
            overflow = new RectOffset(24, 0, 0, 0),
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            imagePosition = ImagePosition.ImageLeft,
            contentOffset = new Vector2(0f, 0f),
            stretchWidth = false,
        }.Initialize(stl =>
        {
            stl.normal.textColor = new Color(0.8396226f, 0.8396226f, 0.8396226f);
            stl.hover.textColor = new Color(1, 1, 1);
            stl.hover.background = stl.normal.background;
            stl.onNormal.textColor = ThemeColor;
            stl.onNormal.background = stl.normal.background;
            stl.onHover.textColor = ThemeColor * 1.2f;
            stl.onHover.background = stl.normal.background;
        })); GUIStyle _stlSectionHeader;
        protected GUIStyle StlDock => _stlDock?.Check() ?? (_stlDock = new GUIStyle("dockarea") { padding = new RectOffset(1, 1, 1, 1), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlDock;
        protected GUIStyle StlLabel => _stlLabel?.Check() ?? (_stlLabel = new GUIStyle("label") { richText = true, wordWrap = true }); GUIStyle _stlLabel;
        protected GUIStyle StlPrefix => _stlPrefix?.Check() ?? (_stlPrefix = new GUIStyle("PrefixLabel") { margin = new RectOffset(3, 3, 2, 2), padding = new RectOffset(1, 1, 0, 0), alignment = TextAnchor.MiddleLeft, richText = true, }.Initialize(stl => { stl.focused.textColor = stl.active.textColor = stl.onNormal.textColor = stl.onActive.textColor = ThemeColor; stl.onNormal.background = stl.active.background; })); GUIStyle _stlPrefix;
        protected GUIStyle StlGroup => _stlGroup?.Check() ?? (_stlGroup = new GUIStyle("NotificationBackground") { border = new RectOffset(16, 16, 13, 13), margin = new RectOffset(6, 6, 6, 6), padding = new RectOffset(10, 10, 6, 6), }); GUIStyle _stlGroup;
        protected GUIStyle StlBox => _stlBox?.Check() ?? (_stlBox = new GUIStyle("window") { margin = new RectOffset(4, 4, 4, 4), padding = new RectOffset(6, 6, 6, 6), contentOffset = new Vector2(0f, 0f), stretchWidth = false, stretchHeight = false, }); GUIStyle _stlBox;
        protected GUIStyle StlNode => _stlNode?.Check() ?? (_stlNode = new GUIStyle("flow node 0") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlNode;
        protected GUIStyle StlHighlight => _stlHighlight?.Check() ?? (_stlHighlight = new GUIStyle("LightmapEditorSelectedHighlight") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 10, 10), overflow = new RectOffset(0, 0, 0, 0), }); GUIStyle _stlHighlight;
        protected GUIStyle StlButton => _stlButton?.Check() ?? (_stlButton = new GUIStyle("LargeButton") { richText = true, }); GUIStyle _stlButton;
        protected GUIStyle StlError => _stlError?.Check() ?? (_stlError = new GUIStyle("Wizard Error") { border = new RectOffset(32, 0, 32, 0), padding = new RectOffset(32, 0, 7, 7), fixedHeight = 0f, }.Initialize(stl => { stl.normal.textColor = new Color(1f, 0.8469602f, 0f); })); GUIStyle _stlError;
        int GetThemeColorHueIndex()
        {
            Color.RGBToHSV(ThemeColor, out float h, out float s, out _);
            if (s < 0.3f) return 0;
            if (h < 0.06f) return 6;
            if (h < 0.13f) return 5;
            if (h < 0.19f) return 4;
            if (h < 0.46f) return 3;
            if (h < 0.52f) return 2;
            if (h < 0.84f) return 1;
            return 6;
        }
        protected GUIStyle StlSeparator => _stlSeparator?.Check() ?? (_stlSeparator = new GUIStyle($"flow node {GetThemeColorHueIndex() }")); GUIStyle _stlSeparator;
        protected GUIStyle StlSeparatorOn => _stlSeparatorOn?.Check() ?? (_stlSeparatorOn = new GUIStyle($"flow node {GetThemeColorHueIndex()} on")); GUIStyle _stlSeparatorOn;
        protected GUIStyle StlIce => _stlIce?.Check() ?? (_stlIce = new GUIStyle("BoldTextField") { padding = new RectOffset(3, 3, 2, 2), fontSize = 11, richText = true, fixedHeight = 0f, stretchWidth = false, imagePosition = ImagePosition.ImageLeft, fontStyle = FontStyle.Normal }); GUIStyle _stlIce;
        #endregion

        #region Drawing Element
        /// <summary>画一个标题</summary>
        protected void Header(string text) => GUILayout.Label(text.Color(ThemeColorExp), StlHeader);
        /// <summary>画一个节标题</summary>
        protected bool SectionHeader(string key, bool defaultVal = true, string labelOverride = null, params GUILayoutOption[] options)
        {
            var ab = GetAnimBool(key, defaultVal);
            return ab.target = GUILayout.Toggle(ab.target, string.IsNullOrEmpty(labelOverride) ? key : labelOverride, StlSectionHeader);
        }
        /// <summary>用特定Style填充区域</summary>
        protected void Box(Rect rect, GUIStyle style) => GUI.Label(rect, GUIContent.none, style);
        /// <summary>用特定Style填充区域</summary>
        protected Rect Box(GUIStyle style, params GUILayoutOption[] options)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, style, options);
            GUI.Label(rect, GUIContent.none, style);
            return rect;
        }
        /// <summary>用特定Style填充区域</summary>
        protected Rect Box(float width, float height, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var rect = GUILayoutUtility.GetRect(width, height, style, options);
            GUI.Label(rect, GUIContent.none, style);
            return rect;
        }
        /// <summary>画一个支持RichText的Label</summary>
        protected void Label(string text, params GUILayoutOption[] options) => GUILayout.Label(text, StlLabel, options);
        /// <summary>画一个支持RichText的Label</summary>
        protected void Label(GUIContent content, params GUILayoutOption[] options) => GUILayout.Label(content, StlLabel, options);
        /// <summary>画一个自定义Label</summary>
        protected void Label(string text, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Label(text, style, options);
        /// <summary>画一个自定义Label</summary>
        protected void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Label(content, style, options);
        /// <summary>画一个Error</summary>
        protected void LabelError(string text) => GUILayout.Label(text, StlError);
        /// <summary>画一个Texture Preview</summary>
        /// <param name="rect">尺寸</param>
        protected void TextureBox(Texture texture, Rect rect) => EditorGUI.DrawPreviewTexture(rect, texture);
        /// <summary>画一个自适应Layout的Texture</summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        protected void TextureBox(Texture texture, float width, float height) => EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false)), texture);
        /// <summary>画一个自适应Layout的Texture</summary>
        /// <param name="expanded">是否自动扩展</param>
        protected void TextureBox(Texture texture, bool expanded = false) => EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width)), texture);
        /// <summary>画一个自适应Layout的透明Texture</summary>
        /// <param name="expanded">是否自动扩展</param>
        protected void TextureBoxTransparent(Texture texture, bool expanded = false) => EditorGUI.DrawTextureTransparent(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width)), texture);
        /// <summary>画一个自适应Layout的Texture，自带控制项</summary>
        protected void TextureBoxComplex(Texture texture)
        {
            var name = texture.name;
            ColorWriteMask cwMask = ColorWriteMask.All;

            using (VERTICAL) using (NODE)
            {
                using (HORIZONTAL)
                {
                    IceToggle($"Expanded {name}", false, "Exp", "Is this texture Expanded");
                    /*if (!texture.graphicsFormat.ToString().Contains("UNorm"))*/
                    using (HORIZONTAL)
                    {
                        ColorWriteMask mask = (ColorWriteMask)0;
                        if (IceToggle($"Color Write Mask R {name}", true, "R")) mask |= ColorWriteMask.Red;
                        if (IceToggle($"Color Write Mask G {name}", true, "G")) mask |= ColorWriteMask.Green;
                        if (IceToggle($"Color Write Mask B {name}", true, "B")) mask |= ColorWriteMask.Blue;
                        if (IceToggle($"Color Write Mask A {name}", true, "A")) mask |= ColorWriteMask.Alpha;
                        if (mask != 0) cwMask = mask;
                    }
                    GUILayout.Label($"{texture.width} : {texture.height} | {(texture is Texture2D t2 ? t2.format.ToString() : texture is RenderTexture rt ? rt.format.ToString() : texture.GetType().ToString())}");
                    GUILayout.FlexibleSpace();
                    IntSliderNoLabel($"Mip Level {name}", 0, 0, texture.mipmapCount - 1);
                }

                using (Horizontal(StlIce))
                {
                    bool expanded = GetBool($"Expanded {name}");
                    if (!expanded) GUILayout.FlexibleSpace();
                    var rect = GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width >> GetInt($"Mip Level {name}")));
                    if (!expanded) GUILayout.FlexibleSpace();
                    if (Event.current.type == EventType.Repaint) EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit, 0, GetInt($"Mip Level {name}"), cwMask);
                }
            }
        }
        protected static void Space(float pixels) => GUILayout.Space(pixels);
        protected static void Space() => GUILayout.FlexibleSpace();
        #endregion

        #region Button & Field
        static readonly GUILayoutOption[] defaultOptions = new GUILayoutOption[] { GUILayout.MinWidth(EditorGUIUtility.singleLineHeight) };
        static GUILayoutOption[] CheckOptions(ref GUILayoutOption[] options) => options = options.Length > 0 ? options : defaultOptions;

        protected bool Button(string text, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Button(text, styleOverride ?? StlButton, options);
        protected bool Button(string text, params GUILayoutOption[] options) => Button(text, null, options);
        protected bool Button(GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Button(content, styleOverride ?? StlButton, options);
        protected bool Button(GUIContent content, params GUILayoutOption[] options) => Button(content, null, options);

        protected bool _Toggle(bool val, GUIStyle styleOverride, params GUILayoutOption[] options) => ControlLabelScope.HasLabel ? EditorGUILayout.Toggle(ControlLabelScope.Label, val, styleOverride ?? "toggle") : EditorGUILayout.Toggle(val, styleOverride ?? "toggle");
        protected bool _Toggle(bool val, params GUILayoutOption[] options) => _Toggle(val, null, options);
        protected bool _Toggle(string label, bool val, GUIStyle styleOverride, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Toggle(val, styleOverride, options); }
        protected bool _Toggle(string label, bool val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Toggle(val, options); }

        protected bool Toggle(ref bool val, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _Toggle(val, styleOverride, options);
        protected bool Toggle(ref bool val, params GUILayoutOption[] options) => val = _Toggle(val, options);
        protected bool Toggle(string label, ref bool val, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _Toggle(label, val, styleOverride, options);
        protected bool Toggle(string label, ref bool val, params GUILayoutOption[] options) => val = _Toggle(label, val, options);
        protected bool ToggleNoLabel(string key, bool defaultVal = false, GUIStyle styleOverride = null, params GUILayoutOption[] options) => SetBool(key, _Toggle(GetBool(key, defaultVal), styleOverride, options));
        protected bool Toggle(string key, bool defaultVal = false, string labelOverride = null, GUIStyle styleOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return ToggleNoLabel(key, defaultVal, styleOverride, options); }

        protected bool _ToggleLeft(bool val, string text, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Toggle(val, text, styleOverride ?? "toggle", options);
        protected bool _ToggleLeft(bool val, string text, params GUILayoutOption[] options) => GUILayout.Toggle(val, text, "toggle", options);
        protected bool _ToggleLeft(bool val, GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Toggle(val, content, styleOverride ?? "toggle", options);
        protected bool _ToggleLeft(bool val, GUIContent content, params GUILayoutOption[] options) => GUILayout.Toggle(val, content, "toggle", options);
        protected bool ToggleLeft(ref bool val, string text, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _ToggleLeft(val, text, styleOverride, options);
        protected bool ToggleLeft(ref bool val, string text, params GUILayoutOption[] options) => val = _ToggleLeft(val, text, options);
        protected bool ToggleLeft(ref bool val, GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _ToggleLeft(val, content, styleOverride, options);
        protected bool ToggleLeft(ref bool val, GUIContent content, params GUILayoutOption[] options) => val = _ToggleLeft(val, content, options);
        protected bool ToggleLeft(string key, bool defaultVal = false, string textOverride = null, GUIStyle styleOverride = null, params GUILayoutOption[] options) => SetBool(key, _ToggleLeft(GetBool(key, defaultVal), string.IsNullOrEmpty(textOverride) ? key : textOverride, styleOverride, options));

        protected bool ToggleButton(string text, bool on, GUIStyle styleOverride, params GUILayoutOption[] options) => on != _ToggleLeft(on, text, styleOverride ?? StlButton, options);
        protected bool ToggleButton(string text, bool on, params GUILayoutOption[] options) => ToggleButton(text, on, null, options);
        protected bool ToggleButton(GUIContent content, bool on, GUIStyle styleOverride, params GUILayoutOption[] options) => on != _ToggleLeft(on, content, styleOverride ?? StlButton, options);
        protected bool ToggleButton(GUIContent content, bool on, params GUILayoutOption[] options) => ToggleButton(content, on, null, options);
        protected bool ToggleButton(Rect rect, string text, bool on, GUIStyle style) => on != GUI.Toggle(rect, on, text, style);

        protected int _IntField(int val, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedIntField(ControlLabelScope.Label, val, options) : EditorGUILayout.DelayedIntField(val, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.IntField(ControlLabelScope.Label, val, options) : EditorGUILayout.IntField(val, options)); }
        protected int _IntField(string label, int val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _IntField(val, options); }
        protected int IntField(ref int val, params GUILayoutOption[] options) => val = _IntField(val, options);
        protected int IntField(string label, ref int val, params GUILayoutOption[] options) => val = _IntField(label, val, options);
        protected int IntFieldNoLabel(string key, int defaultVal = 0, params GUILayoutOption[] options) => SetInt(key, _IntField(GetInt(key, defaultVal), options));
        protected int IntField(string key, int defaultVal = 0, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return IntFieldNoLabel(key, defaultVal, options); }

        protected int _IntSlider(int val, int min, int max, params GUILayoutOption[] options) { CheckOptions(ref options); return ControlLabelScope.HasLabel ? (int)EditorGUILayout.Slider(ControlLabelScope.Label, val, min, max, options) : (int)EditorGUILayout.Slider(val, min, max, options); }
        protected int _IntSlider(string label, int val, int min, int max, params GUILayoutOption[] options) { using (ControlLabel(label)) return _IntSlider(val, min, max, options); }
        protected int IntSlider(ref int val, int min, int max, params GUILayoutOption[] options) => val = _IntSlider(val, min, max, options);
        protected int IntSlider(string label, ref int val, int min, int max, params GUILayoutOption[] options) => val = _IntSlider(label, val, min, max, options);
        protected int IntSliderNoLabel(string key, int defaultVal, int min, int max, params GUILayoutOption[] options) => SetInt(key, _IntSlider(GetInt(key, defaultVal), min, max, options));
        protected int IntSlider(string key, int defaultVal, int min, int max, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return IntSliderNoLabel(key, defaultVal, min, max, options); }
        protected int IntSliderNoLabel(string key, int min, int max, params GUILayoutOption[] options) => SetInt(key, _IntSlider(GetInt(key), min, max, options));
        protected int IntSlider(string key, int min, int max, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return IntSliderNoLabel(key, min, max, options); }

        protected float _FloatField(float val, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedFloatField(ControlLabelScope.Label, val, options) : EditorGUILayout.DelayedFloatField(val, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.FloatField(ControlLabelScope.Label, val, options) : EditorGUILayout.FloatField(val, options)); }
        protected float _FloatField(string label, float val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _FloatField(val, options); }
        protected float FloatField(ref float val, params GUILayoutOption[] options) => val = _FloatField(val, options);
        protected float FloatField(string label, ref float val, params GUILayoutOption[] options) => val = _FloatField(label, val, options);
        protected float FloatFieldNoLabel(string key, float defaultVal = 0, params GUILayoutOption[] options) => SetFloat(key, _FloatField(GetFloat(key, defaultVal), options));
        protected float FloatField(string key, float defaultVal = 0, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return FloatFieldNoLabel(key, defaultVal, options); }

        protected float _Slider(float val, float min = 0, float max = 1, params GUILayoutOption[] options) { CheckOptions(ref options); return ControlLabelScope.HasLabel ? EditorGUILayout.Slider(ControlLabelScope.Label, val, min, max, options) : EditorGUILayout.Slider(val, min, max, options); }
        protected float _Slider(string label, float val, float min = 0, float max = 1, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Slider(val, min, max, options); }
        protected float Slider(ref float val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Slider(val, min, max, options);
        protected float Slider(string label, ref float val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Slider(label, val, min, max, options);
        protected float SliderNoLabel(string key, float defaultVal, float min = 0, float max = 1, params GUILayoutOption[] options) => SetFloat(key, _Slider(GetFloat(key, defaultVal), min, max, options));
        protected float Slider(string key, float defaultVal, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return SliderNoLabel(key, defaultVal, min, max, options); }
        protected float SliderNoLabel(string key, float min = 0, float max = 1, params GUILayoutOption[] options) => SetFloat(key, _Slider(GetFloat(key), min, max, options));
        protected float Slider(string key, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return SliderNoLabel(key, min, max, options); }

        protected string _TextField(string val, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedTextField(ControlLabelScope.Label, val, options) : EditorGUILayout.DelayedTextField(val, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.TextField(ControlLabelScope.Label, val, options) : EditorGUILayout.TextField(val, options)); }
        protected string _TextField(string label, string val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _TextField(val, options); }
        protected string TextField(ref string val, params GUILayoutOption[] options) => val = _TextField(val, options);
        protected string TextField(string label, ref string val, params GUILayoutOption[] options) => val = _TextField(label, val, options);
        protected string TextFieldNoLabel(string key, string defaultVal = "", params GUILayoutOption[] options) => SetString(key, _TextField(GetString(key, defaultVal), options));
        protected string TextField(string key, string defaultVal = "", string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return TextFieldNoLabel(key, defaultVal, options); }

        protected Vector2 _Vector2Field(Vector2 val, string xLabel = null, string yLabel = null)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                const string controlNameX = "Vector2FieldX";
                GUI.SetNextControlName(controlNameX);
                if (!string.IsNullOrEmpty(xLabel))
                {
                    using (LabelWidth(StlPrefix.CalcSize(new GUIContent(xLabel)).x)) using (ControlLabel(xLabel)) FloatField(ref val.x);
                }
                else
                {
                    val.x = DelayedScope.inScope ? EditorGUILayout.DelayedFloatField(val.x, defaultOptions) : EditorGUILayout.FloatField(val.x, defaultOptions);
                }

                const string controlNameY = "Vector2FieldY";
                GUI.SetNextControlName(controlNameY);
                if (!string.IsNullOrEmpty(yLabel))
                {
                    using (LabelWidth(StlPrefix.CalcSize(new GUIContent(yLabel)).x)) using (ControlLabel(yLabel)) FloatField(ref val.y);
                }
                else
                {
                    val.y = DelayedScope.inScope ? EditorGUILayout.DelayedFloatField(val.y, defaultOptions) : EditorGUILayout.FloatField(val.y, defaultOptions);
                }

                if (ControlLabelScope.HasLabel)
                {
                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlNameX || focusedControl == controlNameY;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, ControlLabelScope.Label, on, StlPrefix) && !on) GUI.FocusControl(controlNameX);
                }
            }
            return val;
        }
        protected Vector2 _Vector2Field(string label, Vector2 val, string xLabel = null, string yLabel = null) { using (ControlLabel(label)) return _Vector2Field(val, xLabel, yLabel); }
        protected Vector2 Vector2Field(ref Vector2 val, string xLabel = null, string yLabel = null) => val = _Vector2Field(val, xLabel, yLabel);
        protected Vector2 Vector2Field(string label, ref Vector2 val, string xLabel = null, string yLabel = null) => val = _Vector2Field(label, val, xLabel, yLabel);
        protected Vector2 Vector2FieldNoLabel(string key, Vector2 defaultVal, string xLabel = null, string yLabel = null) => SetVector2(key, _Vector2Field(GetVector2(key, defaultVal), xLabel, yLabel));
        protected Vector2 Vector2Field(string key, Vector2 defaultVal, string labelOverride = null, string xLabel = null, string yLabel = null) { using (ControlLabel(key, labelOverride)) return Vector2FieldNoLabel(key, defaultVal, xLabel, yLabel); }
        protected Vector2 Vector2FieldNoLabel(string key, string xLabel = null, string yLabel = null) => SetVector2(key, _Vector2Field(GetVector2(key), xLabel, yLabel));
        protected Vector2 Vector2Field(string key, string labelOverride = null, string xLabel = null, string yLabel = null) { using (ControlLabel(key, labelOverride)) return Vector2FieldNoLabel(key, xLabel, yLabel); }

        protected void MinMaxSlider(ref float l, ref float r, float min = 0, float max = 1, params GUILayoutOption[] options) { CheckOptions(ref options); EditorGUILayout.MinMaxSlider(ref l, ref r, min, max, options); }
        protected Vector2 _Vector2Slider(Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                const string controlNameX = "Vector2SliderX";
                if (ControlLabelScope.HasLabel)
                {
                    GUI.SetNextControlName(controlNameX);
                    val.x = EditorGUILayout.FloatField(val.x, GUILayout.Width(48), GUILayout.MinWidth(EditorGUIUtility.singleLineHeight));
                }

                const string controlName = "Vector2Slider";
                GUI.SetNextControlName(controlName);
                using (CHANGECHECK)
                {
                    EditorGUILayout.MinMaxSlider(ref val.x, ref val.y, min, max, options.Length > 0 ? options : new GUILayoutOption[] { GUILayout.MinWidth(-4) });

                    if (Changed) GUI.FocusControl(controlName);
                }

                if (ControlLabelScope.HasLabel)
                {
                    const string controlNameY = "Vector2SliderY";
                    GUI.SetNextControlName(controlNameY);
                    val.y = EditorGUILayout.FloatField(val.y, GUILayout.Width(48), GUILayout.MinWidth(EditorGUIUtility.singleLineHeight));

                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlNameX || focusedControl == controlName || focusedControl == controlNameY;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, ControlLabelScope.Label, on, StlPrefix) && !on) GUI.FocusControl(controlName);
                }
            }
            return val;
        }
        protected Vector2 _Vector2Slider(string label, Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Vector2Slider(val, min, max, options); }
        protected Vector2 Vector2Slider(ref Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Vector2Slider(val, min, max, options);
        protected Vector2 Vector2Slider(string label, ref Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Vector2Slider(label, val, min, max, options);
        protected Vector2 Vector2SliderNoLabel(string key, Vector2 defaultVal, float min = 0, float max = 1, params GUILayoutOption[] options) => SetVector2(key, _Vector2Slider(GetVector2(key, defaultVal), min, max, options));
        protected Vector2 Vector2Slider(string key, Vector2 defaultVal, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return Vector2SliderNoLabel(key, defaultVal, min, max, options); }
        protected Vector2 Vector2SliderNoLabel(string key, float min = 0, float max = 1, params GUILayoutOption[] options) => SetVector2(key, _Vector2Slider(GetVector2(key), min, max, options));
        protected Vector2 Vector2Slider(string key, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return Vector2SliderNoLabel(key, min, max, options); }

        /// <summary>
        /// 不带预览功能的 Texture2D Field
        /// </summary>
        /// <returns>return true on changed</returns>
        protected bool TextureFieldNoPreview(string label, ref Texture2D tex, params GUILayoutOption[] options)
        {
            CheckOptions(ref options);
            using (CHANGECHECK)
            {
                using (ControlLabel(label)) tex = (Texture2D)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(options), label, tex, typeof(Texture2D), false);
                return Changed;
            }
        }
        /// <summary>
        /// Texture2D Field
        /// </summary>
        /// <returns>return true on changed</returns>
        protected bool TextureField(string label, ref Texture2D tex, params GUILayoutOption[] options)
        {
            CheckOptions(ref options);
            bool res = false;
            var previewOn = GetAnimBool($"{label} Preview On");
            using (HORIZONTAL)
            {
                Rect labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                const string controlName = "TextureField";
                GUI.SetNextControlName(controlName);
                using (CHANGECHECK)
                {
                    tex = (Texture2D)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(options), tex, typeof(Texture2D), false);
                    res = Changed;
                }

                {
                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlName;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, label, on, StlPrefix))
                    {
                        GUI.FocusControl(controlName);
                        if (Event.current.button != 0)
                        {
                            GenericMenu gm = new GenericMenu();
                            gm.AddItem(new GUIContent("预览"), previewOn.target, () => previewOn.target = !previewOn.target);
                            gm.ShowAsContext();
                        }
                    }
                }
            }
            if (tex != null) using (var fs = new FolderScope(previewOn)) if (fs.visible) TextureBoxComplex(tex);

            return res;
        }

        protected EnumType _EnumPopup<EnumType>(EnumType val, params GUILayoutOption[] options) where EnumType : Enum { CheckOptions(ref options); return ControlLabelScope.HasLabel ? (EnumType)EditorGUILayout.EnumPopup(ControlLabelScope.Label, val, options) : (EnumType)EditorGUILayout.EnumPopup(val, options); }
        protected EnumType _EnumPopup<EnumType>(string label, EnumType val, params GUILayoutOption[] options) where EnumType : Enum { using (ControlLabel(label)) return _EnumPopup(val, options); }
        protected EnumType EnumPopup<EnumType>(ref EnumType val, params GUILayoutOption[] options) where EnumType : Enum => val = _EnumPopup(val, options);
        protected EnumType EnumPopup<EnumType>(string label, ref EnumType val, params GUILayoutOption[] options) where EnumType : Enum => val = _EnumPopup(label, val, options);

        protected ObjType _ObjectField<ObjType>(ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object { CheckOptions(ref options); return ControlLabelScope.HasLabel ? (ObjType)EditorGUILayout.ObjectField(ControlLabelScope.Label, val, typeof(ObjType), allowSceneObjects, options) : (ObjType)EditorGUILayout.ObjectField(val, typeof(ObjType), allowSceneObjects, options); }
        protected ObjType _ObjectField<ObjType>(string label, ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object { using (ControlLabel(label)) return _ObjectField(val, allowSceneObjects, options); }
        protected ObjType ObjectField<ObjType>(ref ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object => val = _ObjectField(val, allowSceneObjects, options);
        protected ObjType ObjectField<ObjType>(string label, ref ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object => val = _ObjectField(label, val, allowSceneObjects, options);

        protected Color _ColorField(Color val, params GUILayoutOption[] options) { CheckOptions(ref options); return ControlLabelScope.HasLabel ? EditorGUILayout.ColorField(ControlLabelScope.Label, val, options) : EditorGUILayout.ColorField(val, options); }
        protected Color _ColorField(string label, Color val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _ColorField(val, options); }
        protected Color ColorField(ref Color val, params GUILayoutOption[] options) => val = _ColorField(val, options);
        protected Color ColorField(string label, ref Color val, params GUILayoutOption[] options) => val = _ColorField(label, val, options);
        protected Color ColorFieldNoLabel(string key, Color defaultVal, params GUILayoutOption[] options) => SetColor(key, _ColorField(GetColor(key, defaultVal), options));
        protected Color ColorField(string key, Color defaultVal, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return ColorFieldNoLabel(key, defaultVal, options); }
        protected Color ColorFieldNoLabel(string key, params GUILayoutOption[] options) => SetColor(key, _ColorField(GetColor(key), options));
        protected Color ColorField(string key, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return ColorFieldNoLabel(key, options); }

        protected bool IceButton(string text, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(text, tooltip), StlIce, options);
        protected bool IceButton(string text, bool on, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(on ? $"{text.Color(ThemeColorExp)}" : text, tooltip), StlIce, options);
        protected bool IceButton(Texture texture, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(texture, tooltip), StlIce, options);
        protected bool IceButton(GUIContent content, params GUILayoutOption[] options) => GUILayout.Button(content, StlIce, options);
        protected bool _IceToggle(string text, bool val, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(val ? $"{text.Color(ThemeColorExp)}" : text, tooltip), StlIce) ? !val : val;
        protected bool IceToggle(string text, ref bool val, string tooltip = null, params GUILayoutOption[] options) => val = _IceToggle(text, val, tooltip, options);
        protected bool IceToggle(string key, bool defaultVal = false, string textOverride = null, string tooltip = null, params GUILayoutOption[] options) => SetBool(key, _IceToggle(string.IsNullOrEmpty(textOverride) ? key : textOverride, GetBool(key, defaultVal), tooltip, options));
        protected bool IceToggleAnim(string key, bool defaultVal = false, string textOverride = null, string tooltip = null, params GUILayoutOption[] options) => SetAnimBoolTarget(key, _IceToggle(string.IsNullOrEmpty(textOverride) ? key : textOverride, GetAnimBoolTarget(key, defaultVal), tooltip, options));
        #endregion

        #region Scope
        /// <summary>
        /// 可展开的区域
        /// </summary>
        protected class FolderScope : IDisposable
        {
            /// <summary>
            /// faded < 1
            /// </summary>
            public readonly bool fading;
            /// <summary>
            /// faded > 0
            /// </summary>
            public readonly bool visible;
            /// <summary>
            /// 宽度是否变化
            /// </summary>
            public readonly bool changeWidth;

            readonly EventType beginType;
            public FolderScope(AnimBool ab, bool changeWidth = false)
            {
                visible = ab.faded > 0;
                fading = ab.faded < 1;
                this.changeWidth = changeWidth;
                beginType = Event.current.type;

                if (visible)
                {
                    if (changeWidth)
                    {
                        var width = ab.faded * EditorGUIUtility.currentViewWidth;
                        EditorGUILayout.BeginVertical(StlEmpty, GUILayout.MaxWidth(width), GUILayout.MinWidth(0));
                    }
                    var e = Event.current;
                    if (fading && e.type != EventType.Used)
                    {
                        if (e.type != EventType.Layout && e.type != EventType.Repaint)
                        {
                            e.type = EventType.Ignore;
                        }
                        try
                        {
                            EditorGUILayout.BeginFadeGroup(ab.faded);
                        }
                        catch
                        {
                            Debug.LogError(ab);
                        }
                    }
                }
                else
                {
                    GUILayout.BeginArea(Rect.zero);
                }
            }

            void IDisposable.Dispose()
            {
                if (visible)
                {
                    var e = Event.current;
                    if (fading && e.type != EventType.Used)
                    {
                        EditorGUILayout.EndFadeGroup();
                        if (e.type == EventType.Ignore && beginType != EventType.Layout && beginType != EventType.Repaint)
                        {
                            e.type = beginType;
                        }
                    }
                    if (changeWidth)
                    {
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    GUILayout.EndArea();
                }
            }
        }
        /// <summary>
        /// 规定Prefix标签宽度
        /// </summary>
        protected class LabelWidthScope : IDisposable
        {
            float originWidth;
            public LabelWidthScope(float width)
            {
                originWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = width;
            }
            void IDisposable.Dispose()
            {
                EditorGUIUtility.labelWidth = originWidth;
            }
        }
        /// <summary>
        /// condition满足时暂时禁用指定区域（变灰，无法交互）
        /// </summary>
        protected class DisableScope : IDisposable
        {
            public DisableScope(bool disabled)
            {
                EditorGUI.BeginDisabledGroup(disabled);
            }
            void IDisposable.Dispose()
            {
                EditorGUI.EndDisabledGroup();
            }
        }
        /// <summary>
        /// 指定区域的Field对用户输入延迟响应，作用于 IntField, FloatField, TextField, Vector2Field
        /// </summary>
        protected class DelayedScope : IDisposable
        {
            public static bool inScope = false;
            public DelayedScope()
            {
                inScope = true;
            }
            void IDisposable.Dispose()
            {
                inScope = false;
            }
        }
        /// <summary>
        /// 用于标记ControlLabel的属性
        /// </summary>
        protected class ControlLabelScope : IDisposable
        {
            public static bool HasLabel => !string.IsNullOrEmpty(label);
            public static string Label => label;

            static string label = null;
            static GUIStyle LabelStyle => _labelStyle != null ? _labelStyle : (_labelStyle = "ControlLabel"); static GUIStyle _labelStyle;

            string originLabel;
            bool originRichText;
            Color originFocusColor;

            public ControlLabelScope(string labelVal, Color labelColor)
            {
                originLabel = label;
                originRichText = LabelStyle.richText;
                originFocusColor = LabelStyle.focused.textColor;

                label = labelVal;
                LabelStyle.richText = true;
                LabelStyle.focused.textColor = labelColor;
            }
            void IDisposable.Dispose()
            {
                label = originLabel;
                LabelStyle.richText = originRichText;
                LabelStyle.focused.textColor = originFocusColor;
            }
        }
        /// <summary>
        /// 用于检测控件是否变化
        /// </summary>
        protected class ChangeCheckScope : IDisposable
        {
            static Stack<bool> changedStack = new Stack<bool>();
            public ChangeCheckScope()
            {
                changedStack.Push(GUI.changed);
                GUI.changed = false;
            }
            void IDisposable.Dispose()
            {
                GUI.changed |= changedStack.Pop();
            }
        }

        protected FolderScope Folder(string key, bool defaultVal = false, bool changeWidth = false) => new FolderScope(GetAnimBool(key, defaultVal), changeWidth);
        /// <summary>显示一个可展开的节</summary>
        protected FolderScope SectionFolder(string key, bool defaultVal = true, string labelOverride = null)
        {
            var ab = GetAnimBool(key, defaultVal);
            ab.target = GUILayout.Toggle(ab.target, string.IsNullOrEmpty(labelOverride) ? key : labelOverride, StlSectionHeader);
            return new FolderScope(ab, true);
        }
        protected GUILayout.HorizontalScope Horizontal(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.HorizontalScope(style ?? GUIStyle.none, options);
        protected GUILayout.HorizontalScope Horizontal(params GUILayoutOption[] options) => new GUILayout.HorizontalScope(options);
        protected GUILayout.VerticalScope Vertical(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.VerticalScope(style ?? GUIStyle.none, options);
        protected GUILayout.VerticalScope Vertical(params GUILayoutOption[] options) => new GUILayout.VerticalScope(options);
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope Scroll(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个竖直Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollVertical(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个隐藏bar的Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollInvisible(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个水平Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollHorizontal(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope Scroll(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个竖直Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollVertical(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个隐藏bar的Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollInvisible(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个水平Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollHorizontal(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope Scroll(GUIStyle style, params GUILayoutOption[] options)
        {
            if (!drawingWindowGUI)
            {
                LogError("不能在OnWindowGUI外调用SCROLL!");
                return null;
            }

            int id = GUIUtility.GetControlID(FocusType.Passive);

            if (checkingControlId) currentControlIdSet.Add(id);

            if (!_intVec2Map.TryGetValue(id, out Vector2 vec))
            {
                _intVec2Map[id] = Vector2.zero;

                if (id != -1) allControlIdSet.Add(id);
                needsCheckControlId = true;
            }
            var res = new GUILayout.ScrollViewScope(vec, false, false, GUIStyle.none, GUIStyle.none, style, options);
            SetVector2(id, res.scrollPosition);

            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope Scroll(params GUILayoutOption[] options) => Scroll(GUIStyle.none, options);
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        protected GUILayout.AreaScope Area(Rect rect, GUIStyle style)
        {
            var margin = style.margin;
            return new GUILayout.AreaScope(new Rect(rect.x + margin.left, rect.y + margin.top, rect.width - margin.horizontal, rect.height - margin.vertical), GUIContent.none, style);
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        protected GUILayout.AreaScope Area(Rect rect) => Area(rect, StlBackground);
        /// <summary>
        /// 在Using语句中使用的Scope，手动添加一个Prefix
        /// </summary>
        protected GUILayout.HorizontalScope Prefix(string text) { var res = Horizontal(); using (LabelWidth(EditorGUIUtility.labelWidth + 2)) EditorGUILayout.PrefixLabel(text, "button", StlPrefix); return res; }
        /// <summary>
        /// 在Using语句中使用的Scope，调整Label的宽度
        /// </summary>
        protected LabelWidthScope LabelWidth(float width) => new LabelWidthScope(width);
        /// <summary>
        /// 在Using语句中使用的Scope，condition满足时暂时禁用指定区域（变灰，无法交互）
        /// </summary>
        protected DisableScope Disable(bool disabled) => new DisableScope(disabled);
        /// <summary>
        /// 在Using语句中使用的Scope，用于标记ControlLabel的属性
        /// </summary>
        protected ControlLabelScope ControlLabel(string label) => new ControlLabelScope(label, ThemeColor);
        /// <summary>
        /// 在Using语句中使用的Scope，用于标记ControlLabel的属性
        /// </summary>
        protected ControlLabelScope ControlLabel(string label, string labelOverride) => new ControlLabelScope(string.IsNullOrEmpty(labelOverride) ? label : labelOverride, ThemeColor);
        #endregion

        #region Scope Object
        protected GUILayout.HorizontalScope HORIZONTAL => Horizontal();
        protected GUILayout.VerticalScope VERTICAL => Vertical();
        protected GUILayout.VerticalScope BOX => Vertical(StlBox);
        protected GUILayout.VerticalScope NODE => Vertical(StlNode);
        protected GUILayout.VerticalScope HIGHLIGHT => Vertical(StlHighlight);
        protected GUILayout.VerticalScope GROUP => Vertical(StlGroup);
        protected GUILayout.HorizontalScope DOCK => Horizontal(StlDock);
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope SCROLL => Scroll();
        /// <summary>
        /// 在Using语句中使用的Scope，指定区域的Field对用户输入延迟响应，作用于 IntField, FloatField, TextField, Vector2Field
        /// </summary>
        protected DelayedScope DELAYED => new DelayedScope();
        /// <summary>
        /// 在Using语句中使用的Scope，检查块中控件是否变化，若有变化，Changed置为true
        /// </summary>
        protected ChangeCheckScope CHANGECHECK => new ChangeCheckScope();
        /// <summary>
        /// 在CHECK块中控件是否变化
        /// </summary>
        protected bool Changed => GUI.changed;
        #endregion

        #endregion

        #region 【PRIVATE】
        bool drawingWindowGUI = false;
        bool needsCheckControlId = false;
        bool checkingControlId = false;
        readonly HashSet<int> allControlIdSet = new HashSet<int>();
        readonly HashSet<int> currentControlIdSet = new HashSet<int>();

        void OnStatusGUI(Rect position)
        {
            using (GROUP) using (var sf = SectionFolder("临时数据监控", false)) using (LabelWidth(Mathf.Max(64, position.width - 256)))
            {
                using (BOX) using (SectionFolder("Color Map"))
                {
                    using (CHANGECHECK)
                    {
                        ColorField("ThemeColor");

                        if (Changed) RefreshThemeColor();
                    }
                    if (_stringColorMap.Count > 1) foreach (var key in _stringColorMap.Keys) if (key != "ThemeColor") ColorField(key);
                }
                if (_stringIntMap.Count > 0) using (BOX) using (SectionFolder("Int Map")) foreach (var key in _stringIntMap.Keys) IntField(key);
                if (_stringFloatMap.Count > 0) using (BOX) using (SectionFolder("Float Map")) foreach (var key in _stringFloatMap.Keys) FloatField(key);
                if (_stringStringMap.Count > 0) using (BOX) using (SectionFolder("String Map")) foreach (var key in _stringStringMap.Keys) TextField(key);
                if (_stringVec2Map.Count > 0) using (BOX) using (SectionFolder("Vector2 Map")) foreach (var key in _stringVec2Map.Keys) Vector2Field(key);
                if (_intVec2Map.Count > 0) using (BOX) using (SectionFolder("Int-Vector2 Map")) foreach (var key in _intVec2Map.Keys) _intVec2Map[key] = _Vector2Field(key.ToString(), _intVec2Map[key]);
                if (_stringBoolMap.Count > 0) using (BOX) using (SectionFolder("Bool Map")) foreach (var key in _stringBoolMap.Keys) Toggle(key);
                if (_stringAnimBoolMap.Count > 0) using (BOX) using (SectionFolder("Anim Bool Map")) foreach (var key in _stringAnimBoolMap.Keys) using (HORIZONTAL) { SetAnimBoolTarget(key, _Toggle(key.ToString(), GetAnimBoolTarget(key))); _Slider(GetAnimBoolFaded(key)); }
            }
        }
        void OnGUI()
        {
            const float debugSeparatorWidth = 8; //中间分隔栏的宽
            const string debugScaleKey = "Debug Panel Scale";
            var debugScale = Mathf.Clamp(GetFloat(debugScaleKey), debugSeparatorWidth + 2, ((_debugUIOrientation == UIOrientation.Left || _debugUIOrientation == UIOrientation.Right) ? position.width : position.height) - 2);
            const string draggingSeparatorKey = "Dragging Separator";
            var draggingSeparator = GetBool(draggingSeparatorKey);

            // Rect计算
            Rect rBack = GUIUtility.ScreenToGUIRect(position);
            rBack.y = 0;
            Rect rContent = rBack;
            Rect rDebug = rBack;
            Rect rSeparator = rBack;

            // 计算
            if (DebugMode)
            {
                // 计算Rect，鼠标样式
                switch (_debugUIOrientation)
                {
                    case UIOrientation.Right:
                        rContent.width -= debugScale;
                        rDebug.width = debugScale - debugSeparatorWidth;
                        rDebug.x = rBack.width - rDebug.width;

                        rSeparator.width = debugSeparatorWidth;
                        rSeparator.x = rDebug.x - debugSeparatorWidth;

                        EditorGUIUtility.AddCursorRect(rSeparator, MouseCursor.ResizeHorizontal);
                        break;
                    case UIOrientation.Left:
                        rContent.width -= debugScale;
                        rDebug.width = debugScale - debugSeparatorWidth;
                        rContent.x = debugScale;

                        rSeparator.width = debugSeparatorWidth;
                        rSeparator.x = rContent.x - debugSeparatorWidth;

                        EditorGUIUtility.AddCursorRect(rSeparator, MouseCursor.ResizeHorizontal);
                        break;
                    case UIOrientation.Bottom:
                        rContent.height -= debugScale;
                        rDebug.height = debugScale - debugSeparatorWidth;
                        rDebug.y = rBack.height - rDebug.height;

                        rSeparator.height = debugSeparatorWidth;
                        rSeparator.y = rDebug.y - debugSeparatorWidth;

                        EditorGUIUtility.AddCursorRect(rSeparator, MouseCursor.ResizeVertical);
                        break;
                    case UIOrientation.Top:
                        rContent.height -= debugScale;
                        rDebug.height = debugScale - debugSeparatorWidth;
                        rContent.y = debugScale;

                        rSeparator.height = debugSeparatorWidth;
                        rSeparator.y = rContent.y - debugSeparatorWidth;

                        EditorGUIUtility.AddCursorRect(rSeparator, MouseCursor.ResizeVertical);
                        break;
                }

                // 分隔栏
                int separatorId = GUIUtility.GetControlID(FocusType.Passive);
                var e = Event.current;
                switch (e.GetTypeForControl(separatorId))
                {
                    case EventType.MouseDown:
                        if (rSeparator.Contains(e.mousePosition))
                        {
                            GUIUtility.hotControl = separatorId;

                            SetBool(draggingSeparatorKey, true);
                            Repaint();
                            e.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (draggingSeparator)
                        {
                            SetBool(draggingSeparatorKey, false);
                            Repaint();
                            e.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (draggingSeparator)
                        {
                            switch (_debugUIOrientation)
                            {
                                case UIOrientation.Right:
                                    SetFloat(debugScaleKey, debugScale - e.delta.x);
                                    break;
                                case UIOrientation.Left:
                                    SetFloat(debugScaleKey, debugScale + e.delta.x);
                                    break;
                                case UIOrientation.Bottom:
                                    SetFloat(debugScaleKey, debugScale - e.delta.y);
                                    break;
                                case UIOrientation.Top:
                                    SetFloat(debugScaleKey, debugScale + e.delta.y);
                                    break;
                            }
                            Repaint();
                            e.Use();
                        }
                        break;
                }
            }

            // 画内容
            using (new GUILayout.AreaScope(rContent)) using (ScrollInvisible("Main Content Area", StlBackground))
            {
                // 开始检测controlId重复
                drawingWindowGUI = true;
                if (needsCheckControlId)
                {
                    currentControlIdSet.Clear();

                    needsCheckControlId = false;
                    checkingControlId = true;
                }

                OnWindowGUI(new Rect(Vector2.zero, rContent.size));

                // 结束检测controlId重复
                drawingWindowGUI = false;
                if (checkingControlId)
                {
                    string debugStr = "ControlId去重!";

                    allControlIdSet.ExceptWith(currentControlIdSet);
                    foreach (var id in allControlIdSet)
                    {
                        _intVec2Map.Remove(id);
                        debugStr += "\n" + id;
                    }
                    allControlIdSet.Clear();
                    allControlIdSet.UnionWith(currentControlIdSet);

                    checkingControlId = false;

                    Log(debugStr);
                }
            }
            OnExtraGUI(rContent);

            // 画Debug相关
            if (DebugMode)
            {
                // 画DebugUI
                using (new GUILayout.AreaScope(rDebug)) using (ScrollInvisible("Debug UI Scroll Area", StlBackground))
                {
                    var debugPos = new Rect(Vector2.zero, rDebug.size);
                    OnDebugGUI(debugPos);
                    if (HasStatusUI) OnStatusGUI(debugPos);
                }
                // 画分隔栏
                GUI.Box(rSeparator, GUIContent.none, draggingSeparator ? StlSeparatorOn : StlSeparator);
            }
        }
        #endregion
    }
}