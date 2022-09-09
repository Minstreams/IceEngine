using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Framework;
using IceEngine.Framework.Internal;
using IceEngine.Internal;
using IceEditor.Framework.Internal;
using IceEditor.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using UnityEngine.UIElements;
using System.Linq.Expressions;

namespace IceEditor
{
    public static class IceGUIUtility
    {
        #region General
        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            OnLoad_Toolbar();
            OnLoad_AppStatusBar();
            OnLoad_HierarchyItem();
        }
        #endregion

        #region Theme Color
        public static Color DefaultThemeColor => Ice.Island.Setting.themeColor;
        public static Color CurrentThemeColor => themeColorOverride ?? (HasPack ? CurrentPack.ThemeColor : DefaultThemeColor);
        internal static Color? themeColorOverride = null;
        #endregion

        #region Custom Drawer
        // 统计所有已有CustomDrawer的类
        static HashSet<Type> _typesWithCustomDrawer;
        static HashSet<Type> TypesWithCustomDrawer
        {
            get
            {
                if (_typesWithCustomDrawer == null)
                {
                    _typesWithCustomDrawer = new HashSet<Type>();
                    var candidates = TypeCache.GetTypesWithAttribute<HasPropertyDrawerAttribute>();
                    foreach (var c in candidates) _typesWithCustomDrawer.Add(c);
                }
                return _typesWithCustomDrawer;
            }
        }
        static bool HasCustomDrawer(this Type self)
        {
            foreach (var t in TypesWithCustomDrawer) if (self.IsSubclassOf(t)) return true;
            return false;
        }
        #endregion

        #region SerializedObject
        public static void DrawSerializedObject(SerializedObject so)
        {
            if (so.targetObject == null)
            {
                using (GROUP)
                {
                    LabelError("脚本已丢失");
                }
                return;
            }

            var info = IceAttributesInfo.GetInfo(so.targetObject.GetType());

            so.UpdateIfRequiredOrScript();

            SerializedProperty itr = so.GetIterator();
            if (!itr.NextVisible(true)) return;

            // 处理全局 Scope
            using var lScope = info.labelWidth == null ? null : LabelWidth(info.labelWidth.Value);
            using var cScope = info.themeColor == null ? null : ThemeColor(info.themeColor.Value);

            // m_Script 是 Monobehavior 隐藏字段，没必要显示在面板上
            if (itr.propertyPath != "m_Script" || itr.NextVisible(false))
            {
                // 正式绘制
                DrawSerializedProperty(itr, info);
                so.ApplyModifiedProperties();
            }

            // 处理 Button
            foreach (var (text, action) in info.buttonList)
            {
                if (Button(text)) action.Invoke(so.targetObject);
            }
        }

        /// <summary>
        /// Attributes信息，用于绘制SerializedObject或者SerializedProperty
        /// </summary>
        internal class IceAttributesInfo
        {
            // 子结构
            public Dictionary<string, IceAttributesInfo> childrenMap = new();

            // Custom Attributes
            public float? labelWidth = null;
            public Color? themeColor = null;
            public Dictionary<string, string> labelMap = new();
            public HashSet<string> runtimeConstSet = new();
            public List<(string text, Action<object> action)> buttonList = new();

            // Debug Info
            public Dictionary<string, string> extraInfo = new();
            public Dictionary<string, string> errorInfo = new();

            // Cache
            static readonly Dictionary<Type, IceAttributesInfo> cacheMap = new Dictionary<Type, IceAttributesInfo>();

            public static IceAttributesInfo GetInfo(Type t) => cacheMap.TryGetValue(t, out var info) ? info : cacheMap[t] = new IceAttributesInfo(t);
            public static void ClearCache() => cacheMap.Clear();

