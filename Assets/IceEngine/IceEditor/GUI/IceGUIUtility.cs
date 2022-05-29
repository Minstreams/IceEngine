using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using IceEngine;
using IceEngine.Internal;
using IceEditor.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System.Reflection;

namespace IceEditor
{
    /// <summary>
    /// attributes信息
    /// </summary>
    public class IceAttributesInfo
    {
        public Dictionary<string, IceAttributesInfo> childrenMap = new();

        public Dictionary<string, string> labelMap = new();
        public HashSet<string> runtimeConstSet = new();

        public Dictionary<string, string> extraInfo = new();
        public Dictionary<string, string> errorInfo = new();
        public static IceAttributesInfo GetInfo(SerializedObject so) => new(so.targetObject.GetType());
        public IceAttributesInfo(Type type, string root = "")
        {
            var fields = type.GetFields();
            foreach (var f in fields)
            {
                var path = root + f.Name;

                // 处理 Label
                {
                    if (f.GetCustomAttribute<LabelAttribute>() is not null and var a) labelMap.Add(path, a.label);
                }

                // 处理 RuntimeConst
                {
                    if (f.GetCustomAttribute<RuntimeConst>() is not null) runtimeConstSet.Add(path);
                }

                {
                    if (f.GetCustomAttribute<MinAttribute>() is not null and var a) extraInfo.Add(path, a.min.ToString());
                }

                // 生成子结构
                static bool IsSystemType(Type t)
                {
                    var end = typeof(object);
                    while (t != end)
                    {
                        var b = t.BaseType;
                        if (b == end)
                        {
                            var ns = t.Namespace;
                            return ns != null && (ns.StartsWith("System") || ns.StartsWith("Unity"));
                        }
                        t = b;
                    }
                    return true;
                }
                var tt = f.FieldType;
                if (!IsSystemType(tt) && tt.GetCustomAttribute<SerializableAttribute>() is not null)
                {
                    childrenMap.Add(path, new IceAttributesInfo(tt, path + "."));
                }
            }
        }
    }

    public static class IceGUIUtility
    {
        #region General Implementation
        public static void DrawSerializedObject(SerializedObject so, IceAttributesInfo info = null)
        {
            so.UpdateIfRequiredOrScript();
            SerializedProperty itr = so.GetIterator();
            if (!itr.NextVisible(true)) return;
            // m_Script 是 Monobehavior 隐藏字段，没必要显示在面板上
            if (itr.propertyPath == "m_Script" && !itr.NextVisible(false)) return;

            // 正式绘制
            DrawSerializedProperty(itr, info);
            so.ApplyModifiedProperties();

            //if (Button("Test"))
            //{
            //    string log = "";
            //    foreach (Type item in TypeCache.GetTypesDerivedFrom<GUIDrawer>())
            //    {
            //        var attr = item.GetCustomAttribute<CustomPropertyDrawer>(true);
            //        if (attr != null)
            //        {
            //            log += item.FullName + "\n";
            //        }
            //    }
            //    Debug.Log(log);
            //}
        }
        static void DrawSerializedProperty(SerializedProperty itr, IceAttributesInfo info = null, SerializedProperty end = null)
        {
            do
            {
                var path = itr.propertyPath;

                // 处理 Label
                if (info == null || !info.labelMap.TryGetValue(path, out string label)) label = itr.displayName;

                // 处理 RuntimeConst
                bool disabled = info != null && info.runtimeConstSet.Contains(path) && EditorApplication.isPlayingOrWillChangePlaymode;

                // 处理子结构
                if (itr.propertyType == SerializedPropertyType.Generic && itr.hasChildren && info != null && info.childrenMap.TryGetValue(path, out var childInfo))
                {
                    var iBegin = itr.Copy();
                    var iEnd = itr.Copy();
                    iBegin.Next(true);
                    iEnd.Next(false);
                    using (BOX) using (SectionFolder(path, true, label)) using (Disable(disabled))
                    {
                        DrawSerializedProperty(iBegin, childInfo, iEnd);
                    }
                }
                else using (Disable(disabled))
                    {
                        switch (itr.propertyType)
                        {
                            case SerializedPropertyType.Boolean: itr.boolValue = _Toggle(label, itr.boolValue); break;
                            case SerializedPropertyType.Integer: itr.intValue = _IntField(label, itr.intValue); break;
                            case SerializedPropertyType.Float: itr.floatValue = _FloatField(label, itr.floatValue); break;
                            case SerializedPropertyType.String: itr.stringValue = _TextField(label, itr.stringValue); break;
                            case SerializedPropertyType.Color: itr.colorValue = _ColorField(label, itr.colorValue); break;
                            case SerializedPropertyType.Vector2: itr.vector2Value = _Vector2Field(label, itr.vector2Value); break;
                            case SerializedPropertyType.Vector3: itr.vector3Value = _Vector3Field(label, itr.vector3Value); break;
                            case SerializedPropertyType.Vector4: itr.vector4Value = _Vector4Field(label, itr.vector4Value); break;
                            case SerializedPropertyType.Vector2Int: itr.vector2IntValue = _Vector2IntField(label, itr.vector2IntValue); break;
                            case SerializedPropertyType.Vector3Int: itr.vector3IntValue = _Vector3IntField(label, itr.vector3IntValue); break;
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
                            default: EditorGUILayout.PropertyField(itr, TempContent(label), true); break;
                        }
                    }

                if (info != null)
                {
                    if (info.extraInfo.TryGetValue(path, out var extraInfo))
                    {
                        using (DOCK) Label(extraInfo);
                    }
                    if (info.errorInfo.TryGetValue(path, out var error))
                    {
                        using (GROUP) LabelError(error);
                    }
                }
            } while (itr.NextVisible(false) && itr != end);
        }
        public static Color DefaultThemeColor => IceIsland.Setting.themeColor;
        public static Color CurrentThemeColor => HasPack ? CurrentPack.ThemeColor : DefaultThemeColor;
        #endregion

