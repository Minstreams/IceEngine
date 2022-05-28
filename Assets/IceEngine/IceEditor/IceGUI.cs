using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using IceEngine;
using IceEditor.Internal;

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
        public static GUIStyle StlDock => _stlDock?.Check() ?? (_stlDock = new GUIStyle("dockarea") { padding = new RectOffset(1, 1, 1, 1), contentOffset = new Vector2(0f, 0f), }); static GUIStyle _stlDock;
        public static GUIStyle StlGroup => _stlGroup?.Check() ?? (_stlGroup = new GUIStyle("NotificationBackground") { border = new RectOffset(16, 16, 13, 13), margin = new RectOffset(6, 6, 6, 6), padding = new RectOffset(10, 10, 6, 6), }); static GUIStyle _stlGroup;
        public static GUIStyle StlBox => _stlBox?.Check() ?? (_stlBox = new GUIStyle("window") { margin = new RectOffset(4, 4, 4, 4), padding = new RectOffset(6, 6, 6, 6), contentOffset = new Vector2(0f, 0f), stretchWidth = false, stretchHeight = false, }); static GUIStyle _stlBox;
        public static GUIStyle StlNode => _stlNode?.Check() ?? (_stlNode = new GUIStyle("flow node 0") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); static GUIStyle _stlNode;
        public static GUIStyle StlHighlight => _stlHighlight?.Check() ?? (_stlHighlight = new GUIStyle("LightmapEditorSelectedHighlight") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 10, 10), overflow = new RectOffset(0, 0, 0, 0), }); static GUIStyle _stlHighlight;
        public static GUIStyle StlButton => _stlButton?.Check() ?? (_stlButton = new GUIStyle("LargeButton") { richText = true, }); static GUIStyle _stlButton;
        public static GUIStyle StlError => _stlError?.Check() ?? (_stlError = new GUIStyle("Wizard Error") { border = new RectOffset(32, 0, 32, 0), padding = new RectOffset(32, 0, 7, 7), fixedHeight = 0f, }.Initialize(stl => { stl.normal.textColor = new Color(1f, 0.8469602f, 0f); })); static GUIStyle _stlError;
        public static GUIStyle StlIce => _stlIce?.Check() ?? (_stlIce = new GUIStyle("BoldTextField") { padding = new RectOffset(3, 3, 2, 2), fontSize = 11, richText = true, fixedHeight = 0f, stretchWidth = false, imagePosition = ImagePosition.ImageLeft, fontStyle = FontStyle.Normal }); static GUIStyle _stlIce;
        public static GUIStyle StlSectionHeader => IceGUIUtility.HasPack ? IceGUIUtility.CurrentPack.StlSectionHeader : _stlSectionHeader?.Check() ?? (_stlSectionHeader = IceGUIUtility.GetStlSectionHeader(IcePreference.Config.themeColor)); static GUIStyle _stlSectionHeader;
        public static GUIStyle StlPrefix => IceGUIUtility.HasPack ? IceGUIUtility.CurrentPack.StlPrefix : _stlPrefix?.Check() ?? (_stlPrefix = IceGUIUtility.GetStlPrefix(IcePreference.Config.themeColor)); static GUIStyle _stlPrefix;
        public static GUIStyle StlSubAreaSeparator => IceGUIUtility.HasPack ? IceGUIUtility.CurrentPack.StlSubAreaSeparator : _stlSubAreaSeparator?.Check() ?? (_stlSubAreaSeparator = IceGUIUtility.GetStlSubAreaSeparator(IcePreference.Config.themeColor)); static GUIStyle _stlSubAreaSeparator;
        #endregion

        #region Scope
        /// <summary>
        /// 可展开的区域
        /// </summary>
        public class FolderScope : IDisposable
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
        public class LabelWidthScope : IDisposable
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
        public class DisableScope : IDisposable
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
        public class DelayedScope : IDisposable
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
        internal class ControlLabelScope : IDisposable
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
        public class ChangeCheckScope : IDisposable
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
        /// <summary>
        /// 用于将一个区域拆分为可调整大小的两个区域
        /// </summary>
        public class SubAreaScope : IDisposable
        {
            /// <summary>
            /// 主区域的Rect
            /// </summary>
            public Rect mainRect;
            /// <summary>
            /// 副区域的Rect
            /// </summary>
            public Rect subRect;

            Rect originRect;
            Rect sepRect;
            Action<float> onPosChange;
            IceGUIDirection direction;
            GUIStyle separatorStyle;
            float pos;

            static float posOffset;

            public SubAreaScope(Rect rect, float pos, Action<float> onPosChange, IceGUIDirection direction, GUIStyle separatorStyle, float width, float border)
            {
                float rWidth = direction switch
                {
                    IceGUIDirection.Top or IceGUIDirection.Bottom => rect.height,
                    _ => rect.width,
                };

                this.pos = pos = Mathf.Clamp(pos, width + border, rWidth - border);
                this.onPosChange = onPosChange;
                this.direction = direction;
                this.separatorStyle = separatorStyle;

                originRect = rect;

                mainRect = direction switch
                {
                    IceGUIDirection.Right => rect.MoveEdge(right: -pos),
                    IceGUIDirection.Left => rect.MoveEdge(left: pos),
                    IceGUIDirection.Bottom => rect.MoveEdge(bottom: -pos),
                    IceGUIDirection.Top => rect.MoveEdge(top: pos),
                    _ => rect,
                };

                subRect = direction switch
                {
                    IceGUIDirection.Right => rect.MoveEdge(left: rWidth - pos + width),
                    IceGUIDirection.Left => rect.MoveEdge(right: -(rWidth - pos + width)),
                    IceGUIDirection.Bottom => rect.MoveEdge(top: rWidth - pos + width),
                    IceGUIDirection.Top => rect.MoveEdge(bottom: -(rWidth - pos + width)),
                    _ => Rect.zero,
                };

                sepRect = direction switch
                {
                    IceGUIDirection.Right => rect.MoveEdge(left: rWidth - pos - border, right: border - pos + width),
                    IceGUIDirection.Left => rect.MoveEdge(left: pos - width - border, right: pos + border - rWidth),
                    IceGUIDirection.Bottom => rect.MoveEdge(top: rWidth - pos - border, bottom: border - pos + width),
                    IceGUIDirection.Top => rect.MoveEdge(top: pos - width - border, bottom: pos + border - rWidth),
                    _ => Rect.zero,
                };
            }
            void IDisposable.Dispose()
            {
                // 画Separator
                var id = GUIUtility.GetControlID(FocusType.Passive);
                bool focusing = GUIUtility.hotControl == id;
                EditorGUIUtility.AddCursorRect(focusing ? originRect : sepRect, direction switch
                {
                    IceGUIDirection.Top or IceGUIDirection.Bottom => MouseCursor.ResizeVertical,
                    _ => MouseCursor.ResizeHorizontal,
                });

                switch (E.type)
                {
                    case EventType.MouseDown:
                        if (Event.current.button == 0 && sepRect.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = id;

                            float mp = direction switch
                            {
                                IceGUIDirection.Top or IceGUIDirection.Bottom => E.mousePosition.y,
                                _ => E.mousePosition.x,
                            };

                            posOffset = direction switch
                            {
                                IceGUIDirection.Right or IceGUIDirection.Bottom => pos + mp,
                                _ => pos - mp,
                            };
                            E.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (focusing)
                        {
                            GUIUtility.hotControl = 0;
                            E.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (focusing)
                        {
                            float mp = direction switch
                            {
                                IceGUIDirection.Top or IceGUIDirection.Bottom => E.mousePosition.y,
                                _ => E.mousePosition.x,
                            };

                            onPosChange?.Invoke(pos = direction switch
                            {
                                IceGUIDirection.Right or IceGUIDirection.Bottom => posOffset - mp,
                                _ => posOffset + mp,
                            });
                            E.Use();
                        }
                        break;
                    case EventType.Repaint:
                        StyleBox(sepRect, separatorStyle, isHover: null, on: focusing);
                        break;
                }
            }
        }


        public static GUILayout.HorizontalScope Horizontal(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.HorizontalScope(style ?? GUIStyle.none, options);
        public static GUILayout.HorizontalScope Horizontal(params GUILayoutOption[] options) => new GUILayout.HorizontalScope(options);
        public static GUILayout.VerticalScope Vertical(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.VerticalScope(style ?? GUIStyle.none, options);
        public static GUILayout.VerticalScope Vertical(params GUILayoutOption[] options) => new GUILayout.VerticalScope(options);

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        public static GUILayout.AreaScope Area(Rect rect, GUIStyle style)
        {
            if (style == null) return new GUILayout.AreaScope(rect);
            var margin = style.margin;
            return new GUILayout.AreaScope(rect.MoveEdge(margin.left, -margin.right, margin.top, -margin.bottom), GUIContent.none, style);
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        public static GUILayout.AreaScope Area(Rect rect) => Area(rect, StlBackground);
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        public static GUILayout.AreaScope AreaRaw(Rect rect) => Area(rect, null);
        /// <summary>
        /// 在Using语句中使用的Scope，手动添加一个Prefix
        /// </summary>
        public static GUILayout.HorizontalScope Prefix(string text) { var res = Horizontal(); using (LabelWidth(EditorGUIUtility.labelWidth + 2)) EditorGUILayout.PrefixLabel(text, "button", StlPrefix); return res; }
        /// <summary>
        /// 在Using语句中使用的Scope，调整Label的宽度
        /// </summary>
        public static LabelWidthScope LabelWidth(float width) => new LabelWidthScope(width);
        /// <summary>
        /// 在Using语句中使用的Scope，condition满足时暂时禁用指定区域（变灰，无法交互）
        /// </summary>
        public static DisableScope Disable(bool disabled) => new DisableScope(disabled);
        /// <summary>
        /// 在Using语句中使用的Scope，用于标记ControlLabel的属性
        /// </summary>
        internal static ControlLabelScope ControlLabel(string label) => new ControlLabelScope(label, IceGUIUtility.CurrentThemeColor);
        /// <summary>
        /// 在Using语句中使用的Scope，用于标记ControlLabel的属性
        /// </summary>
        internal static ControlLabelScope ControlLabel(string label, string labelOverride) => new ControlLabelScope(string.IsNullOrEmpty(labelOverride) ? label : labelOverride, IceGUIUtility.CurrentThemeColor);
        /// <summary>
        /// 用于将一个区域拆分为可调整大小的两个区域
        /// </summary>
        public static SubAreaScope SubArea(Rect rect, float pos, Action<float> onPosChange, IceGUIDirection direction = IceGUIDirection.Right, GUIStyle separatorStyleOverride = null, float width = 4, float border = 2) => new SubAreaScope(rect, pos, onPosChange, direction, separatorStyleOverride ?? StlSubAreaSeparator, width, border);

        public static GUILayout.HorizontalScope HORIZONTAL => Horizontal();
        public static GUILayout.VerticalScope VERTICAL => Vertical();
        public static GUILayout.VerticalScope BOX => Vertical(StlBox);
        public static GUILayout.VerticalScope NODE => Vertical(StlNode);
        public static GUILayout.VerticalScope HIGHLIGHT => Vertical(StlHighlight);
        public static GUILayout.VerticalScope GROUP => Vertical(StlGroup);
        public static GUILayout.HorizontalScope DOCK => Horizontal(StlDock);
        /// <summary>
        /// 在Using语句中使用的Scope，指定区域的Field对用户输入延迟响应，作用于 IntField, FloatField, TextField, Vector2Field
        /// </summary>
        public static DelayedScope DELAYED => new DelayedScope();
        /// <summary>
        /// 在Using语句中使用的Scope，检查块中控件是否变化，若有变化，Changed置为true
        /// </summary>
        public static ChangeCheckScope GUICHECK => new ChangeCheckScope();
        #endregion

        #region Utility
        /// <summary>
        /// 在CHECK块中控件是否变化
        /// </summary>
        public static bool GUIChanged => GUI.changed;

        static readonly GUIContent _tempText = new GUIContent();
        static readonly GUIContent _tempImage = new GUIContent();
        static readonly GUIContent _tempTextImage = new GUIContent();
        public static GUIContent TempContent(string text)
        {
            _tempText.text = text;
            _tempText.tooltip = string.Empty;
            return _tempText;
        }
        public static GUIContent TempContent(string text, string tooltip)
        {
            _tempText.text = text;
            _tempText.tooltip = tooltip;
            return _tempText;
        }
        public static GUIContent TempContent(Texture image)
        {
            _tempImage.image = image;
            _tempImage.tooltip = string.Empty;
            return _tempImage;
        }
        public static GUIContent TempContent(Texture image, string tooltip)
        {
            _tempImage.image = image;
            _tempImage.tooltip = tooltip;
            return _tempImage;
        }
        public static GUIContent TempContent(string text, Texture image)
        {
            _tempTextImage.text = text;
            _tempTextImage.image = image;
            return _tempTextImage;
        }

        public static Rect GetRect(params GUILayoutOption[] options) => GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, options);
        public static Rect GetRect(GUIStyle style, params GUILayoutOption[] options) => GUILayoutUtility.GetRect(GUIContent.none, style, options);
        public static Rect GetRect(GUIContent content, GUIStyle style, params GUILayoutOption[] options) => GUILayoutUtility.GetRect(content, style, options);
        public static Rect GetRect(float aspect, params GUILayoutOption[] options) => GUILayoutUtility.GetAspectRect(aspect, options);
        public static Rect GetRect(float aspect, GUIStyle style, params GUILayoutOption[] options) => GUILayoutUtility.GetAspectRect(aspect, style, options);
        public static Rect GetRect(float width, float height, params GUILayoutOption[] options) => GUILayoutUtility.GetRect(width, height, options);
        public static Rect GetRect(float width, float height, GUIStyle style, params GUILayoutOption[] options) => GUILayoutUtility.GetRect(width, height, style, options);
        public static Rect GetLastRect() => Event.current.type == EventType.Repaint ? GUILayoutUtility.GetLastRect() : throw new IceGUIException("Can't call GetLastRect() out of Repaint Event");

        public static GUIStyle GetStyle(string key = null, Func<GUIStyle> itor = null) => IceGUIStyleBox.GetStyle(key, itor);

        public static Event E => Event.current;
        #endregion

        #region Drawing Elements
        /// <summary>
        /// 用特定Style填充区域
        /// </summary>
        public static Rect StyleBox(Rect rect, GUIStyle style, string text = null, bool hasMargin = false, bool? isHover = false, bool isActive = false, bool on = false, bool hasKeyboardFocus = false) => StyleBox(rect, style, string.IsNullOrEmpty(text) ? GUIContent.none : new GUIContent(text), hasMargin, isHover, isActive, on, hasKeyboardFocus);
        /// <summary>
        /// 用特定Style填充区域
        /// </summary>
        public static Rect StyleBox(Rect rect, GUIStyle style, GUIContent content, bool hasMargin = false, bool? isHover = false, bool isActive = false, bool on = false, bool hasKeyboardFocus = false)
        {
            if (style == null) return rect;
            var margin = style.margin;
            var res = hasMargin ? rect.MoveEdge(margin.left, -margin.right, margin.top, -margin.bottom) : rect;
            if (Event.current.type == EventType.Repaint)
            {
                style.Draw(res, content, isHover ?? rect.Contains(E.mousePosition), isActive, on, hasKeyboardFocus);
            }
            return res;
        }

        public static void Header(string text, string colorExp) => GUILayout.Label(text.Color(colorExp), StlHeader);
        public static void Header(string text, Color color) => GUILayout.Label(text.Color(color), StlHeader);
        public static void Header(string text) => GUILayout.Label(text.Color(IceGUIUtility.CurrentThemeColor), StlHeader);

        public static void SectionHeader(string text, params GUILayoutOption[] options) => StyleBox(GetRect(TempContent(text), StlSectionHeader, options), StlSectionHeader, text, on: true);

        public static void Label(string text, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Label(text, style, options);
        public static void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Label(content, style, options);
        public static void Label(string text, params GUILayoutOption[] options) => Label(text, StlLabel, options);
        public static void Label(GUIContent content, params GUILayoutOption[] options) => Label(content, StlLabel, options);
        public static void LabelError(string text, params GUILayoutOption[] options) => Label(text, StlError, options);

        /// <summary>
        /// 画一个Texture Preview
        /// </summary>
        /// <param name="rect">尺寸</param>
        public static void TextureBox(Texture texture, Rect rect) => EditorGUI.DrawPreviewTexture(rect, texture);
        /// <summary>
        /// 画一个自适应Layout的Texture
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public static void TextureBox(Texture texture, float width, float height) => EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false)), texture);
        /// <summary>
        /// 画一个自适应Layout的Texture
        /// </summary>
        /// <param name="expanded">是否自动扩展</param>
        public static void TextureBox(Texture texture, bool expanded = false) => EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width)), texture);
        /// <summary>
        /// 画一个自适应Layout的透明Texture
        /// </summary>
        /// <param name="expanded">是否自动扩展</param>
        public static void TextureBoxTransparent(Texture texture, bool expanded = false) => EditorGUI.DrawTextureTransparent(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width)), texture);

        public static void Space(float pixels) => GUILayout.Space(pixels);
        public static void Space() => GUILayout.FlexibleSpace();
        #endregion

        #region Button & Fields
        static readonly GUILayoutOption[] defaultOptions = new GUILayoutOption[] { GUILayout.MinWidth(EditorGUIUtility.singleLineHeight) };
        internal static GUILayoutOption[] CheckOptions(ref GUILayoutOption[] options) => options = options.Length > 0 ? options : defaultOptions;

        public static bool Button(string text, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Button(text, styleOverride ?? StlButton, options);
        public static bool Button(string text, params GUILayoutOption[] options) => Button(text, null, options);
        public static bool Button(GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Button(content, styleOverride ?? StlButton, options);
        public static bool Button(GUIContent content, params GUILayoutOption[] options) => Button(content, null, options);

        public static bool SectionHeader(AnimBool val, string label, params GUILayoutOption[] options) => val.target = GUILayout.Toggle(val.target, label, StlSectionHeader, options);

        public static bool _Toggle(bool val, GUIStyle styleOverride, params GUILayoutOption[] options) => ControlLabelScope.HasLabel ? EditorGUILayout.Toggle(ControlLabelScope.Label, val, styleOverride ?? "toggle") : EditorGUILayout.Toggle(val, styleOverride ?? "toggle");
        public static bool _Toggle(bool val, params GUILayoutOption[] options) => _Toggle(val, null, options);
        public static bool _Toggle(string label, bool val, GUIStyle styleOverride, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Toggle(val, styleOverride, options); }
        public static bool _Toggle(string label, bool val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Toggle(val, options); }

        public static bool Toggle(ref bool val, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _Toggle(val, styleOverride, options);
        public static bool Toggle(ref bool val, params GUILayoutOption[] options) => val = _Toggle(val, options);
        public static bool Toggle(string label, ref bool val, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _Toggle(label, val, styleOverride, options);
        public static bool Toggle(string label, ref bool val, params GUILayoutOption[] options) => val = _Toggle(label, val, options);

        public static bool _ToggleLeft(bool val, string text, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Toggle(val, text, styleOverride ?? "toggle", options);
        public static bool _ToggleLeft(bool val, string text, params GUILayoutOption[] options) => GUILayout.Toggle(val, text, "toggle", options);
        public static bool _ToggleLeft(bool val, GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => GUILayout.Toggle(val, content, styleOverride ?? "toggle", options);
        public static bool _ToggleLeft(bool val, GUIContent content, params GUILayoutOption[] options) => GUILayout.Toggle(val, content, "toggle", options);
        public static bool ToggleLeft(ref bool val, string text, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _ToggleLeft(val, text, styleOverride, options);
        public static bool ToggleLeft(ref bool val, string text, params GUILayoutOption[] options) => val = _ToggleLeft(val, text, options);
        public static bool ToggleLeft(ref bool val, GUIContent content, GUIStyle styleOverride, params GUILayoutOption[] options) => val = _ToggleLeft(val, content, styleOverride, options);
        public static bool ToggleLeft(ref bool val, GUIContent content, params GUILayoutOption[] options) => val = _ToggleLeft(val, content, options);

        public static bool ToggleButton(string text, bool on, GUIStyle styleOverride, params GUILayoutOption[] options) => on != _ToggleLeft(on, text, styleOverride ?? StlButton, options);
        public static bool ToggleButton(string text, bool on, params GUILayoutOption[] options) => ToggleButton(text, on, null, options);
        public static bool ToggleButton(GUIContent content, bool on, GUIStyle styleOverride, params GUILayoutOption[] options) => on != _ToggleLeft(on, content, styleOverride ?? StlButton, options);
        public static bool ToggleButton(GUIContent content, bool on, params GUILayoutOption[] options) => ToggleButton(content, on, null, options);
        public static bool ToggleButton(Rect rect, string text, bool on, GUIStyle style) => on != GUI.Toggle(rect, on, text, style);

        public static int _IntField(int val, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedIntField(ControlLabelScope.Label, val, options) : EditorGUILayout.DelayedIntField(val, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.IntField(ControlLabelScope.Label, val, options) : EditorGUILayout.IntField(val, options)); }
        public static int _IntField(string label, int val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _IntField(val, options); }
        public static int IntField(ref int val, params GUILayoutOption[] options) => val = _IntField(val, options);
        public static int IntField(string label, ref int val, params GUILayoutOption[] options) => val = _IntField(label, val, options);

        public static int _IntSlider(int val, int min, int max, params GUILayoutOption[] options) { CheckOptions(ref options); return ControlLabelScope.HasLabel ? (int)EditorGUILayout.Slider(ControlLabelScope.Label, val, min, max, options) : (int)EditorGUILayout.Slider(val, min, max, options); }
        public static int _IntSlider(string label, int val, int min, int max, params GUILayoutOption[] options) { using (ControlLabel(label)) return _IntSlider(val, min, max, options); }
        public static int IntSlider(ref int val, int min, int max, params GUILayoutOption[] options) => val = _IntSlider(val, min, max, options);
        public static int IntSlider(string label, ref int val, int min, int max, params GUILayoutOption[] options) => val = _IntSlider(label, val, min, max, options);

        public static float _FloatField(float val, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedFloatField(ControlLabelScope.Label, val, options) : EditorGUILayout.DelayedFloatField(val, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.FloatField(ControlLabelScope.Label, val, options) : EditorGUILayout.FloatField(val, options)); }
        public static float _FloatField(string label, float val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _FloatField(val, options); }
        public static float FloatField(ref float val, params GUILayoutOption[] options) => val = _FloatField(val, options);
        public static float FloatField(string label, ref float val, params GUILayoutOption[] options) => val = _FloatField(label, val, options);

        public static float _Slider(float val, float min = 0, float max = 1, params GUILayoutOption[] options) { CheckOptions(ref options); return ControlLabelScope.HasLabel ? EditorGUILayout.Slider(ControlLabelScope.Label, val, min, max, options) : EditorGUILayout.Slider(val, min, max, options); }
        public static float _Slider(string label, float val, float min = 0, float max = 1, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Slider(val, min, max, options); }
        public static float Slider(ref float val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Slider(val, min, max, options);
        public static float Slider(string label, ref float val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Slider(label, val, min, max, options);

        public static string _TextField(string val, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedTextField(ControlLabelScope.Label, val, options) : EditorGUILayout.DelayedTextField(val, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.TextField(ControlLabelScope.Label, val, options) : EditorGUILayout.TextField(val, options)); }
        public static string _TextField(string label, string val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _TextField(val, options); }
        public static string TextField(ref string val, params GUILayoutOption[] options) => val = _TextField(val, options);
        public static string TextField(string label, ref string val, params GUILayoutOption[] options) => val = _TextField(label, val, options);

        public static void MinMaxSlider(ref float l, ref float r, float min = 0, float max = 1, params GUILayoutOption[] options) { CheckOptions(ref options); EditorGUILayout.MinMaxSlider(ref l, ref r, min, max, options); }
        static string DoVectorUnitField(ref float val, string label, string controlName)
        {
            GUI.SetNextControlName(controlName);
            if (!string.IsNullOrEmpty(label))
            {
                using (LabelWidth(StlPrefix.CalcSize(TempContent(label)).x)) using (ControlLabel(label)) FloatField(ref val);
            }
            else
            {
                val = DelayedScope.inScope ? EditorGUILayout.DelayedFloatField(val, defaultOptions) : EditorGUILayout.FloatField(val, defaultOptions);
            }
            return controlName;
        }

        public static Vector2 _Vector2Field(Vector2 val, string xLabel = null, string yLabel = null)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                string controlNameX = DoVectorUnitField(ref val.x, xLabel, "Vector2FieldX");
                string controlNameY = DoVectorUnitField(ref val.y, yLabel, "Vector2FieldY");

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
        public static Vector2 _Vector2Field(string label, Vector2 val, string xLabel = null, string yLabel = null) { using (ControlLabel(label)) return _Vector2Field(val, xLabel, yLabel); }
        public static Vector2 Vector2Field(ref Vector2 val, string xLabel = null, string yLabel = null) => val = _Vector2Field(val, xLabel, yLabel);
        public static Vector2 Vector2Field(string label, ref Vector2 val, string xLabel = null, string yLabel = null) => val = _Vector2Field(label, val, xLabel, yLabel);

        public static Vector2 _Vector2Slider(Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options)
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
                using (GUICHECK)
                {
                    EditorGUILayout.MinMaxSlider(ref val.x, ref val.y, min, max, options.Length > 0 ? options : new GUILayoutOption[] { GUILayout.MinWidth(-4) });

                    if (GUIChanged) GUI.FocusControl(controlName);
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
        public static Vector2 _Vector2Slider(string label, Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options) { using (ControlLabel(label)) return _Vector2Slider(val, min, max, options); }
        public static Vector2 Vector2Slider(ref Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Vector2Slider(val, min, max, options);
        public static Vector2 Vector2Slider(string label, ref Vector2 val, float min = 0, float max = 1, params GUILayoutOption[] options) => val = _Vector2Slider(label, val, min, max, options);

        public static Vector3 _Vector3Field(Vector3 val, string xLabel = null, string yLabel = null, string zLabel = null)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                string controlNameX = DoVectorUnitField(ref val.x, xLabel, "Vector3FieldX");
                string controlNameY = DoVectorUnitField(ref val.y, yLabel, "Vector3FieldY");
                string controlNameZ = DoVectorUnitField(ref val.z, zLabel, "Vector3FieldZ");

                if (ControlLabelScope.HasLabel)
                {
                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlNameX || focusedControl == controlNameY || focusedControl == controlNameZ;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, ControlLabelScope.Label, on, StlPrefix) && !on) GUI.FocusControl(controlNameX);
                }
            }
            return val;
        }
        public static Vector3 _Vector3Field(string label, Vector3 val, string xLabel = null, string yLabel = null, string zLabel = null) { using (ControlLabel(label)) return _Vector3Field(val, xLabel, yLabel, zLabel); }
        public static Vector3 Vector3Field(ref Vector3 val, string xLabel = null, string yLabel = null, string zLabel = null) => val = _Vector3Field(val, xLabel, yLabel, zLabel);
        public static Vector3 Vector3Field(string label, ref Vector3 val, string xLabel = null, string yLabel = null, string zLabel = null) => val = _Vector3Field(label, val, xLabel, yLabel, zLabel);

        public static Vector4 _Vector4Field(Vector4 val, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                string controlNameX = DoVectorUnitField(ref val.x, xLabel, "Vector4FieldX");
                string controlNameY = DoVectorUnitField(ref val.y, yLabel, "Vector4FieldY");
                string controlNameZ = DoVectorUnitField(ref val.z, zLabel, "Vector4FieldZ");
                string controlNameW = DoVectorUnitField(ref val.w, wLabel, "Vector4FieldW");

                if (ControlLabelScope.HasLabel)
                {
                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlNameX || focusedControl == controlNameY || focusedControl == controlNameZ || focusedControl == controlNameW;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, ControlLabelScope.Label, on, StlPrefix) && !on) GUI.FocusControl(controlNameX);
                }
            }
            return val;
        }
        public static Vector4 _Vector4Field(string label, Vector4 val, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) { using (ControlLabel(label)) return _Vector4Field(val, xLabel, yLabel, zLabel, wLabel); }
        public static Vector4 Vector4Field(ref Vector4 val, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) => val = _Vector4Field(val, xLabel, yLabel, zLabel, wLabel);
        public static Vector4 Vector4Field(string label, ref Vector4 val, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) => val = _Vector4Field(label, val, xLabel, yLabel, zLabel, wLabel);

        static int DoVectorIntUnitField(int val, string label, string controlName, out string controlNameOut)
        {
            GUI.SetNextControlName(controlName);
            controlNameOut = controlName;
            if (!string.IsNullOrEmpty(label))
            {
                using (LabelWidth(StlPrefix.CalcSize(TempContent(label)).x)) using (ControlLabel(label)) return _IntField(val);
            }
            else
            {
                return DelayedScope.inScope ? EditorGUILayout.DelayedIntField(val, defaultOptions) : EditorGUILayout.IntField(val, defaultOptions);
            }
        }

        public static Vector2Int _Vector2IntField(Vector2Int val, string xLabel = null, string yLabel = null)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                val.x = DoVectorIntUnitField(val.x, xLabel, "Vector2IntFieldX", out string controlNameX);
                val.y = DoVectorIntUnitField(val.y, yLabel, "Vector2IntFieldY", out string controlNameY);

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
        public static Vector2Int _Vector2IntField(string label, Vector2Int val, string xLabel = null, string yLabel = null) { using (ControlLabel(label)) return _Vector2IntField(val, xLabel, yLabel); }
        public static Vector2Int Vector2IntField(ref Vector2Int val, string xLabel = null, string yLabel = null) => val = _Vector2IntField(val, xLabel, yLabel);
        public static Vector2Int Vector2IntField(string label, ref Vector2Int val, string xLabel = null, string yLabel = null) => val = _Vector2IntField(label, val, xLabel, yLabel);

        public static Vector3Int _Vector3IntField(Vector3Int val, string xLabel = null, string yLabel = null, string zLabel = null)
        {
            using (HORIZONTAL)
            {
                Rect labelRect = new Rect();
                if (ControlLabelScope.HasLabel) labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                val.x = DoVectorIntUnitField(val.x, xLabel, "Vector3IntFieldX", out string controlNameX);
                val.y = DoVectorIntUnitField(val.y, yLabel, "Vector3IntFieldY", out string controlNameY);
                val.z = DoVectorIntUnitField(val.z, zLabel, "Vector3IntFieldZ", out string controlNameZ);

                if (ControlLabelScope.HasLabel)
                {
                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlNameX || focusedControl == controlNameY || focusedControl == controlNameZ;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, ControlLabelScope.Label, on, StlPrefix) && !on) GUI.FocusControl(controlNameX);
                }
            }
            return val;
        }
        public static Vector3Int _Vector3IntField(string label, Vector3Int val, string xLabel = null, string yLabel = null, string zLabel = null) { using (ControlLabel(label)) return _Vector3IntField(val, xLabel, yLabel, zLabel); }
        public static Vector3Int Vector3IntField(ref Vector3Int val, string xLabel = null, string yLabel = null, string zLabel = null) => val = _Vector3IntField(val, xLabel, yLabel, zLabel);
        public static Vector3Int Vector3IntField(string label, ref Vector3Int val, string xLabel = null, string yLabel = null, string zLabel = null) => val = _Vector3IntField(label, val, xLabel, yLabel, zLabel);

        public static EnumType _EnumPopup<EnumType>(EnumType val, params GUILayoutOption[] options) where EnumType : Enum { CheckOptions(ref options); return ControlLabelScope.HasLabel ? (EnumType)EditorGUILayout.EnumPopup(ControlLabelScope.Label, val, options) : (EnumType)EditorGUILayout.EnumPopup(val, options); }
        public static EnumType _EnumPopup<EnumType>(string label, EnumType val, params GUILayoutOption[] options) where EnumType : Enum { using (ControlLabel(label)) return _EnumPopup(val, options); }
        public static EnumType EnumPopup<EnumType>(ref EnumType val, params GUILayoutOption[] options) where EnumType : Enum => val = _EnumPopup(val, options);
        public static EnumType EnumPopup<EnumType>(string label, ref EnumType val, params GUILayoutOption[] options) where EnumType : Enum => val = _EnumPopup(label, val, options);

        public static ObjType _ObjectField<ObjType>(ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object { CheckOptions(ref options); return ControlLabelScope.HasLabel ? (ObjType)EditorGUILayout.ObjectField(ControlLabelScope.Label, val, typeof(ObjType), allowSceneObjects, options) : (ObjType)EditorGUILayout.ObjectField(val, typeof(ObjType), allowSceneObjects, options); }
        public static ObjType _ObjectField<ObjType>(string label, ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object { using (ControlLabel(label)) return _ObjectField(val, allowSceneObjects, options); }
        public static ObjType ObjectField<ObjType>(ref ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object => val = _ObjectField(val, allowSceneObjects, options);
        public static ObjType ObjectField<ObjType>(string label, ref ObjType val, bool allowSceneObjects = false, params GUILayoutOption[] options) where ObjType : UnityEngine.Object => val = _ObjectField(label, val, allowSceneObjects, options);

        public static Color _ColorField(Color val, params GUILayoutOption[] options) { CheckOptions(ref options); return ControlLabelScope.HasLabel ? EditorGUILayout.ColorField(ControlLabelScope.Label, val, options) : EditorGUILayout.ColorField(val, options); }
        public static Color _ColorField(string label, Color val, params GUILayoutOption[] options) { using (ControlLabel(label)) return _ColorField(val, options); }
        public static Color ColorField(ref Color val, params GUILayoutOption[] options) => val = _ColorField(val, options);
        public static Color ColorField(string label, ref Color val, params GUILayoutOption[] options) => val = _ColorField(label, val, options);

        /// <summary>
        /// 不带预览功能的 Texture2D Field
        /// </summary>
        /// <returns>return true on changed</returns>
        public static bool TextureFieldNoPreview(string label, ref Texture2D tex, params GUILayoutOption[] options)
        {
            CheckOptions(ref options);
            using (GUICHECK)
            {
                using (ControlLabel(label)) tex = (Texture2D)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(options), label, tex, typeof(Texture2D), false);
                return GUIChanged;
            }
        }

        public static bool IceButton(string text, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(TempContent(text, tooltip), StlIce, options);
        public static bool IceButton(string text, bool on, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(TempContent(on ? $"{text.Color(IceGUIUtility.CurrentThemeColor)}" : text, tooltip), StlIce, options);
        public static bool IceButton(Texture texture, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(TempContent(texture, tooltip), StlIce, options);
        public static bool IceButton(GUIContent content, params GUILayoutOption[] options) => GUILayout.Button(content, StlIce, options);
        public static bool _IceToggle(string text, bool val, string tooltip = null, params GUILayoutOption[] options) => GUILayout.Button(TempContent(val ? $"{text.Color(IceGUIUtility.CurrentThemeColor)}" : text, tooltip), StlIce) ? !val : val;
        public static bool IceToggle(string text, ref bool val, string tooltip = null, params GUILayoutOption[] options) => val = _IceToggle(text, val, tooltip, options);
        #endregion
    }
}