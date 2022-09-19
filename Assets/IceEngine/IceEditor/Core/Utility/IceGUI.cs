using System;
using System.Collections.Generic;

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
        public static GUIStyle StlBackground => _stlBackground?.Check() ?? (_stlBackground = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), stretchHeight = true, alignment = TextAnchor.MiddleCenter, }); static GUIStyle _stlBackground;
        public static GUIStyle StlLabel => _stlLabel?.Check() ?? (_stlLabel = new GUIStyle("label") { richText = true, wordWrap = true, stretchWidth = false }); static GUIStyle _stlLabel;
        public static GUIStyle StlHeader => _stlHeader?.Check() ?? (_stlHeader = new GUIStyle("LODRendererRemove") { margin = new RectOffset(5, 3, 6, 2), fontSize = 12, fontStyle = FontStyle.Normal, richText = true, }); static GUIStyle _stlHeader;
        public static GUIStyle StlDock => _stlDock?.Check() ?? (_stlDock = new GUIStyle("dockarea") { padding = new RectOffset(1, 1, 1, 1), contentOffset = new Vector2(0f, 0f), }); static GUIStyle _stlDock;
        public static GUIStyle StlGroup => _stlGroup?.Check() ?? (_stlGroup = new GUIStyle("NotificationBackground") { border = new RectOffset(16, 16, 13, 13), margin = new RectOffset(6, 6, 6, 6), padding = new RectOffset(10, 10, 6, 6), }); static GUIStyle _stlGroup;
        public static GUIStyle StlBox => _stlBox?.Check() ?? (_stlBox = new GUIStyle("window") { margin = new RectOffset(4, 4, 4, 4), padding = new RectOffset(6, 6, 6, 6), contentOffset = new Vector2(0f, 0f), stretchWidth = false, stretchHeight = false, }); static GUIStyle _stlBox;
        public static GUIStyle StlNode => _stlNode?.Check() ?? (_stlNode = new GUIStyle("flow node 0") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); static GUIStyle _stlNode;
        public static GUIStyle StlHighlight => _stlHighlight?.Check() ?? (_stlHighlight = new GUIStyle("LightmapEditorSelectedHighlight") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 10, 10), overflow = new RectOffset(0, 0, 0, 0), }); static GUIStyle _stlHighlight;
        public static GUIStyle StlButton => _stlButton?.Check() ?? (_stlButton = new GUIStyle("button") { richText = true, }); static GUIStyle _stlButton;
        public static GUIStyle StlError => _stlError?.Check() ?? (_stlError = new GUIStyle("Wizard Error") { border = new RectOffset(32, 0, 32, 0), padding = new RectOffset(32, 0, 7, 7), fixedHeight = 0f, }.Initialize(stl => { stl.normal.textColor = new Color(1f, 0.8469602f, 0f); })); static GUIStyle _stlError;
        public static GUIStyle StlIce => _stlIce?.Check() ?? (_stlIce = new GUIStyle("BoldTextField") { padding = new RectOffset(3, 3, 2, 2), fontSize = 11, richText = true, fixedHeight = 0f, stretchWidth = false, imagePosition = ImagePosition.ImageLeft, fontStyle = FontStyle.Normal }); static GUIStyle _stlIce;
        public static GUIStyle StlSectionHeader => IceGUIUtility.HasPack ? IceGUIUtility.CurrentPack.StlSectionHeader : _stlSectionHeader?.Check() ?? (_stlSectionHeader = IceGUIUtility.GetStlSectionHeader(IceGUIUtility.DefaultThemeColor)); static GUIStyle _stlSectionHeader;
        public static GUIStyle StlPrefix => IceGUIUtility.HasPack ? IceGUIUtility.CurrentPack.StlPrefix : _stlPrefix?.Check() ?? (_stlPrefix = IceGUIUtility.GetStlPrefix(IceGUIUtility.DefaultThemeColor)); static GUIStyle _stlPrefix;
        public static GUIStyle StlSubAreaSeparator => IceGUIUtility.HasPack ? IceGUIUtility.CurrentPack.StlSubAreaSeparator : _stlSubAreaSeparator?.Check() ?? (_stlSubAreaSeparator = IceGUIUtility.GetStlSubAreaSeparator(IceGUIUtility.DefaultThemeColor)); static GUIStyle _stlSubAreaSeparator;
        public static GUIStyle StlViewportToolButton => _stlViewportToolButton?.Check() ?? (_stlViewportToolButton = new GUIStyle("HoverHighlight") { alignment = TextAnchor.MiddleCenter, contentOffset = new Vector2(1f, 0f), fixedWidth = 0f, fixedHeight = 0f, }); static GUIStyle _stlViewportToolButton;
        public static GUIStyle StlSelectedBox => _stlSelectedBox?.Check() ?? (_stlSelectedBox = new GUIStyle("LightmapEditorSelectedHighlight") { overflow = new RectOffset(6, 6, 6, 6), }); static GUIStyle _stlSelectedBox;
        public static GUIStyle StlGraphPortName => _stlGraphPortName?.Check() ?? (_stlGraphPortName = new GUIStyle("label") { margin = new RectOffset(-1, -1, 0, 0), padding = new RectOffset(0, 0, 0, 0), fontSize = 9, alignment = TextAnchor.MiddleCenter, }.Initialize(stl => { stl.normal.textColor = new Color(0.3962264f, 0.3962264f, 0.3962264f); })); static GUIStyle _stlGraphPortName;
        public static GUIStyle StlGraphPortLabel => _stlGraphPortLabel?.Check() ?? (_stlGraphPortLabel = new GUIStyle("ShurikenValue") { margin = new RectOffset(1, 1, 2, 2), padding = new RectOffset(3, 3, 0, 0), fontSize = 12, alignment = TextAnchor.MiddleCenter, fixedHeight = 0f, }); static GUIStyle _stlGraphPortLabel;
        public static GUIStyle StlSearchTextField => _stlSearchTextField?.Check() ?? (_stlSearchTextField = new GUIStyle("SearchTextField") { padding = new RectOffset(14, 3, 2, 1), fontSize = 12, fixedHeight = 0f, }); static GUIStyle _stlSearchTextField;
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
        public class ControlLabelScope : IDisposable
        {
            public static bool HasLabel => !string.IsNullOrEmpty(label);
            public static string Label => label;

            static string label = null;

            string originLabel;

            public ControlLabelScope(string labelVal)
            {
                originLabel = label;
                label = labelVal;
            }
            void IDisposable.Dispose()
            {
                label = originLabel;
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
                bool focusing = GUIHotControl == id;
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
                            GUIHotControl = id;

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
                        if (focusing && E.button == 0)
                        {
                            GUIHotControl = 0;
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
        /// <summary>
        /// 指定一个Area
        /// </summary>
        public class AreaScope : IDisposable
        {
            public AreaScope(Rect areaRect) : this(areaRect, GUIContent.none, GUIStyle.none) { }
            public AreaScope(Rect areaRect, GUIContent content, GUIStyle style)
            {
                GUILayout.BeginArea(areaRect, content, style);
                ViewportScope.areaStack.Push(areaRect);
            }
            void IDisposable.Dispose()
            {
                ViewportScope.areaStack.Pop();
                GUILayout.EndArea();
            }
        }
        /// <summary>
        /// 指定一个ScrollArea
        /// </summary>
        public class ScrollScope : IDisposable
        {
            public Vector2 scrollPosition;
            public ScrollScope(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options)
            {
                Vector2 clipPos = EditorGUIUtility.GUIToScreenPoint(Vector2.zero);
                this.scrollPosition = GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background, options);
                clipPos = EditorGUIUtility.GUIToScreenPoint(Vector2.zero) - clipPos;
                ViewportScope.areaStack.Push(new Rect(clipPos, new Vector2(Screen.width, Screen.height + scrollPosition.y)));
            }

            void IDisposable.Dispose()
            {
                ViewportScope.areaStack.Pop();
                GUILayout.EndScrollView();
            }
        }
        /// <summary>
        /// 一个可缩放可移动的工作视图，目前和ScrollScope不兼容
        /// </summary>
        public class ViewportScope : IDisposable
        {
            /// <summary>
            /// 执行剔除的Rect
            /// </summary>
            public Rect ClipRect { get; private set; }
            /// <summary>
            /// 画布Rect
            /// </summary>
            public Rect CanvasRect { get; private set; }

            bool inUtilityWindow = false;
            Matrix4x4 originMatrix;
            Vector2 screenSize;
            public Action onCloseScope;

            public static int clipStackCount = 0;
            public static Stack<Rect> areaStack = new Stack<Rect>();
            static Stack<Rect> areaStackTemp = new Stack<Rect>();

            /// <summary>
            /// 一个可缩放可移动的工作视图
            /// </summary>
            /// <param name="workspace">工作区Rect</param>
            /// <param name="workspaceSize">工作区的尺寸</param>
            /// <param name="canvasSize">画布的尺寸</param>
            /// <param name="scale">画布缩放值</param>
            /// <param name="offset">画布偏移</param>
            public ViewportScope(Rect workspace, float workspaceSize, float canvasSize, float scale, Vector2 offset, bool inUtilityWindow)
            {
                // 解除外部剔除
                screenSize = new Vector2(Screen.width, Screen.height);
                Rect sc = GUIUtility.GUIToScreenRect(workspace);
                while (areaStack.Count > 0)
                {
                    areaStackTemp.Push(areaStack.Pop());
                    GUI.EndClip();
                }

                // Tab窗口内需要撤销最外层Clip
                this.inUtilityWindow = inUtilityWindow;
                if (!inUtilityWindow) GUI.EndClip();

                workspace = GUIUtility.ScreenToGUIRect(sc);

                // 数学运算
                float workspaceScale = workspaceSize / canvasSize;
                CanvasRect = new Rect(Vector2.zero, Vector2.one * canvasSize);

                // 矩阵运算
                {
                    //var baseOffset = GUIUtility.ScreenToGUIPoint(baseScreenRect.position);
                    originMatrix = GUI.matrix;
                    GUI.matrix =
                        // 还原并平移
                        Matrix4x4.Translate(workspace.center - Vector2.one * 0.5f * workspaceSize * scale + offset * workspaceScale * scale/* - baseOffset*/) *
                        // 自定义缩放
                        Matrix4x4.Scale(Vector3.one * workspaceScale * scale) *
                        // 移回原点
                        //Matrix4x4.Translate(baseOffset) *
                        // 矩阵支持多层叠加
                        originMatrix;
                }

                // 剔除运算
                {
                    var clipSize = workspace.size / (workspaceScale * scale);
                    ClipRect = new Rect(Vector2.one * canvasSize * 0.5f - offset - clipSize * 0.5f, clipSize);
                    GUI.BeginClip(ClipRect, -ClipRect.position, Vector2.zero, false);
                }
            }
            void IDisposable.Dispose()
            {
                onCloseScope?.Invoke();

                GUI.EndClip();
                GUI.matrix = originMatrix;

                // Tab窗口内需要还原最外层Clip
                if (!inUtilityWindow) GUI.BeginClip(new Rect(Vector2.up * 21, screenSize));

                while (areaStackTemp.Count > 0)
                {
                    var r = areaStackTemp.Pop();
                    GUI.BeginClip(r);
                    areaStack.Push(r);
                }
            }
        }

        /// <summary>
        /// 暂时改变 GUI.Color
        /// </summary>
        public class GUIColorScope : IDisposable
        {
            Color originColor;
            public GUIColorScope(Color color)
            {
                originColor = GUI.color;
                GUI.color = color;
            }
            void IDisposable.Dispose()
            {
                GUI.color = originColor;
            }
        }
        /// <summary>
        /// 暂时改变 Handles.Color
        /// </summary>
        public class HandlesColorScope : IDisposable
        {
            Color originColor;
            public HandlesColorScope(Color color)
            {
                originColor = Handles.color;
                Handles.color = color;
            }
            void IDisposable.Dispose()
            {
                Handles.color = originColor;
            }
        }
        /// <summary>
        /// 暂时改变 IceGUIUtility.themeColorOverride
        /// </summary>
        public class ThemeColorScope : IDisposable
        {
            Color? originColor;
            Color originTextColor;
            Color originPackColor;

            GUIStyle LabelStyle => IceGUIUtility.LabelStyle;
            public ThemeColorScope(Color color)
            {
                originColor = IceGUIUtility.themeColorOverride;
                originTextColor = LabelStyle.focused.textColor;
                if (IceGUIUtility.HasPack)
                {
                    originPackColor = IceGUIUtility.CurrentPack.ThemeColor;
                    IceGUIUtility.CurrentPack.ThemeColor = color;
                }
                IceGUIUtility.themeColorOverride = color;
                LabelStyle.focused.textColor = color;
            }
            void IDisposable.Dispose()
            {
                LabelStyle.focused.textColor = originTextColor;
                IceGUIUtility.themeColorOverride = originColor;
                if (IceGUIUtility.HasPack) IceGUIUtility.CurrentPack.ThemeColor = originPackColor;
            }
        }



        public static GUILayout.HorizontalScope Horizontal(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.HorizontalScope(style ?? GUIStyle.none, options);
        public static GUILayout.HorizontalScope Horizontal(params GUILayoutOption[] options) => new GUILayout.HorizontalScope(options);
        public static GUILayout.VerticalScope Vertical(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.VerticalScope(style ?? GUIStyle.none, options);
        public static GUILayout.VerticalScope Vertical(params GUILayoutOption[] options) => new GUILayout.VerticalScope(options);

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        public static AreaScope Area(Rect rect, GUIStyle style)
        {
            if (style == null) return new AreaScope(rect);
            var margin = style.margin;
            return new AreaScope(rect.MoveEdge(margin.left, -margin.right, margin.top, -margin.bottom), GUIContent.none, style);
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        public static AreaScope Area(Rect rect) => Area(rect, StlBackground);
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Layout Area
        /// </summary>
        public static AreaScope AreaRaw(Rect rect) => Area(rect, null);
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
        public static ControlLabelScope ControlLabel(string label) => new ControlLabelScope(label);
        /// <summary>
        /// 在Using语句中使用的Scope，用于标记ControlLabel的属性
        /// </summary>
        public static ControlLabelScope ControlLabel(string label, string labelOverride) => new ControlLabelScope(string.IsNullOrEmpty(labelOverride) ? label : labelOverride);
        /// <summary>
        /// 用于将一个区域拆分为可调整大小的两个区域
        /// </summary>
        public static SubAreaScope SubArea(Rect rect, out Rect mainRect, out Rect subRect, float pos, Action<float> onPosChange, IceGUIDirection direction = IceGUIDirection.Right, GUIStyle separatorStyleOverride = null, float width = 4, float border = 2)
        {
            var scope = new SubAreaScope(rect, pos, onPosChange, direction, separatorStyleOverride ?? StlSubAreaSeparator, width, border);
            mainRect = scope.mainRect;
            subRect = scope.subRect;
            return scope;
        }
        /// <summary>
        /// 一个可缩放可移动的工作视图
        /// </summary>
        /// <param name="workspace">工作区Rect</param>
        /// <param name="canvasSize">画布的尺寸</param>
        /// <param name="viewScale">画布缩放值</param>
        /// <param name="viewOffset">画布偏移</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="useWidthOrHeightOfWorkspaceAsSize">为true时，取工作区的宽作为尺寸<br/>为false时，取工作的高作为尺寸<br/>为null时，取二者中较小值作为尺寸</param>
        /// <param name="useAbsoluteScale">为true时，缩放比例应用于像素<br/>为false时，缩放比例应用于画布</param>
        /// <param name="useLimitedOffset">是否限制视图偏移范围，使画布始终可见</param>
        /// <param name="gridColor">指定一个颜色，沿画布边缘对齐绘制一个网格</param>
        /// <param name="styleBackground">可为工作区设定一个背景样式</param>
        /// <param name="styleCanvas">可为画布设定一个背景样式</param>
        /// <returns></returns>
        public static ViewportScope Viewport(Rect workspace, float canvasSize, ref float viewScale, ref Vector2 viewOffset, float minScale = 0.5f, float maxScale = 2.0f, bool? useWidthOrHeightOfWorkspaceAsSize = null, bool useAbsoluteScale = false, bool useLimitedOffset = true, Color? gridColor = null, GUIStyle styleBackground = null, GUIStyle styleCanvas = null, bool inUtilityWindow = false)
        {
            if (workspace == Rect.zero) return null;

            // 数学运算
            float workspaceSize = useWidthOrHeightOfWorkspaceAsSize == null ? Mathf.Min(workspace.height, workspace.width) : useWidthOrHeightOfWorkspaceAsSize.Value ? workspace.width : workspace.height;
            var scale = Mathf.Clamp(viewScale, minScale, maxScale);
            if (useAbsoluteScale)
            {
                scale *= canvasSize / workspaceSize;
            }
            var offset = viewOffset;
            if (useLimitedOffset)
            {
                var t = canvasSize / workspaceSize;
                float borderX = Mathf.Max(workspace.width * t / scale * 0.5f, canvasSize * 0.5f);
                offset.x = Mathf.Clamp(offset.x, -borderX, borderX);
                float borderY = Mathf.Max(workspace.height * t / scale * 0.5f, canvasSize * 0.5f);
                offset.y = Mathf.Clamp(offset.y, -borderY, borderY);
            }

            // 背景
            if (styleBackground != null) StyleBox(workspace, styleBackground);

            // Control
            int preMoveViewControl = GetControlID();
            int moveViewControl = GetControlID();

            // CursorRect
            if (E.type == EventType.Repaint && (GUIHotControl == moveViewControl || GUIHotControl == preMoveViewControl)) EditorGUIUtility.AddCursorRect(workspace, MouseCursor.Pan);

            // 生成Scope对象
            var viewport = new ViewportScope(workspace, workspaceSize, canvasSize, scale, offset, inUtilityWindow);

            Rect clipRect = viewport.ClipRect;

            // 绘制前景和Grid
            if (E.type == EventType.Repaint)
            {
                // 前景
                if (styleCanvas != null) StyleBox(viewport.CanvasRect, styleCanvas);

                // Grid
                if (gridColor != null)
                {
                    Handles.color = gridColor.Value;

                    float xMin = clipRect.xMin;
                    float xMax = clipRect.xMax;
                    float yMin = clipRect.yMin;
                    float yMax = clipRect.yMax;
                    float wGrid = canvasSize;
                    for (float x = wGrid; x < xMax; x += wGrid) Handles.DrawLine(new Vector3(x, yMin), new Vector3(x, yMax));
                    for (float x = 0; x > xMin; x -= wGrid) Handles.DrawLine(new Vector3(x, yMin), new Vector3(x, yMax));
                    for (float y = wGrid; y < yMax; y += wGrid) Handles.DrawLine(new Vector3(xMin, y), new Vector3(xMax, y));
                    for (float y = 0; y > yMin; y -= wGrid) Handles.DrawLine(new Vector3(xMin, y), new Vector3(xMax, y));

                    Handles.color = Color.white;
                }
            }

            // 移动和缩放
            switch (E.type)
            {
                case EventType.ScrollWheel:
                    if (viewport.ClipRect.Contains(E.mousePosition))
                    {
                        if (useAbsoluteScale)
                        {
                            scale *= workspaceSize / canvasSize;
                        }

                        var newScale = Mathf.Clamp(scale * (1 - E.delta.y * 0.05f), minScale, maxScale);

                        viewOffset = offset += (E.mousePosition - clipRect.center) * (scale / newScale - 1);
                        viewScale = newScale;
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == preMoveViewControl) GUIHotControl = moveViewControl;
                    if (GUIHotControl == moveViewControl)
                    {
                        viewOffset = offset += E.mousePosition - _cache_drag;
                        E.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIHotControl == moveViewControl && E.button != 0)
                    {
                        GUIHotControl = 0;
                        E.Use();
                    }
                    break;
                case EventType.KeyUp:
                    if (GUIHotControl == preMoveViewControl && E.keyCode == KeyCode.Space)
                    {
                        GUIHotControl = 0;
                        E.Use();
                    }
                    break;
            }

            viewport.onCloseScope += () =>
            {
                if (clipRect.Contains(E.mousePosition))
                {
                    switch (E.type)
                    {
                        case EventType.KeyDown:
                            if (GUIHotControl == 0 && E.keyCode == KeyCode.Space)
                            {
                                GUIHotControl = preMoveViewControl;
                                E.Use();
                            }
                            break;
                        case EventType.MouseDown:
                            if (GUIHotControl == preMoveViewControl || (GUIHotControl == 0 && E.button != 0))
                            {
                                _cache_drag = E.mousePosition;
                                GUIHotControl = preMoveViewControl;
                            }
                            break;
                    }
                }
            };

            return viewport;
        }
        /// <summary>
        /// 一个可缩放可移动的Canvas视图
        /// </summary>
        /// <param name="workspace">工作区Rect</param>
        /// <param name="canvasSize">画布的尺寸</param>
        /// <param name="viewScale">画布缩放值</param>
        /// <param name="viewOffset">画布偏移</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="useWidthOrHeightOfWorkspaceAsSize">为true时，取工作区的宽作为尺寸<br/>为false时，取工作的高作为尺寸<br/>为null时，取二者中较小值作为尺寸</param>
        /// <param name="useAbsoluteScale">为true时，缩放比例应用于像素<br/>为false时，缩放比例应用于画布</param>
        /// <param name="styleBackground">可为工作区设定一个背景样式</param>
        /// <param name="styleCanvas">可为画布设定一个背景样式</param>
        /// <returns></returns>
        public static ViewportScope ViewportCanvas(Rect workspace, float canvasSize, ref float viewScale, ref Vector2 viewOffset, float minScale = 0.5f, float maxScale = 2.0f, bool? useWidthOrHeightOfWorkspaceAsSize = null, bool useAbsoluteScale = false, GUIStyle styleBackground = null, GUIStyle styleCanvas = null, bool inUtilityWindow = false) => Viewport(workspace, canvasSize, ref viewScale, ref viewOffset, minScale, maxScale, useWidthOrHeightOfWorkspaceAsSize, useAbsoluteScale, true, null, styleBackground, styleCanvas, inUtilityWindow);
        /// <summary>
        /// 一个可缩放可移动的Grid视图
        /// </summary>
        /// <param name="workspace">工作区Rect</param>
        /// <param name="gridSize">grid块的尺寸</param>
        /// <param name="viewScale">画布缩放值</param>
        /// <param name="viewOffset">画布偏移</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="useWidthOrHeightOfWorkspaceAsSize">为true时，取工作区的宽作为尺寸<br/>为false时，取工作的高作为尺寸<br/>为null时，取二者中较小值作为尺寸</param>
        /// <param name="gridColor">指定一个颜色，沿画布边缘对齐绘制一个网格</param>
        /// <param name="styleBackground">可为工作区设定一个背景样式</param>
        /// <returns></returns>
        public static ViewportScope ViewportGrid(Rect workspace, float gridSize, ref float viewScale, ref Vector2 viewOffset, float minScale = 0.4f, float maxScale = 4.0f, bool? useWidthOrHeightOfWorkspaceAsSize = null, Color? gridColor = null, GUIStyle styleBackground = null, bool inUtilityWindow = true) => Viewport(workspace, gridSize, ref viewScale, ref viewOffset, minScale, maxScale, useWidthOrHeightOfWorkspaceAsSize, true, false, gridColor, styleBackground, null, inUtilityWindow);
        /// <summary>
        /// 暂时改变 GUI.Color
        /// </summary>
        public static GUIColorScope GUIColor(Color color) => new GUIColorScope(color);
        /// <summary>
        /// 暂时改变 Handles.Color
        /// </summary>
        public static HandlesColorScope HandlesColor(Color color) => new HandlesColorScope(color);
        /// <summary>
        /// 暂时改变 IceGUIUtility.themeColorOverride
        /// </summary>
        public static ThemeColorScope ThemeColor(Color color) => new ThemeColorScope(color);

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

        public static int GUIHotControl { get => GUIUtility.hotControl; set => GUIUtility.hotControl = value; }
        public static int GetControlID(FocusType focus = FocusType.Passive) => GUIUtility.GetControlID(focus);

        public static GUIStyle GetStyle(string key = null, Func<GUIStyle> itor = null) => IceGUIStyleBox.GetStyle(key, itor);

        public static Event E => Event.current;

        // Internal Caches
        internal static Vector2 _cache_click;
        internal static Vector2 _cache_drag;
        internal static Vector2 _cache_offset;
        internal static Vector2 _cache_pos;
        internal static double _cache_time;
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

        /// <summary>
        /// 绘制有颜色渐变效果的贝塞尔曲线
        /// </summary>
        /// <param name="position">起点</param>
        /// <param name="target">终点</param>
        /// <param name="tangentPoint">切点</param>
        /// <param name="startColor">开始颜色</param>
        /// <param name="endColor">结束颜色</param>
        /// <param name="width">宽度</param>
        /// <param name="edge">alpha过渡边缘宽度</param>
        public static void DrawBezierLine(Vector2 position, Vector2 target, Vector2 tangentPoint, Color startColor, Color endColor, float width = 1.5f, float edge = 1)
        {
            if (E.type != EventType.Repaint) return;

            Handles.DrawLine(Vector3.zero, Vector3.zero);

            Vector2 delta = target - position;
            int segment = Mathf.CeilToInt((Mathf.Abs(delta.x) + 2 * Mathf.Abs(delta.y)) * GUI.matrix[0] / 16) + 1;
            float unit = 1.0f / (segment - 1);


            float innerWidth = Mathf.Max(0, width - edge);
            if (innerWidth > 0) DrawLinePart(-innerWidth, 1, innerWidth, 1);
            if (edge > 0)
            {
                DrawLinePart(-innerWidth, 1, -width, 0);
                DrawLinePart(innerWidth, 1, width, 0);
            }

            void DrawLinePart(float offset1, float alpha1, float offset2, float alpha2)
            {
                GL.Begin(GL.TRIANGLE_STRIP);

                Vector2 lp = Vector2.zero;
                Vector2 n = (tangentPoint - position).normalized;
                n = new Vector2(n.y, -n.x);
                for (int i = 0; i < segment; ++i)
                {
                    float t = i * unit;
                    var p1 = Vector2.LerpUnclamped(position, tangentPoint, t);
                    var p2 = Vector2.LerpUnclamped(tangentPoint, target, t);
                    Color col = Color.LerpUnclamped(startColor, endColor, t);
                    Vector2 p = Vector2.LerpUnclamped(p1, p2, t);
                    if (i > 0)
                    {
                        n = (p - lp).normalized;
                        n = new Vector2(n.y, -n.x);
                    }
                    lp = p;
                    col.a = alpha1;
                    GL.Color(col);
                    GL.Vertex(p + n * offset1);
                    col.a = alpha2;
                    GL.Color(col);
                    GL.Vertex(p + n * offset2);
                }

                GL.End();
            }
        }

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

        public static string _TextField(string val, GUIStyle style, params GUILayoutOption[] options) { CheckOptions(ref options); return DelayedScope.inScope ? (ControlLabelScope.HasLabel ? EditorGUILayout.DelayedTextField(ControlLabelScope.Label, val, style ?? EditorStyles.textField, options) : EditorGUILayout.DelayedTextField(val, style ?? EditorStyles.textField, options)) : (ControlLabelScope.HasLabel ? EditorGUILayout.TextField(ControlLabelScope.Label, val, style ?? EditorStyles.textField, options) : EditorGUILayout.TextField(val, style ?? EditorStyles.textField, options)); }
        public static string _TextField(string val, params GUILayoutOption[] options) => _TextField(val, null, options);
        public static string _TextField(string label, string val, GUIStyle style, params GUILayoutOption[] options) { using (ControlLabel(label)) return _TextField(val, style, options); }
        public static string _TextField(string label, string val, params GUILayoutOption[] options) => _TextField(label, val, null, options);
        public static string TextField(ref string val, GUIStyle styleOverride = null, params GUILayoutOption[] options) => val = _TextField(val, styleOverride, options);
        public static string TextField(string label, ref string val, GUIStyle styleOverride = null, params GUILayoutOption[] options) => val = _TextField(label, val, styleOverride, options);

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

        /// <summary>
        /// 搜索框，筛选一个string集合
        /// </summary>
        /// <param name="origin">待筛选的string集合</param>
        /// <param name="result">筛选过的集合（高亮后的名字|原始值）</param>
        /// <param name="filter">关键字</param>
        /// <param name="useRegex">使用正则表达式</param>
        /// <param name="continuousMatching">连续匹配</param>
        /// <param name="caseSensitive">区分大小写</param>
        /// <param name="extraElementsAction">额外GUI元素</param>
        public static void SearchField(IEnumerable<string> origin, ref List<(string displayName, string value)> result, ref string filter, ref bool useRegex, ref bool continuousMatching, ref bool caseSensitive)
        {
            using (HORIZONTAL) using (GUICHECK)
            {
                TextField(ref filter, StlSearchTextField);
                if (!useRegex)
                {
                    IceToggle("连", ref continuousMatching, "连续匹配");
                }
                IceToggle("Aa", ref caseSensitive, "区分大小写");
                IceToggle(".*", ref useRegex, "使用正则表达式");

                if (GUIChanged || result == null) result = origin.Filter(filter, useRegex, continuousMatching, caseSensitive);
            }
        }
        #endregion
    }
}