        #region GUIAutoPack
        public static bool HasPack => _currentPack != null;
        public static IceGUIAutoPack CurrentPack => _currentPack ?? throw new IceGUIException("IceGUIAuto functions must be called inside a GUIPackScope!");
        static IceGUIAutoPack _currentPack;

        public class GUIPackScope : IDisposable
        {
            IceGUIAutoPack originPack = null;
            public GUIPackScope(IceGUIAutoPack pack)
            {
                originPack = _currentPack;
                _currentPack = pack;
            }
            void IDisposable.Dispose()
            {
                _currentPack = originPack;
            }
        }
        #endregion

        #region GUIStyle
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
        #endregion

        #region Preference Setting
        public static SettingsProvider GetSettingProvider<ConfigType>(string path, string label, ConfigType config, Func<ConfigType> createConfigAction, SettingsScope scope = SettingsScope.User) where ConfigType : ScriptableObject
        {
            SerializedObject so = null;
            return new SettingsProvider(path, scope)
            {
                activateHandler = (filter, rootElement) =>
                {
                    if (config == null) config = createConfigAction?.Invoke();
                    if (config != null) so = new SerializedObject(config);
                },
                guiHandler = (filter) =>
                {
                    if (so == null)
                    {
                        LabelError("Serialized Object is not generated!");
                        return;
                    }
                    DrawSerializedObject(so);
                },
                label = label,
            };
        }

        /// <summary>
        /// 在这里渲染所有的运行时系统配置
        /// </summary>
        [SettingsProvider]
        static SettingsProvider GetIceConfigSettingProvider()
        {
            Dictionary<string, SerializedObject> iceConfigSOMap = new Dictionary<string, SerializedObject>();

            // 获取所有需要的配置对象
            void CollectSubSystemFromAssembly(Assembly a)
            {
                var cs = a.GetTypes().Where(t => !t.IsGenericType && t.IsSubclassOf(typeof(IceSetting)));
                foreach (var c in cs)
                {
                    // TODO: Name获取方式改为SubString(7)
                    iceConfigSOMap.Add(c.Name.Replace("Setting", ""), new SerializedObject((UnityEngine.Object)c.BaseType.GetProperty("Setting", c).GetValue(null)));
                }
            }

            var iceAssembly = typeof(IceSetting).Assembly;
            CollectSubSystemFromAssembly(iceAssembly);

            var iceName = iceAssembly.GetName().Name;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetReferencedAssemblies().Select(a => a.Name).Contains(iceName))) CollectSubSystemFromAssembly(a);

            string selectedSetting = iceConfigSOMap.First().Key;

            return new SettingsProvider("Project/IceEngine", SettingsScope.Project)
            {
                guiHandler = filter =>
                {
                    var so = iceConfigSOMap[selectedSetting];
                    using (GROUP)
                    {
                        Header(selectedSetting);
                        DrawSerializedObject(so);
                    }
                },
                titleBarGuiHandler = () =>
                {
                    foreach (var so in iceConfigSOMap)
                    {
                        if (IceButton(so.Key, so.Key == selectedSetting)) selectedSetting = so.Key;
                    }
                },
            };
        }
        #endregion
    }
}
