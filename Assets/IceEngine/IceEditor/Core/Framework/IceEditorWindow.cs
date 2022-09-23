using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Framework
{
    /// <summary>
    /// 包含基础功能的窗口类
    /// </summary>
    public abstract class IceEditorWindow : EditorWindow, IHasCustomMenu
    {
        #region 【配置】

        #region 核心配置

        /// <summary>
        /// GUI界面代码
        /// </summary>
        protected abstract void OnWindowGUI(Rect position);
        #endregion

        #region 可选配置
        /// <summary>
        /// 默认标题文本
        /// </summary>
        protected virtual string Title => GetType().Name;
        /// <summary>
        /// 标题Icon
        /// </summary>
        protected virtual Texture Icon => null;
        /// <summary>
        /// 重载此字符串来改变上下文菜单中“Debug模式”
        /// </summary>
        protected virtual string DebugModeName => "Debug";
        /// <summary>
        /// 默认标题，重载此项将使Tile和Icon失效
        /// </summary>
        public virtual GUIContent TitleContent => Icon == null ? new GUIContent(Title) : new GUIContent(Title, Icon);
        /// <summary>
        ///  Debug模式的默认标题
        /// </summary>
        public virtual GUIContent TitleContentDebug => new GUIContent(TitleContent) { text = TitleContent.text + $" - {DebugModeName}" };
        /// <summary>
        /// 默认主题颜色
        /// </summary>
        protected virtual Color DefaultThemeColor => IceGUIUtility.DefaultThemeColor;
        /// <summary>
        /// 重载此方法来添加自定义上下文菜单
        /// </summary>
        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"{DebugModeName}模式"), DebugMode, () => DebugMode = !DebugMode);
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/右"), _debugGUIDirection == IceGUIDirection.Right, () => { _debugGUIDirection = IceGUIDirection.Right; });
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/左"), _debugGUIDirection == IceGUIDirection.Left, () => { _debugGUIDirection = IceGUIDirection.Left; });
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/下"), _debugGUIDirection == IceGUIDirection.Bottom, () => { _debugGUIDirection = IceGUIDirection.Bottom; });
            menu.AddItem(new GUIContent($"{DebugModeName}面板位置/上"), _debugGUIDirection == IceGUIDirection.Top, () => { _debugGUIDirection = IceGUIDirection.Top; });
        }
        protected virtual void OnEnable()
        {
            wantsLessLayoutEvents = true;
            Input.imeCompositionMode = IMECompositionMode.On;
            RefreshTitleContent();
        }
        /// <summary>
        /// 额外的GUI（无Layout）
        /// </summary>
        protected virtual void OnExtraGUI(Rect position) { }
        /// <summary>
        /// Debug界面代码
        /// </summary>
        protected virtual void OnDebugGUI(Rect position) { }
        /// <summary>
        /// 是否在 DebugUI 中显示 StatusUI
        /// </summary>
        protected virtual bool HasStatusUI => true;
        /// <summary>
        /// OnWindowGUI是否有默认Scroll区域
        /// </summary>
        protected virtual bool HasScrollScopeOnWindowGUI => true;
        /// <summary>
        /// 主题颜色改变事件
        /// </summary>
        protected virtual void OnThemeColorChange() { }
        #endregion

        #endregion

        #region 【接口】

        #region 临时数据托管
        public Color GetColor(string key) => Pack.GetColor(key);
        public Color GetColor(string key, Color defaultVal) => Pack.GetColor(key, defaultVal);
        public Color SetColor(string key, Color value) => Pack.SetColor(key, value);

        public bool GetBool(string key, bool defaultVal = false) => Pack.GetBool(key, defaultVal);
        public bool SetBool(string key, bool value) => Pack.SetBool(key, value);

        public AnimBool GetAnimBool(string key, bool defaultVal = false) => Pack.GetAnimBool(key, defaultVal);
        public bool GetAnimBoolValue(string key, bool defaultVal = false) => Pack.GetAnimBoolValue(key, defaultVal);
        public bool GetAnimBoolTarget(string key, bool defaultVal = false) => Pack.GetAnimBoolTarget(key, defaultVal);
        public float GetAnimBoolFaded(string key, bool defaultVal = false) => Pack.GetAnimBoolFaded(key, defaultVal);
        public bool SetAnimBoolValue(string key, bool value) => Pack.SetAnimBoolValue(key, value);
        public bool SetAnimBoolTarget(string key, bool value) => Pack.SetAnimBoolTarget(key, value);

        public int GetInt(string key, int defaultVal = 0) => Pack.GetInt(key, defaultVal);
        public int SetInt(string key, int value) => Pack.SetInt(key, value);

        public float GetFloat(string key, float defaultVal = 0) => Pack.GetFloat(key, defaultVal);
        public float SetFloat(string key, float value) => Pack.SetFloat(key, value);

        public string GetString(string key, string defaultVal = "") => Pack.GetString(key, defaultVal);
        public string SetString(string key, string value) => Pack.SetString(key, value);

        public Vector2 GetVector2(int key, Vector2 defaultVal = default) => Pack.GetVector2(key, defaultVal);
        public Vector2 SetVector2(int key, Vector2 value) => Pack.SetVector2(key, value);

        public Vector2 GetVector2(string key, Vector2 defaultVal = default) => Pack.GetVector2(key, defaultVal);
        public Vector2 SetVector2(string key, Vector2 value) => Pack.SetVector2(key, value);

        public Vector3 GetVector3(string key, Vector3 defaultVal = default) => Pack.GetVector3(key, defaultVal);
        public Vector3 SetVector3(string key, Vector3 value) => Pack.SetVector3(key, value);

        public Vector4 GetVector4(string key, Vector4 defaultVal = default) => Pack.GetVector4(key, defaultVal);
        public Vector4 SetVector4(string key, Vector4 value) => Pack.SetVector4(key, value);

        public Vector2Int GetVector2Int(string key, Vector2Int defaultVal = default) => Pack.GetVector2Int(key, defaultVal);
        public Vector2Int SetVector2Int(string key, Vector2Int value) => Pack.SetVector2Int(key, value);

        public Vector3Int GetVector3Int(string key, Vector3Int defaultVal = default) => Pack.GetVector3Int(key, defaultVal);
        public Vector3Int SetVector3Int(string key, Vector3Int value) => Pack.SetVector3Int(key, value);
        #endregion

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
        /// <summary>
        /// DebugUI方向
        /// </summary>
        public IceGUIDirection DebugGUIDirection { get => _debugGUIDirection; set => _debugGUIDirection = value; }
        [SerializeField] IceGUIDirection _debugGUIDirection = IceGUIDirection.Right;
        #endregion

        #region 主题颜色
        /// <summary>
        /// 主题颜色
        /// </summary>
        public Color ThemeColor { get => Pack.ThemeColor; set => Pack.ThemeColor = value; }
        /// <summary>
        /// 主题颜色表达式
        /// </summary>
        public string ThemeColorExp => Pack.ThemeColorExp;
        #endregion

        #region GUIStyle
        /// <summary>
        /// 获取特定自定义样式
        /// </summary>
        protected GUIStyle GetStyle(string name, [CallerFilePath] string callerFilePath = null)
        {
            if (currentFilePath == null) currentFilePath = callerFilePath;
            return IceGUI.GetStyle(name);
        }
        protected GUIStyle GetStyle() => IceGUI.GetStyle();
        #endregion

        #region Log & Dialog
        /// <summary>
        /// 输出一个的Log
        /// </summary>
        protected void Log(string text, UnityEngine.Object context = null)
        {
            text = $"【{titleContent.text.Color(ThemeColorExp)}】{text}";
            if (context == null) Debug.Log(text);
            else Debug.Log(text, context);
        }
        /// <summary>
        /// 输出一个Debug模式下才会显示的Log
        /// </summary>
        protected void LogDebug(string text, UnityEngine.Object context = null)
        {
            if (!DebugMode) return;
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

        #endregion

        #region 【GUI Shortcut】

        #region Scope
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope ScrollAuto(GUIStyle style, params GUILayoutOption[] options)
        {
            if (!drawingWindowGUI)
            {
                LogError("不能在OnWindowGUI外调用SCROLL!");
                return null;
            }

            int id = GUIUtility.GetControlID(FocusType.Passive);

            if (checkingControlId) currentControlIdSet.Add(id);

            if (!Pack._intVec2Map.TryGetValue(id, out Vector2 vec))
            {
                Pack._intVec2Map[id] = Vector2.zero;

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
        protected GUILayout.ScrollViewScope ScrollAuto(params GUILayoutOption[] options) => ScrollAuto(GUIStyle.none, options);

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected GUILayout.ScrollViewScope SCROLL => ScrollAuto();
        /// <summary>
        /// <example>PACK</example>
        /// 用于手动引用IceGUIAutoPack,这样用：
        /// <c>using var _ = <paramref name="PACK"/>;</c>
        /// </summary>
        protected IceGUIUtility.GUIPackScope PACK => new IceGUIUtility.GUIPackScope(Pack);
        #endregion

        #endregion

        #region 【PRIVATE】
        public IceGUIAutoPack Pack => _pack ??= new IceGUIAutoPack(DefaultThemeColor, Repaint, OnThemeColorChange); [SerializeField] IceGUIAutoPack _pack;

        bool drawingWindowGUI = false;
        bool needsCheckControlId = false;
        bool checkingControlId = false;
        readonly HashSet<int> allControlIdSet = new HashSet<int>();
        readonly HashSet<int> currentControlIdSet = new HashSet<int>();

        string currentFilePath = null;

        void OnStatusGUI(Rect position)
        {
            using (GROUP)
            {
                using (HORIZONTAL)
                {
                    IceGUIAuto.SectionHeader("临时数据监控");
                    using (Disable(true)) _TextField(currentFilePath ?? "null");
                }
                using (Folder("临时数据监控")) using (LabelWidth(Mathf.Max(64, position.width - 256)))
                {
                    using (BOX) using (SectionFolder("Color Map"))
                    {
                        using (GUICHECK)
                        {
                            ColorField("ThemeColor");

                            if (GUIChanged)
                            {
                                Pack.RefreshThemeColor();
                            }
                        }
                        if (Pack._stringColorMap.Count > 1) foreach (var key in Pack._stringColorMap.Keys) if (key != "ThemeColor") ColorField(key);
                    }
                    if (Pack._stringBoolMap.Count > 0) using (BOX) using (SectionFolder("Bool Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringBoolMap.Clear(); })) foreach (var key in Pack._stringBoolMap.Keys) Toggle(key);
                    if (Pack._stringIntMap.Count > 0) using (BOX) using (SectionFolder("Int Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringIntMap.Clear(); })) foreach (var key in Pack._stringIntMap.Keys) IntField(key);
                    if (Pack._stringFloatMap.Count > 0) using (BOX) using (SectionFolder("Float Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringFloatMap.Clear(); })) foreach (var key in Pack._stringFloatMap.Keys) FloatField(key);
                    if (Pack._stringStringMap.Count > 0) using (BOX) using (SectionFolder("String Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringStringMap.Clear(); })) foreach (var key in Pack._stringStringMap.Keys) TextField(key);
                    if (Pack._stringVec2Map.Count > 0) using (BOX) using (SectionFolder("Vector2 Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringVec2Map.Clear(); })) foreach (var key in Pack._stringVec2Map.Keys) Vector2Field(key);
                    if (Pack._stringVec3Map.Count > 0) using (BOX) using (SectionFolder("Vector3 Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringVec3Map.Clear(); })) foreach (var key in Pack._stringVec3Map.Keys) Vector3Field(key);
                    if (Pack._stringVec4Map.Count > 0) using (BOX) using (SectionFolder("Vector4 Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringVec4Map.Clear(); })) foreach (var key in Pack._stringVec4Map.Keys) Vector4Field(key);
                    if (Pack._stringVec2IntMap.Count > 0) using (BOX) using (SectionFolder("Vector2Int Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringVec2IntMap.Clear(); })) foreach (var key in Pack._stringVec2IntMap.Keys) Vector2IntField(key);
                    if (Pack._stringVec3IntMap.Count > 0) using (BOX) using (SectionFolder("Vector3Int Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringVec3IntMap.Clear(); })) foreach (var key in Pack._stringVec3IntMap.Keys) Vector3IntField(key);
                    if (Pack._intVec2Map.Count > 0) using (BOX) using (SectionFolder("Int-Vector2 Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._intVec2Map.Clear(); })) foreach (var key in Pack._intVec2Map.Keys) Pack._intVec2Map[key] = _Vector2Field(key.ToString(), Pack._intVec2Map[key]);
                    // AnimBool会被SectionFolder影响,必须放在最后面
                    if (Pack._stringAnimBoolMap.Count > 0) using (BOX) using (SectionFolder("Anim Bool Map", extraAction: () => { Space(); if (IceButton("Clear")) Pack._stringAnimBoolMap.Clear(); })) foreach (var key in Pack._stringAnimBoolMap.Keys) using (HORIZONTAL) { SetAnimBoolTarget(key, _Toggle(key.ToString(), GetAnimBoolTarget(key))); _Slider(GetAnimBoolFaded(key)); }
                }
            }
        }
        void OnGUI()
        {
            using var _ = PACK;

            // Rect计算
            Rect rBack = new Rect(Vector2.zero, position.size);

            // 画内容
            using (SubAreaScope area = DebugMode ? SubArea(rBack, out var _, out var _, "Debug Area", 432, DebugGUIDirection) : null)
            {
                Rect rMain = (area?.mainRect ?? rBack);
                using (Area(rMain)) using (HasScrollScopeOnWindowGUI ? ScrollInvisible("Main Content Area", StlBackground) : null)
                {
                    // 开始检测controlId重复
                    drawingWindowGUI = true;
                    if (needsCheckControlId)
                    {
                        currentControlIdSet.Clear();

                        needsCheckControlId = false;
                        checkingControlId = true;
                    }

                    OnWindowGUI(new Rect(Vector2.zero, rMain.size));

                    // 结束检测controlId重复
                    drawingWindowGUI = false;
                    if (checkingControlId)
                    {
                        //string debugStr = "ControlId去重!";

                        allControlIdSet.ExceptWith(currentControlIdSet);
                        foreach (var id in allControlIdSet)
                        {
                            Pack._intVec2Map.Remove(id);
                            //debugStr += "\n" + id;
                        }
                        allControlIdSet.Clear();
                        allControlIdSet.UnionWith(currentControlIdSet);

                        checkingControlId = false;

                        //Log(debugStr);
                    }
                }

                OnExtraGUI(rMain);

                // 画Debug相关
                if (DebugMode)
                {
                    // 画DebugUI
                    using (Area(area.subRect)) using (ScrollInvisible("Debug UI Scroll Area", StlBackground))
                    {
                        var debugPos = new Rect(Vector2.zero, area.subRect.size);
                        OnDebugGUI(debugPos);
                        if (HasStatusUI) OnStatusGUI(debugPos);
                    }
                }
            }
        }
        #endregion
    }
}