            public IceAttributesInfo(Type type)
            {
                // 处理主type
                foreach (var a in type.GetCustomAttributes(true))
                {
                    if (a is ThemeColorAttribute tc)
                    {
                        themeColor = tc.Color;
                    }
                    else if (a is LabelWidthAttribute lw)
                    {
                        labelWidth = lw.Width;
                    }
                }

                // 处理Fields
                var fields = type.GetFields();
                foreach (var f in fields)
                {
                    var path = f.Name;

                    // 处理 Label
                    {
                        if (f.GetCustomAttribute<LabelAttribute>() is not null and var a) labelMap.Add(path, a.Label);
                    }

                    // 处理 RuntimeConst
                    {
                        if (f.GetCustomAttribute<RuntimeConstAttribute>() is not null) runtimeConstSet.Add(path);
                    }

                    {
                        if (f.GetCustomAttribute<MinAttribute>() is not null and var a) extraInfo.Add(path, a.min.ToString());
                    }

                    // 生成子结构
                    var tt = f.FieldType;
                    if (!IsSystemType(tt) && tt.GetCustomAttribute<SerializableAttribute>() is not null && !tt.HasCustomDrawer())
                    {
                        childrenMap.Add(path, GetInfo(tt));
                    }
                }

                static bool IsSystemType(Type t)
                {
                    var ns = t.GetRootType().Namespace;
                    return ns != null && (ns.StartsWith("System") || ns.StartsWith("Unity"));
                }

                // 只对根类型处理Methods
                if (typeof(MonoBehaviour).IsAssignableFrom(type) || typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    var methods = type.GetMethods();
                    foreach (var m in methods)
                    {
                        // 处理 Button
                        {
                            if (m.GetCustomAttribute<ButtonAttribute>() is not null and var a) buttonList.Add((string.IsNullOrEmpty(a.Label) ? m.Name : a.Label, target => m.Invoke(target, null)));
                        }
                    }
                }
            }
        }
        static void DrawSerializedProperty(SerializedProperty itr, IceAttributesInfo info, SerializedProperty end = null, int rootPathLength = 0)
        {
            do
            {
                var path = itr.propertyPath.Substring(rootPathLength);

                // 处理 Label
                if (!info.labelMap.TryGetValue(path, out string label)) label = itr.displayName;

                // 处理 RuntimeConst
                bool disabled = info.runtimeConstSet.Contains(path) && EditorApplication.isPlayingOrWillChangePlaymode;

                if (itr.propertyType == SerializedPropertyType.Generic && itr.hasChildren && info.childrenMap.TryGetValue(path, out var childInfo))
                {
                    // 处理子结构
                    var iBegin = itr.Copy(); iBegin.NextVisible(true);
                    var iEnd = itr.Copy(); iEnd.NextVisible(false);

                    using (NODE) using (SectionFolder(path, true, $"{label}|{path}")) using (Disable(disabled))
                    {
                        DrawSerializedProperty(iBegin, childInfo, iEnd, rootPathLength + path.Length + 1);
                    }
                }
                else
                {
                    // 绘制
                    using (Disable(disabled)) PropertyField(itr, label);
                }

                if (info.extraInfo.TryGetValue(path, out var extraInfo))
                {
                    using (DOCK) Label(extraInfo);
                }
                if (info.errorInfo.TryGetValue(path, out var error))
                {
                    using (GROUP) LabelError(error);
                }
            } while (itr.NextVisible(false) && itr.propertyPath != end?.propertyPath);
        }
        static void PropertyField(SerializedProperty p, string label)
        {
            // TODO:全面支持多目标编辑
            var bMulti = p.hasMultipleDifferentValues;
            if (bMulti) GUI.color = CurrentThemeColor * 0.6f;
            using (GUICHECK)
            {
                switch (p.propertyType)
                {
                    //case SerializedPropertyType.Boolean: var boolVal = _Toggle(label, itr.boolValue); if (GUIChanged) itr.boolValue = boolVal; break;
                    //case SerializedPropertyType.Integer: var intVal = _IntField(label, itr.intValue); if (GUIChanged) itr.intValue = intVal; break;
                    //case SerializedPropertyType.Float: var floatValue = _FloatField(label, itr.floatValue); if (GUIChanged) itr.floatValue = floatValue; break;
                    //case SerializedPropertyType.String: var stringValue = _TextField(label, itr.stringValue); if (GUIChanged) itr.stringValue = stringValue; break;
                    //case SerializedPropertyType.Color: var colorValue = _ColorField(label, itr.colorValue); if (GUIChanged) itr.colorValue = colorValue; break;
                    case SerializedPropertyType.Vector2: var vector2Value = _Vector2Field(label, p.vector2Value); if (GUIChanged) p.vector2Value = vector2Value; break;
                    case SerializedPropertyType.Vector3: var vector3Value = _Vector3Field(label, p.vector3Value); if (GUIChanged) p.vector3Value = vector3Value; break;
                    case SerializedPropertyType.Vector4: var vector4Value = _Vector4Field(label, p.vector4Value); if (GUIChanged) p.vector4Value = vector4Value; break;
                    case SerializedPropertyType.Vector2Int: var vector2IntValue = _Vector2IntField(label, p.vector2IntValue); if (GUIChanged) p.vector2IntValue = vector2IntValue; break;
                    case SerializedPropertyType.Vector3Int: var vector3IntValue = _Vector3IntField(label, p.vector3IntValue); if (GUIChanged) p.vector3IntValue = vector3IntValue; break;
                    case SerializedPropertyType.Generic:
                    case SerializedPropertyType.ObjectReference:
                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.Enum:
                    case SerializedPropertyType.Rect:
                    case SerializedPropertyType.ArraySize:
                    case SerializedPropertyType.Character:
                    case SerializedPropertyType.AnimationCurve:
                    case SerializedPropertyType.Bounds:
                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Quaternion:
                    case SerializedPropertyType.ExposedReference:
                    case SerializedPropertyType.FixedBufferSize:
                    case SerializedPropertyType.RectInt:
                    case SerializedPropertyType.BoundsInt:
                    case SerializedPropertyType.ManagedReference:
                    case SerializedPropertyType.Hash128:
                    default: EditorGUILayout.PropertyField(p, TempContent(label), true); break;
                }
            }
            if (bMulti) GUI.color = Color.white;
            //if (E.type == EventType.Repaint && itr.hasMultipleDifferentValues)
            //{
            //    var rMul = GetLastRect().MoveEdge(EditorGUIUtility.labelWidth + 2).ApplyBorder(0);
            //    StyleBox(rMul, GetStyle(), "Multiple Values");
            //}
        }
        #endregion

