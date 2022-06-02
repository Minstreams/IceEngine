using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Internal;
using IceEditor.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor
{


    public static class IceGUIUtility
    {
        #region General Implementation
        public static void DrawSerializedObject(SerializedObject so)
        {
            var info = IceAttributesInfo.GetInfo(so.targetObject.GetType());

            so.UpdateIfRequiredOrScript();

            SerializedProperty itr = so.GetIterator();
            if (!itr.NextVisible(true)) return;
            // m_Script 是 Monobehavior 隐藏字段，没必要显示在面板上
            if (itr.propertyPath == "m_Script" && !itr.NextVisible(false)) return;

            // 正式绘制
            DrawSerializedProperty(itr, info);

            so.ApplyModifiedProperties();

            // 处理 Button
            foreach (var b in info.buttonList)
            {
                if (Button(b.text)) b.action.Invoke(so.targetObject);
            }
        }


        /// <summary>
        /// Attributes信息，用于绘制 SerializedObject 或者 SerializedProperty
        /// </summary>
        internal class IceAttributesInfo
        {
            // 子结构
            public Dictionary<string, IceAttributesInfo> childrenMap = new();

            // Custom Attributes
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
                    if (!IsSystemType(tt) && tt.GetCustomAttribute<SerializableAttribute>() is not null)
                    {
                        childrenMap.Add(path, GetInfo(tt));
                    }
                }
                static bool IsSystemType(Type t)
                {
                    var ns = t.GetRoot().Namespace;
                    return ns != null && (ns.StartsWith("System") || ns.StartsWith("Unity"));
                }

                // 只对根类型处理Methods
                if (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject)))
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
                    using (Disable(disabled))
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
                            default: EditorGUILayout.PropertyField(itr, TempContent(label), true); break;
                        }
                    }
                }

                if (info.extraInfo.TryGetValue(path, out var extraInfo))
                {
                    using (DOCK) Label(extraInfo);
                }
                if (info.errorInfo.TryGetValue(path, out var error))
                {
                    using (GROUP) LabelError(error);
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
            Dictionary<string, SerializedObject> iceSettingSOMap = new Dictionary<string, SerializedObject>();

            // 从所有相关的Assembly中收集数据
            var settingCollection = TypeCache.GetTypesDerivedFrom<TSetting>();
            foreach (var settingType in settingCollection)
            {
                if (settingType.IsGenericType) continue;

                // 显示Title删掉开头的Setting
                var title = settingType.Name.StartsWith(prefix) ? settingType.Name[prefix.Length..] : settingType.Name;
                var so = new SerializedObject((UnityEngine.Object)settingType.BaseType.GetProperty("Setting", settingType).GetValue(null));
                iceSettingSOMap.Add(title, so);
            }

            // 选择的项目字段
            string selectedSetting = iceSettingSOMap.First().Key;


            // GUI
            return new SettingsProvider("Project/" + path, SettingsScope.Project)
            {
                guiHandler = filter =>
                {
                    var so = iceSettingSOMap[selectedSetting];
                    using (GROUP)
                    {
                        Header(selectedSetting);
                        DrawSerializedObject(so);
                    }
                },
                titleBarGuiHandler = () =>
                {
                    foreach (var so in iceSettingSOMap)
                    {
                        if (IceButton(so.Key, so.Key == selectedSetting)) selectedSetting = so.Key;
                    }
                },
            };
        }
        #endregion
    }
}
