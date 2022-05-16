using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using IceEngine;
using System.Linq;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

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
        /// 默认主题颜色
        /// </summary>
        protected virtual Color DefaultThemeColor => IcePreference.Config.themeColor;
        /// <summary>
        /// 是否在 DebugUI 中显示 StatusUI
        /// </summary>
        protected virtual bool HasStatusUI => true;
        /// <summary>
        /// 反序列化事件
        /// </summary>
        public virtual void OnAfterDeserialize() { }
        /// <summary>
        /// 序列化事件
        /// </summary>
        public virtual void OnBeforeSerialize() { }
        /// <summary>
        /// 主题颜色改变事件
        /// </summary>
        protected virtual void OnThemeColorChange() { }
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
        public Color ThemeColor { get => Pack.ThemeColor; set => Pack.ThemeColor = value; }
        /// <summary>
        /// 主题颜色表达式
        /// </summary>
        public string ThemeColorExp => Pack.ThemeColorExp;
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
        protected IceGUIUtility.GUIPackScope PACK => new IceGUIUtility.GUIPackScope(Pack);
        #endregion

        #endregion

        #region 【PRIVATE】
        internal IceGUIAutoPack Pack => _pack ??= new IceGUIAutoPack(DefaultThemeColor, Repaint, OnThemeColorChange); [SerializeField] IceGUIAutoPack _pack;

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
                    using (GUICHECK)
                    {
                        ColorField("ThemeColor");

                        if (GUIChanged) Pack.RefreshThemeColor();
                    }
                    if (Pack._stringColorMap.Count > 1) foreach (var key in Pack._stringColorMap.Keys) if (key != "ThemeColor") ColorField(key);
                }
                if (Pack._stringBoolMap.Count > 0) using (BOX) using (SectionFolder("Bool Map")) foreach (var key in Pack._stringBoolMap.Keys) Toggle(key);
                if (Pack._stringIntMap.Count > 0) using (BOX) using (SectionFolder("Int Map")) foreach (var key in Pack._stringIntMap.Keys) IntField(key);
                if (Pack._stringFloatMap.Count > 0) using (BOX) using (SectionFolder("Float Map")) foreach (var key in Pack._stringFloatMap.Keys) FloatField(key);
                if (Pack._stringStringMap.Count > 0) using (BOX) using (SectionFolder("String Map")) foreach (var key in Pack._stringStringMap.Keys) TextField(key);
                if (Pack._stringVec2Map.Count > 0) using (BOX) using (SectionFolder("Vector2 Map")) foreach (var key in Pack._stringVec2Map.Keys) Vector2Field(key);
                if (Pack._stringVec3Map.Count > 0) using (BOX) using (SectionFolder("Vector3 Map")) foreach (var key in Pack._stringVec3Map.Keys) Vector3Field(key);
                if (Pack._stringVec4Map.Count > 0) using (BOX) using (SectionFolder("Vector4 Map")) foreach (var key in Pack._stringVec4Map.Keys) Vector4Field(key);
                if (Pack._stringVec2IntMap.Count > 0) using (BOX) using (SectionFolder("Vector2Int Map")) foreach (var key in Pack._stringVec2IntMap.Keys) Vector2IntField(key);
                if (Pack._stringVec3IntMap.Count > 0) using (BOX) using (SectionFolder("Vector3Int Map")) foreach (var key in Pack._stringVec3IntMap.Keys) Vector3IntField(key);
                if (Pack._intVec2Map.Count > 0) using (BOX) using (SectionFolder("Int-Vector2 Map")) foreach (var key in Pack._intVec2Map.Keys) Pack._intVec2Map[key] = _Vector2Field(key.ToString(), Pack._intVec2Map[key]);
                // AnimBool会被SectionFolder影响,必须放在最后面
                if (Pack._stringAnimBoolMap.Count > 0) using (BOX) using (SectionFolder("Anim Bool Map")) foreach (var key in Pack._stringAnimBoolMap.Keys) using (HORIZONTAL) { SetAnimBoolTarget(key, _Toggle(key.ToString(), GetAnimBoolTarget(key))); _Slider(GetAnimBoolFaded(key)); }
            }
        }
        void OnGUI()
        {
            using var _ = PACK;

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