        #region GUIAutoPack
        public static bool HasPack => _currentPack != null;
        public static IceGUIAutoPack CurrentPack => _currentPack ?? throw new IceGUIException("IceGUIAuto functions must be called inside a GUIPackScope!");
        static IceGUIAutoPack _currentPack;
        internal static GUIStyle LabelStyle => _labelStyle != null ? _labelStyle : (_labelStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).FindStyle("ControlLabel")); static GUIStyle _labelStyle;

        public class GUIPackScope : IDisposable
        {
            readonly IceGUIAutoPack originPack = null;
            Color originFocusColor;

            public GUIPackScope(IceGUIAutoPack pack)
            {
                originPack = _currentPack;
                _currentPack = pack;

                originFocusColor = LabelStyle.focused.textColor;
                LabelStyle.focused.textColor = CurrentThemeColor;
            }
            void IDisposable.Dispose()
            {
                _currentPack = originPack;
                LabelStyle.focused.textColor = originFocusColor;
            }
        }
        #endregion

        #region GUIStyle
        public static int GetThemeColorHueIndex(Color themeColor)
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
        public static GUIStyle GetStyle(string key = null, Func<GUIStyle> itor = null) => IceGUIStyleBox.GetStyle(key, itor);
        internal static GUIStyle GetStlSectionHeader(Color themeColor) => new GUIStyle("AnimationEventTooltip")
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
        internal static GUIStyle GetStlPrefix(Color themeColor) => new GUIStyle("PrefixLabel") { margin = new RectOffset(3, 3, 2, 2), padding = new RectOffset(1, 1, 0, 0), alignment = TextAnchor.MiddleLeft, richText = true, }.Initialize(stl => { stl.focused.textColor = stl.active.textColor = stl.onNormal.textColor = stl.onActive.textColor = themeColor; stl.onNormal.background = stl.active.background; });
        internal static GUIStyle GetStlSubAreaSeparator(Color themeColor)
        {
            GUIStyle on = $"flow node {GetThemeColorHueIndex(themeColor)}";
            var stl = new GUIStyle("flow node 0") { padding = new RectOffset(0, 0, 0, 0), contentOffset = new Vector2(0f, 0f), };
            stl.onNormal.background = on.normal.background;
            stl.onNormal.scaledBackgrounds = on.normal.scaledBackgrounds.ToArray();
            return stl;
        }
        #endregion

        #region SettingProvider
        /// <summary>
        /// 所有的运行时系统配置
        /// </summary>
        [SettingsProvider]
        static SettingsProvider GetIceSettingProvider() => GetIceSettingProvider<IceSetting>("IceEngine", "Setting");
        /// <summary>
        /// 所有的编辑时系统配置
        /// </summary>
        [SettingsProvider]
        static SettingsProvider GetIceEditorSettingProvider() => GetIceSettingProvider<IceEditorSetting>("IceEditor", "EditorSetting");
        /// <summary>
        /// 在Project窗口部署某一配置类所有的子类实例
        /// </summary>
        /// <param name="path">配置路径</param>
        /// <param name="baseSettingType">配置类</param>
        /// <param name="prefix">子类名称统一前缀</param>
        internal static SettingsProvider GetIceSettingProvider<TSetting>(string path, string prefix) where TSetting : ScriptableObject
        {
            Type baseSettingType = typeof(TSetting);

            // soMap
            Dictionary<string, (SerializedObject, Color?)> iceSettingSOMap = new();

            // 从所有相关的Assembly中收集数据
            var settingCollection = TypeCache.GetTypesDerivedFrom<TSetting>();
            foreach (var settingType in settingCollection)
            {
                if (settingType.IsGenericType) continue;

                // 显示Title删掉开头的Setting
                var title = settingType.Name.StartsWith(prefix) ? settingType.Name[prefix.Length..] : settingType.Name;
                var setting = settingType.BaseType.GetProperty("Setting", settingType).GetValue(null) as TSetting;
                var so = new SerializedObject(setting);
                var color = so.FindProperty("themeColor")?.colorValue;
                iceSettingSOMap.Add(title, (so, color));
            }

            // 选择的项目字段
            string selectedSetting = null;


            // GUI
            return new SettingsProvider("Project/" + path, SettingsScope.Project)
            {
                guiHandler = filter =>
                {
                    if (selectedSetting == null)
                    {
                        foreach ((string title, (SerializedObject so, Color? color)) in iceSettingSOMap)
                        {
                            using (GROUP) using (color == null ? null : ThemeColor(color.Value))
                            {
                                Header(title);
                                DrawSerializedObject(so);
                            }
                        }
                        return;
                    }
                    else
                    {
                        (SerializedObject so, Color? color) = iceSettingSOMap[selectedSetting];
                        using (GROUP) using (color == null ? null : ThemeColor(color.Value))
                        {
                            Header(selectedSetting);
                            DrawSerializedObject(so);
                        }
                    }
                },
                titleBarGuiHandler = () =>
                {
                    foreach ((string title, (_, Color? color)) in iceSettingSOMap)
                    {
                        string t = title;
                        Color c = color ?? CurrentThemeColor;
                        //using (color == null ? null : ThemeColor(color.Value))
                        if (selectedSetting == title || selectedSetting == null) t = t.Color(c);
                        if (IceButton(t)) selectedSetting = selectedSetting == title ? null : title;
                    }
                },
            };
        }
        #endregion

        #region IceGraphPort
        public const float PORT_SIZE = 16;
        public const float PORT_RADIUS = PORT_SIZE * 0.5f;

        public static Vector2 GetPos(this IceprintPort port)
        {
            var node = port.node;
            Vector2 res = node.position;
            if (node.folded)
            {
                if (port.IsOutport)
                {
                    res.x += node.GetSizeTitle().x;
                    res.y += (node.GetSizeTitle().y * (port.id + 1)) / (node.outports.Count + 1);
                }
                else
                {
                    res.y += (node.GetSizeTitle().y * (port.id + 1)) / (node.inports.Count + 1);
                }
            }
            else
            {
                res.y += port.id * PORT_SIZE + PORT_RADIUS;
                if (port.IsOutport) res.x += node.GetSizeUnfolded().x + PORT_RADIUS;
                else res.x -= PORT_RADIUS;
            }
            return res;
        }
        public static Vector2 GetTangent(this IceprintPort self) => self.IsOutport ? Vector2.right : Vector2.left;
        public static void DrawPortLine(Vector2 position, Vector2 target, Vector2 tangent, Color startColor, Color endColor, float width = 1.5f, float edge = 1)
        {
            if (E.type != EventType.Repaint) return;

            Vector2 center = 0.5f * (position + target);
            Color centerColor = 0.5f * (startColor + endColor);

            float tangentLength = Mathf.Clamp(Vector2.Dot(tangent, center - position) * 0.6f, 8, 32);
            Vector2 tangentPoint = position + tangent * tangentLength;

            DrawBezierLine(position, center, tangentPoint, startColor, centerColor, width, edge);
        }
        #endregion

        #region IceGraphNode

        #region Drawer
        static Dictionary<Type, IceprintNodeDrawer> NodeDrawerMap
        {
            get
            {
                if (_nodeDrawerMap == null)
                {
                    _nodeDrawerMap = new();
                    var drawers = TypeCache.GetTypesDerivedFrom<IceprintNodeDrawer>();
                    foreach (var dt in drawers)
                    {
                        if (dt.IsAbstract) continue;
                        var drawer = (IceprintNodeDrawer)Activator.CreateInstance(dt);
                        if (!_nodeDrawerMap.TryAdd(drawer.NodeType, drawer)) throw new Exception($"Collecting drawer [{dt.FullName}] failed! [{drawer.NodeType}] already has a drawer [{_nodeDrawerMap[drawer.NodeType]}]");
                    }
                }
                return _nodeDrawerMap;
            }
        }
        static Dictionary<Type, IceprintNodeDrawer> _nodeDrawerMap = null;
        public static IceprintNodeDrawer GetDrawer(this IceprintNode node) => NodeDrawerMap.TryGetValue(node.GetType(), out var drawer) ? drawer : _defaultDrawer;
        static readonly IceprintNodeDrawer _defaultDrawer = new();

        static Dictionary<Type, IceprintNodeComponentDrawer> NodeComponentDrawerMap
        {
            get
            {
                if (_nodeComponentDrawerMap == null)
                {
                    _nodeComponentDrawerMap = new();
                    var drawers = TypeCache.GetTypesDerivedFrom<IceprintNodeComponentDrawer>();
                    foreach (var dt in drawers)
                    {
                        if (dt.IsAbstract) continue;
                        var drawer = (IceprintNodeComponentDrawer)Activator.CreateInstance(dt);
                        if (!_nodeComponentDrawerMap.TryAdd(drawer.NodeType, drawer)) throw new Exception($"Collecting drawer [{dt.FullName}] failed! [{drawer.NodeType}] already has a drawer [{_nodeComponentDrawerMap[drawer.NodeType]}]");
                    }
                }
                return _nodeComponentDrawerMap;
            }
        }
        static Dictionary<Type, IceprintNodeComponentDrawer> _nodeComponentDrawerMap = null;
        public static IceprintNodeComponentDrawer GetDrawer(this IceprintNodeComponent node) => NodeComponentDrawerMap.TryGetValue(node.GetType(), out var drawer) ? drawer : _defaultComponentDrawer;
        static readonly IceprintNodeComponentDrawer _defaultComponentDrawer = new();
        #endregion

        public static Rect GetArea(this IceprintNode node) => new(node.position, node.GetSize());
        public static Vector2 GetSize(this IceprintNode node) => node.folded ? node.GetSizeTitle() : node.GetSizeUnfolded();
        public static Vector2 GetSizeUnfolded(this IceprintNode node) => new
        (
            Mathf.Max(node.GetSizeBody().x, node.GetSizeTitle().x),
            node.GetSizeBody().y + node.GetSizeTitle().y
        );
        public static Vector2 GetSizeTitle(this IceprintNode node) => node.GetDrawer().GetSizeTitle(node);
        public static Vector2 GetSizeTitle(this IceprintNodeComponent node) => node.GetDrawer().GetSizeTitle(node);
        public static Vector2 GetSizeBody(this IceprintNode node) => node.GetDrawer().GetSizeBody(node);
        public static Vector2 GetSizeBody(this IceprintNodeComponent node) => node.GetDrawer().GetSizeBody(node);
        public static string GetDisplayName(this IceprintNode node) => node.GetDrawer().GetDisplayName(node);
        public static string GetDisplayName(this IceprintNodeComponent node) => node.GetDrawer().GetDisplayName(node);
        #endregion

        #region Toolbar
        static Action onToolbarGUILeft;
        static Action onToolbarGUIMidLeft;
        static Action onToolbarGUIMidRight;
        static Action onToolbarGUIRight;

        static readonly Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static ScriptableObject m_currentToolbar;

        static void OnLoad_Toolbar()
        {
            var callbacks = TypeCache.GetMethodsWithAttribute<ToolbarGUICallbackAttribute>();
            foreach (var callback in callbacks)
            {
                if (!callback.IsStatic) throw new IceGUIException("Toolbar GUI handler must be static!");

                var handler = Expression.Lambda<Action>(Expression.Call(callback)).Compile();
                switch (callback.GetCustomAttribute<ToolbarGUICallbackAttribute>().Position)
                {
                    case ToolbarGUIPosition.Left:
                        onToolbarGUILeft += handler;
                        break;
                    case ToolbarGUIPosition.MidLeft:
                        onToolbarGUIMidLeft += handler;
                        break;
                    case ToolbarGUIPosition.MidRight:
                        onToolbarGUIMidRight += handler;
                        break;
                    case ToolbarGUIPosition.Right:
                        onToolbarGUIRight += handler;
                        break;
                }
            }

            EditorApplication.update -= TryRegisterToolbarGUI;
            EditorApplication.update += TryRegisterToolbarGUI;
        }
        internal static void TryRegisterToolbarGUI()
        {
            // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
            if (m_currentToolbar == null)
            {
                // Find toolbar
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (m_currentToolbar != null)
                {
                    var mRoot = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(m_currentToolbar) as VisualElement;

                    if (onToolbarGUILeft != null || onToolbarGUIMidLeft != null) mRoot.Q("ToolbarZoneLeftAlign").Add(new IMGUIContainer()
                    {
                        name = "IceToolbarLeft",
                        style = { flexGrow = 1, marginLeft = 4, marginRight = 4 },
                        onGUIHandler = OnToolbarGUILeft,
                    });

                    if (onToolbarGUIMidRight != null || onToolbarGUIRight != null) mRoot.Q("ToolbarZoneRightAlign").Add(new IMGUIContainer()
                    {
                        name = "IceToolbarRight",
                        style = { flexGrow = 1, marginLeft = 4, marginRight = 4 },
                        onGUIHandler = OnToolbarGUIRight,
                    });
                }
            }
        }
        static void OnToolbarGUILeft()
        {
            using (HORIZONTAL)
            {
                onToolbarGUILeft?.Invoke();
                Space();
                onToolbarGUIMidLeft?.Invoke();
            }
        }
        static void OnToolbarGUIRight()
        {
            using (HORIZONTAL)
            {
                onToolbarGUIMidRight?.Invoke();
                Space();
                onToolbarGUIRight?.Invoke();
            }
        }
        #endregion

        #region AppStatusBar
        static Action onAppStatusBarGUILeft;
        static Action onAppStatusBarGUIRight;

        static readonly Type m_appStatusBarType = typeof(Editor).Assembly.GetType("UnityEditor.AppStatusBar");
        static readonly Type m_guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
        static ScriptableObject m_currentAppStatusBar;

        static void OnLoad_AppStatusBar()
        {
            var callbacks = TypeCache.GetMethodsWithAttribute<AppStatusBarGUICallbackAttribute>();
            foreach (var callback in callbacks)
            {
                if (!callback.IsStatic) throw new IceGUIException("AppStatusBar GUI handler must be static!");

                var handler = Expression.Lambda<Action>(Expression.Call(callback)).Compile();
                if (callback.GetCustomAttribute<AppStatusBarGUICallbackAttribute>().IsRight)
                    onAppStatusBarGUIRight += handler;
                else onAppStatusBarGUILeft += handler;
            }

            EditorApplication.update -= TryRegisterAppStatusBarGUI;
            EditorApplication.update += TryRegisterAppStatusBarGUI;
        }
        internal static void TryRegisterAppStatusBarGUI()
        {
            // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
            if (m_currentAppStatusBar == null)
            {
                // Find status bar
                m_currentAppStatusBar = (ScriptableObject)m_appStatusBarType.GetField("s_AppStatusBar", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                if (m_currentAppStatusBar != null)
                {
                    var viewField = m_guiViewType.GetProperty("visualTree", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    var mRoot = viewField.GetValue(m_currentAppStatusBar) as VisualElement;

                    if (onAppStatusBarGUILeft != null || onAppStatusBarGUIRight != null)
                    {
                        mRoot.style.flexDirection = FlexDirection.Row;
                        mRoot.Q(className: "unity-imgui-container").style.position = Position.Relative;

                        if (onAppStatusBarGUILeft != null) mRoot.Insert(0, new IMGUIContainer()
                        {
                            name = "IceStatusBarLeft",
                            style = { flexShrink = 0, marginLeft = 4, marginRight = 4 },
                            onGUIHandler = OnAppStatusBarGUILeft,
                        });

                        if (onAppStatusBarGUIRight != null) mRoot.Add(new IMGUIContainer()
                        {
                            name = "IceStatusBarRight",
                            style = { flexShrink = 0, marginLeft = 4, marginRight = 4 },
                            onGUIHandler = OnAppStatusBarGUIRight,
                        });
                    }
                }
            }
        }
        static void OnAppStatusBarGUILeft()
        {
            using (HORIZONTAL) onAppStatusBarGUILeft?.Invoke();
        }
        static void OnAppStatusBarGUIRight()
        {
            using (HORIZONTAL) onAppStatusBarGUIRight?.Invoke();
        }
        #endregion

        #region HierarchyItem
        static Dictionary<Type, Action<Component, Rect>> hierarchyItemGUICallbackMap = new();
        static readonly Type componentType = typeof(Component);
        static readonly Type rectType = typeof(Rect);
        static void OnLoad_HierarchyItem()
        {
            var callbacks = TypeCache.GetMethodsWithAttribute<HierarchyItemGUICallbackAttribute>();
            foreach (var callback in callbacks)
            {
                if (!callback.IsStatic) throw new IceGUIException("Hierarchy Item GUI handler must be static!");
                var pt = CheckParams(callback);

                var p0 = Expression.Parameter(componentType);
                var p1 = Expression.Parameter(rectType);
                var handler = Expression.Lambda<Action<Component, Rect>>(Expression.Call(callback, Expression.Convert(p0, pt), p1), p0, p1).Compile();
                if (!hierarchyItemGUICallbackMap.TryGetValue(pt, out var listener)) hierarchyItemGUICallbackMap.Add(pt, null);
                hierarchyItemGUICallbackMap[pt] += handler;
            }

            Type CheckParams(MethodInfo m)
            {
                var ps = m.GetParameters();
                if (ps.Length != 2 || ps[1].ParameterType != rectType) throw new Exception($"Invalid Parameters of {m.DeclaringType.FullName}.{m.Name}");
                return ps[0].ParameterType;
            }

            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        static void OnHierarchyItemGUI(int instanceId, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj == null) return;
            if (obj is not GameObject go) throw new Exception($"Hierarchy Item must be a gameobject! {obj}");
            foreach ((Type t, var callback) in hierarchyItemGUICallbackMap)
            {
                var comp = go.GetComponent(t);
                if (comp == null) continue;
                callback?.Invoke(comp, selectionRect);
            }
        }
        #endregion
    }
}