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
    /// 封装通用GUI类
    /// </summary>
    public static class IceGUI
    {
        #region 样式
        public static GUIStyle StlEmpty => _stlEmpty?.Check() ?? (_stlEmpty = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), }); static GUIStyle _stlEmpty;
        public static GUIStyle StlBackground => _stlBackground?.Check() ?? (_stlBackground = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), stretchHeight = true, }); static GUIStyle _stlBackground;
        public static GUIStyle StlLabel => _stlLabel?.Check() ?? (_stlLabel = new GUIStyle("label") { richText = true, wordWrap = true }); static GUIStyle _stlLabel;
        public static GUIStyle StlHeader => _stlHeader?.Check() ?? (_stlHeader = new GUIStyle("LODRendererRemove") { margin = new RectOffset(5, 3, 6, 2), fontSize = 12, fontStyle = FontStyle.Normal, richText = true, }); static GUIStyle _stlHeader;
        public static GUIStyle StlSectionHeader => null;
        public static GUIStyle StlPrefix => null;
        public static GUIStyle StlDock => _stlDock?.Check() ?? (_stlDock = new GUIStyle("dockarea") { padding = new RectOffset(1, 1, 1, 1), contentOffset = new Vector2(0f, 0f), }); static GUIStyle _stlDock;
        public static GUIStyle StlGroup => _stlGroup?.Check() ?? (_stlGroup = new GUIStyle("NotificationBackground") { border = new RectOffset(16, 16, 13, 13), margin = new RectOffset(6, 6, 6, 6), padding = new RectOffset(10, 10, 6, 6), }); static GUIStyle _stlGroup;
        public static GUIStyle StlBox => _stlBox?.Check() ?? (_stlBox = new GUIStyle("window") { margin = new RectOffset(4, 4, 4, 4), padding = new RectOffset(6, 6, 6, 6), contentOffset = new Vector2(0f, 0f), stretchWidth = false, stretchHeight = false, }); static GUIStyle _stlBox;
        public static GUIStyle StlNode => _stlNode?.Check() ?? (_stlNode = new GUIStyle("flow node 0") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); static GUIStyle _stlNode;
        public static GUIStyle StlHighlight => _stlHighlight?.Check() ?? (_stlHighlight = new GUIStyle("LightmapEditorSelectedHighlight") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 10, 10), overflow = new RectOffset(0, 0, 0, 0), }); static GUIStyle _stlHighlight;
        public static GUIStyle StlButton => _stlButton?.Check() ?? (_stlButton = new GUIStyle("LargeButton") { richText = true, }); static GUIStyle _stlButton;
        public static GUIStyle StlError => _stlError?.Check() ?? (_stlError = new GUIStyle("Wizard Error") { border = new RectOffset(32, 0, 32, 0), padding = new RectOffset(32, 0, 7, 7), fixedHeight = 0f, }.Initialize(stl => { stl.normal.textColor = new Color(1f, 0.8469602f, 0f); })); static GUIStyle _stlError;
        public static GUIStyle StlSeparator => null;
        public static GUIStyle StlSeparatorOn => null;
        public static GUIStyle StlIce => _stlIce?.Check() ?? (_stlIce = new GUIStyle("BoldTextField") { padding = new RectOffset(3, 3, 2, 2), fontSize = 11, richText = true, fixedHeight = 0f, stretchWidth = false, imagePosition = ImagePosition.ImageLeft, fontStyle = FontStyle.Normal }); static GUIStyle _stlIce;
        public static GUIStyle GetStlSectionHeader(Color themeColor) => new GUIStyle("AnimationEventTooltip")
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
            stl.onNormal.textColor = themeColor;
            stl.onNormal.background = stl.normal.background;
            stl.onHover.textColor = themeColor * 1.2f;
            stl.onHover.background = stl.normal.background;
        });
        public static GUIStyle GetStlPrefix(Color themeColor) => new GUIStyle("PrefixLabel") { margin = new RectOffset(3, 3, 2, 2), padding = new RectOffset(1, 1, 0, 0), alignment = TextAnchor.MiddleLeft, richText = true, }.Initialize(stl => { stl.focused.textColor = stl.active.textColor = stl.onNormal.textColor = stl.onActive.textColor = themeColor; stl.onNormal.background = stl.active.background; });
        public static GUIStyle GetStlSeparator(Color themeColor) => new GUIStyle($"flow node {GetThemeColorHueIndex(themeColor)}");
        public static GUIStyle GetStlSeparatorOn(Color themeColor) => new GUIStyle($"flow node {GetThemeColorHueIndex(themeColor)} on");
        #endregion

        #region Drawing Elements
        /// <summary>
        /// 用特定Style填充区域
        /// </summary>
        public static Rect Box(Rect rect, GUIStyle style, bool hasMargin = true, bool hoverable = false, bool isActive = false, bool on = false, bool hasKeyboardFocus = false)
        {
            if (style == null) return rect;
            var margin = style.margin;
            var res = hasMargin ? rect.MoveEdge(margin.left, -margin.right, margin.top, -margin.bottom) : rect;
            if (Event.current.type == EventType.Repaint)
            {
                style.Draw(res, hoverable && res.Contains(Event.current.mousePosition), isActive, on, hasKeyboardFocus);
            }
            return res;
        }
        /// <summary>
        /// 用特定Style填充区域
        /// </summary>
        public static Rect Box(GUIStyle style, params GUILayoutOption[] options) => Box(GUILayoutUtility.GetRect(GUIContent.none, style, options), style, false);
        /// <summary>
        /// 用特定Style填充区域
        /// </summary>
        public static Rect Box(GUIStyle style, bool hoverable = false, params GUILayoutOption[] options) => Box(GUILayoutUtility.GetRect(GUIContent.none, style, options), style, false, hoverable);
        /// <summary>
        /// 用特定Style填充区域
        /// </summary>
        public static Rect Box(GUIStyle style, bool hoverable = false, bool isActive = false, bool on = false, bool hasKeyboardFocus = false, params GUILayoutOption[] options) => Box(GUILayoutUtility.GetRect(GUIContent.none, style, options), style, false, hoverable, isActive, on, hasKeyboardFocus);

        public static void Header(string text) => GUILayout.Label(text.Color(IceConfig.Config.themeColor), StlHeader);
        public static void Header(string text, Color color) => GUILayout.Label(text.Color(color), StlHeader);
        public static void Header(string text, string colorExp) => GUILayout.Label(text.Color(colorExp), StlHeader);
        public static bool SectionHeader(AnimBool val, string label, GUIStyle stlSectionHeader = null, params GUILayoutOption[] options)
        {
            return val.target = GUILayout.Toggle(val.target, label, stlSectionHeader ?? StlSectionHeader);
        }

        #endregion

        #region Button & Fields
        static readonly GUILayoutOption[] defaultOptions = new GUILayoutOption[] { GUILayout.MinWidth(EditorGUIUtility.singleLineHeight) };
        static GUILayoutOption[] CheckOptions(ref GUILayoutOption[] options) => options = options.Length > 0 ? options : defaultOptions;

        public static bool Button(string text, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Button(text, styleOverride ?? StlButton, options);
        public static bool Button(string text, params GUILayoutOption[] options) => Button(text, null, options);
        public static bool Button(GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Button(content, styleOverride ?? StlButton, options);
        public static bool Button(GUIContent content, params GUILayoutOption[] options) => Button(content, null, options);
        #endregion

        public static void DrawSerializedObject(SerializedObject so)
        {
            so.UpdateIfRequiredOrScript();
            SerializedProperty itr = so.GetIterator();
            itr.NextVisible(true);
            do if (itr.propertyPath != "m_Script")
                {
                    // Property Field
                    EditorGUILayout.PropertyField(itr, true);
                }
            while (itr.NextVisible(false));
            so.ApplyModifiedProperties();
        }
        public static SettingsProvider GetSettingProvider<ConfigType>(string path, ConfigType config, Func<ConfigType> createConfigAction, SettingsScope scope = SettingsScope.User) where ConfigType : ScriptableObject
        {
            SerializedObject so = null;
            return new SettingsProvider(path, scope)
            {
                activateHandler = (filter, rootElement) =>
                {
                    if (config == null) return;
                    so = new SerializedObject(config);
                },
                guiHandler = (filter) =>
                {
                    if (config == null)
                    {
                        if (Button("Create Config"))
                        {
                            config = createConfigAction?.Invoke();
                            if (config != null) so = new SerializedObject(config);
                        }
                        return;
                    }
                    DrawSerializedObject(so);
                }
            };
        }

        #region PRIVATE
        static int GetThemeColorHueIndex(Color themeColor)
        {
            Color.RGBToHSV(themeColor, out float h, out float s, out _);
            if (s < 0.3f) return 0;
            if (h < 0.06f) return 6;
            if (h < 0.13f) return 5;
            if (h < 0.19f) return 4;
            if (h < 0.46f) return 3;
            if (h < 0.52f) return 2;
            if (h < 0.84f) return 1;
            return 6;
        }
        #endregion

    }
}