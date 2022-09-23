using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    /// <summary>
    /// GUI Style Box
    /// </summary>
    internal sealed class IceGUIStyleBox : IceEditorWindow
    {
        #region 【常量】
        /// <summary>
        /// 默认Style表列
        /// </summary>
        readonly string[] dList =
        {
            "box",
            "button",
            "label",
            "scrollView",
            "textArea",
            "textField",
            "toggle",
            "window",
            "horizontalScrollbar",
            "horizontalScrollbarThumb",
            "horizontalScrollbarLeftButton",
            "horizontalScrollbarRightButton",
            "verticalScrollbar",
            "verticalScrollbarThumb",
            "verticalScrollbarUpButton",
            "verticalScrollbarDownButton",
            "horizontalSlider",
            "horizontalSliderThumb",
            "verticalSlider",
            "verticalSliderThumb"
        };
        #endregion

        #region 【接口】
        /// <summary>
        /// 单例
        /// </summary>
        public static IceGUIStyleBox Instance => _instance != null ? _instance : (_instance = OpenWindow()); static IceGUIStyleBox _instance;
        /// <summary>
        /// id = 0: 正在编辑的样式，id > 0: 样式列表中的样式
        /// </summary>
        public static GUIStyle GetStyle(string key = null, Func<GUIStyle> itor = null)
        {
            if (string.IsNullOrEmpty(key)) return Instance.stlInspecting;
            if (Instance.stlCacheMap.TryGetValue(key, out StyleCache val)) return val.stl;
            else
            {
                var stl = itor?.Invoke() ?? GUIStyle.none;
                Instance.stlCacheMap.Add(key, new StyleCache(stl, string.IsNullOrEmpty(stl.name) ? new GUIStyle() : new GUIStyle(stl.name)));
                return stl;
            }
        }

        /// <summary>
        /// 为true时显示运行时样式表，否则显示编辑时样式表
        /// </summary>
        public bool InGameSkin
        {
            get => _inGameSkin; set
            {
                if (_inGameSkin != value)
                {
                    _inGameSkin = value;
                    InitializeGUISkin();
                }
            }
        }
        /// <summary>
        /// 生成样式代码
        /// </summary>
        public string GetStyleCode(string styleName)
        {
            styleName = Regex.Replace(styleName, "^Stl", "");

            string itorCode = "";
            string initCode = "";

            var pOrigin = PropertyObj.FindProperty("stlInspectingOrigin");
            var pActive = PropertyObj.FindProperty("stlInspecting");

            pOrigin.NextVisible(true);
            pActive.NextVisible(true);

            do
            {
                if (!SerializedProperty.DataEquals(pActive, pOrigin))
                {
                    if (pActive.type == "GUIStyleState")
                    {
                        var subOrigin = pOrigin.Copy();
                        var subActive = pActive.Copy();
                        subOrigin.NextVisible(true);
                        subActive.NextVisible(true);

                        string stateName = Regex.Replace(pActive.name, "^m_(.)", m => m.Groups[1].Value.ToLower());

                        for (int i = 0; i < 3; ++i)
                        {
                            if (!SerializedProperty.DataEquals(subActive, subOrigin)) initCode += $"stl.{stateName}.{GetItorCode(subActive, Regex.Replace(subActive.name, "^m_(.)", m => m.Groups[1].Value.ToLower()))}; ";
                            subActive.NextVisible(false);
                            subOrigin.NextVisible(false);
                        }
                    }
                    else
                    {
                        itorCode += $"{GetItorCode(pActive, Regex.Replace(pActive.name, "^m_(?:Text)?(.)", m => m.Groups[1].Value.ToLower()))}, ";
                    }
                }
            } while (pActive.NextVisible(false) && pOrigin.NextVisible(false));

            var hasItor = !string.IsNullOrWhiteSpace(itorCode);
            var hasInit = !string.IsNullOrWhiteSpace(initCode);

            if (!hasItor && !hasInit && !InGameSkin) return $"GUIStyle Stl{styleName} => \"{stlInspectingOrigin.name}\";";
            else return $"GUIStyle Stl{styleName} => _stl{styleName}?.Check() ?? (_stl{styleName} = new GUIStyle({(string.IsNullOrEmpty(stlInspectingOrigin.name) ? "" : (InGameSkin ? $"EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game).FindStyle(\"{stlInspectingOrigin.name}\")" : $"\"{stlInspectingOrigin.name}\""))}){(hasItor ? $" {{ {itorCode}}}" : "")}{(hasInit ? $".Initialize(stl => {{ {initCode}}})" : "")}); GUIStyle _stl{styleName};";

        }
        #endregion

        #region 【字段】
        bool _inGameSkin = false;

        readonly Dictionary<string, GUIStyle> stlMap = new Dictionary<string, GUIStyle>();
        [NonSerialized] List<(string displayName, string name)> stlFiltered = null;

        [SerializeField] GUIStyle stlInspectingOrigin = new GUIStyle();
        [SerializeField] GUIStyle stlInspecting = new GUIStyle();

        string styleName = string.Empty;
        #endregion

        #region 【定制】

        [MenuItem("IceEngine/Style Box", false, 20)]
        public static IceGUIStyleBox OpenWindow() => GetWindow<IceGUIStyleBox>();
        protected override string Title => InGameSkin ? "GUIStyle 样例窗口 - InGameSkin" : "GUIStyle 样例窗口";
        protected override Color DefaultThemeColor => new Color(1, 0.7f, 0);
        protected override void OnThemeColorChange()
        {
            base.OnThemeColorChange();
            stlFiltered = null;
        }
        public override void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("InGameSkin"), InGameSkin, () => InGameSkin = !InGameSkin);
            menu.AddSeparator("");
            base.AddItemsToMenu(menu);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeGUISkin();
        }
        void InitializeGUISkin()
        {
            stlFiltered = null;

            var skin = EditorGUIUtility.GetBuiltinSkin(InGameSkin ? EditorSkin.Game : EditorSkin.Scene);
            var sList = skin.customStyles;

            stlMap.Clear();
            foreach (var d in dList) stlMap.Add(d, skin.GetStyle(d));
            foreach (var s in sList) stlMap.Add(s.name, s);


            RefreshTitleContent();
        }
        #endregion

        #region 【样式管理】
        #region 测试样式显示
        SerializedObject PropertyObj => _propertyObj ?? (_propertyObj = new SerializedObject(this)); SerializedObject _propertyObj;
        enum GUIType
        {
            GUILayout,
            GUI,
        }
        enum DisplayType
        {
            Label,
            Box,
            Button,
            TextField,
            Toggle,
            EnumPopup,
        }
        enum TestEnum
        {
            AA,
            BBB,
            CCCC,
            DDDDD,
        }

        string displayText = "Test测试";
        bool displayBool;
        TestEnum displayEnum;

        GUIType guiType;
        DisplayType displayType;

        readonly List<GUILayoutOption> displayOptions = new List<GUILayoutOption>();

        struct FloatOption
        {
            public string name;
            public float defaultValue;

            public FloatOption(string name, float defaultValue)
            {
                this.name = name;
                this.defaultValue = defaultValue;
            }
        }
        readonly FloatOption[] layoutFloatOptions =
        {
            new FloatOption ("Width", 128),
            new FloatOption ("MinWidth", 128),
            new FloatOption ("MaxWidth", 128),
            new FloatOption ("Height", 32),
            new FloatOption ("MinHeight", 32),
            new FloatOption ("MaxHeight", 32),
        };
        struct BoolOption
        {
            public string name;
            public bool defaultValue;

            public BoolOption(string name, bool defaultValue)
            {
                this.name = name;
                this.defaultValue = defaultValue;
            }
        }
        readonly BoolOption[] layoutBoolOptions =
        {
            new BoolOption ("ExpandWidth", true),
            new BoolOption ("ExpandHeight", true),
        };
        void ConstructLayoutOptions()
        {
            displayOptions.Clear();

            void RecordLayoutFloatOption(string name, float defaultValue)
            {
                if (GetBool($"Has Layout {name}")) displayOptions.Add((GUILayoutOption)typeof(GUILayout).InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { GetFloat($"Val Layout {name}", defaultValue) }));
            }
            foreach (var o in layoutFloatOptions) RecordLayoutFloatOption(o.name, o.defaultValue);

            void RecordLayoutBoolOption(string name, bool defaultValue)
            {
                if (GetBool($"Has Layout {name}")) displayOptions.Add((GUILayoutOption)typeof(GUILayout).InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { GetBool($"Val Layout {name}", defaultValue) }));
            }
            foreach (var o in layoutBoolOptions) RecordLayoutBoolOption(o.name, o.defaultValue);
        }
        void DisplayStyle(GUIStyle style, GUILayoutOption[] optionsOverride = null)
        {
            GUILayoutOption[] options = optionsOverride ?? displayOptions.ToArray();
            switch (displayType)
            {
                case DisplayType.Label:
                    GUILayout.Label(displayText, style, options);
                    break;
                case DisplayType.Box:
                    GUILayout.Box(displayText, style, options);
                    break;
                case DisplayType.Button:
                    GUILayout.Button(displayText, style, options);
                    break;
                case DisplayType.TextField:
                    displayText = GUILayout.TextField(displayText, style, options);
                    break;
                case DisplayType.Toggle:
                    displayBool = GUILayout.Toggle(displayBool, displayText, style, options);
                    break;
                case DisplayType.EnumPopup:
                    displayEnum = (TestEnum)EditorGUILayout.EnumPopup(displayEnum, style, options);
                    break;
            }
        }
        void DisplayStyle(Rect rect, GUIStyle style)
        {
            switch (displayType)
            {
                case DisplayType.Label:
                    GUI.Label(rect, displayText, style);
                    break;
                case DisplayType.Box:
                    GUI.Box(rect, displayText, style);
                    break;
                case DisplayType.Button:
                    GUI.Button(rect, displayText, style);
                    break;
                case DisplayType.TextField:
                    displayText = GUI.TextField(rect, displayText, style);
                    break;
                case DisplayType.Toggle:
                    displayBool = GUI.Toggle(rect, displayBool, displayText, style);
                    break;
                case DisplayType.EnumPopup:
                    displayEnum = (TestEnum)EditorGUI.EnumPopup(rect, displayEnum, style);
                    break;
            }
        }
        #endregion

        #region 样式表
        [Serializable]
        struct StyleCache
        {
            public GUIStyle stl;
            public GUIStyle stlOrigin;

            public StyleCache(GUIStyle stl, GUIStyle stlOrigin)
            {
                this.stl = stl;
                this.stlOrigin = stlOrigin;
            }
        }

        IceDictionary<string, StyleCache> stlCacheMap = new IceDictionary<string, StyleCache>();
        #endregion

        #endregion

        #region 【GUI】
        #region Style
        // Style
        GUIStyle StlLabelBtn => _stlLabelBtn?.Check() ?? (_stlLabelBtn = new GUIStyle("ToolbarTextField") { richText = true, stretchWidth = true, fixedHeight = 0, wordWrap = true }); GUIStyle _stlLabelBtn;
        GUIStyle StlCode => _stlCode?.Check() ?? (_stlCode = new GUIStyle("label") { margin = new RectOffset(1, 1, 0, 0), padding = new RectOffset(0, 0, 1, 1), fontSize = 14, fontStyle = FontStyle.Bold, richText = true }).Initialize(stl => stl.normal.textColor = Color.white); GUIStyle _stlCode;
        GUIStyle StlGUIBorder => _stlGUIBorder?.Check() ?? (_stlGUIBorder = new GUIStyle("grey_border") { margin = new RectOffset(0, 16, 0, 0), }); GUIStyle _stlGUIBorder;
        GUIStyle StlGUIBorderHighlight => _stlGUIBorderHighlight?.Check() ?? (_stlGUIBorderHighlight = new GUIStyle("LightmapEditorSelectedHighlight") { overflow = new RectOffset(7, 7, 7, 7), fontSize = 10, alignment = TextAnchor.LowerCenter, wordWrap = true, imagePosition = ImagePosition.TextOnly, contentOffset = new Vector2(0f, -70f), }.Initialize(stl => { stl.normal.textColor = new Color(1f, 0.9862055f, 0f); })); GUIStyle _stlGUIBorderHighlight;
        #endregion

        #region Shortcuts
        void TextWithLabel(string labelStr, ref string text)
        {
            using (HORIZONTAL)
            {
                GUILayout.Label($"{labelStr}", StlLabel, GUILayout.ExpandWidth(false));
                text = EditorGUILayout.TextField(text);
            }
        }
        #endregion

        void Reset(SerializedProperty pActive, SerializedProperty pOrigin)
        {
            switch (pActive.propertyType)
            {
                case SerializedPropertyType.Generic:
                    if (pActive.type == "RectOffset")
                    {
                        var subActive = pActive.Copy();
                        var subOrigin = pOrigin.Copy();
                        subActive.NextVisible(true);
                        subOrigin.NextVisible(true);

                        for (int i = 0; i < 4; ++i)
                        {
                            subActive.intValue = subOrigin.intValue;
                            subActive.NextVisible(false);
                            subOrigin.NextVisible(false);
                        }
                    }
                    else if (pActive.type == "vector")
                    {
                        var size = pOrigin.arraySize;
                        var subActive = pActive.Copy();
                        var subOrigin = pOrigin.Copy();
                        subActive.NextVisible(true);
                        subOrigin.NextVisible(true);

                        for (int i = 0; i <= size; ++i)
                        {
                            Reset(subActive, subOrigin);
                            subActive.NextVisible(false);
                            subOrigin.NextVisible(false);
                        }
                    }
                    else throw new NotImplementedException();
                    break;
                case SerializedPropertyType.Integer: pActive.intValue = pOrigin.intValue; break;
                case SerializedPropertyType.Boolean: pActive.boolValue = pOrigin.boolValue; break;
                case SerializedPropertyType.Float: pActive.floatValue = pOrigin.floatValue; break;
                case SerializedPropertyType.String: pActive.stringValue = pOrigin.stringValue; break;
                case SerializedPropertyType.Color: pActive.colorValue = pOrigin.colorValue; break;
                case SerializedPropertyType.ObjectReference: pActive.objectReferenceInstanceIDValue = pOrigin.objectReferenceInstanceIDValue; break;
                case SerializedPropertyType.LayerMask: throw new NotImplementedException();
                case SerializedPropertyType.Enum: pActive.enumValueIndex = pOrigin.enumValueIndex; break;
                case SerializedPropertyType.Vector2: pActive.vector2Value = pOrigin.vector2Value; break;
                case SerializedPropertyType.Vector3: pActive.vector3Value = pOrigin.vector3Value; break;
                case SerializedPropertyType.Vector4: pActive.vector4Value = pOrigin.vector4Value; break;
                case SerializedPropertyType.Rect: pActive.rectValue = pOrigin.rectValue; break;
                case SerializedPropertyType.ArraySize: pActive.intValue = pOrigin.intValue; break;
                case SerializedPropertyType.Character: throw new NotImplementedException();
                case SerializedPropertyType.AnimationCurve: pActive.animationCurveValue = pOrigin.animationCurveValue; break;
                case SerializedPropertyType.Bounds: pActive.boundsValue = pOrigin.boundsValue; break;
                case SerializedPropertyType.Gradient: throw new NotImplementedException();
                case SerializedPropertyType.Quaternion: pActive.quaternionValue = pOrigin.quaternionValue; break;
                case SerializedPropertyType.ExposedReference: pActive.exposedReferenceValue = pOrigin.exposedReferenceValue; break;
                case SerializedPropertyType.FixedBufferSize: throw new NotImplementedException();
                case SerializedPropertyType.Vector2Int: pActive.vector2IntValue = pOrigin.vector2IntValue; break;
                case SerializedPropertyType.Vector3Int: pActive.vector3IntValue = pOrigin.vector3IntValue; break;
                case SerializedPropertyType.RectInt: pActive.rectIntValue = pOrigin.rectIntValue; break;
                case SerializedPropertyType.BoundsInt: pActive.boundsIntValue = pOrigin.boundsIntValue; break;
                case SerializedPropertyType.ManagedReference: throw new NotImplementedException();
            }
        }
        string GetItorCode(SerializedProperty p, string name)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Generic:
                    if (p.type == "RectOffset") return $"{name} = new RectOffset({p.FindPropertyRelative("m_Left").intValue}, {p.FindPropertyRelative("m_Right").intValue}, {p.FindPropertyRelative("m_Top").intValue}, {p.FindPropertyRelative("m_Bottom").intValue})";
                    else if (p.type == "vector")
                    {
                        string elements = "";
                        for (int i = 0; i < p.arraySize; ++i)
                        {
                            var oi = p.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (oi == null)
                            {
                                elements += "null, ";
                            }
                            else
                            {
                                var path = AssetDatabase.GetAssetPath(oi);
                                if (!path.StartsWith("Assets"))
                                {
                                    elements += "null, ";
                                    LogError($"贴图路径不在Assets中！Property: {Regex.Replace(p.propertyPath, "(?:stlInspecting\\.)?m_(.)", m => m.Groups[1].Value.ToLower())}[{i}] | Path: {path}");
                                }
                                else
                                {
                                    elements += $"AssetDatabase.LoadAssetAtPath<Texture2D>(\"{path}\"), ";
                                }
                            }
                        }
                        return $"{name} = new Texture2D[] {{ {elements}}}";
                    }
                    return $"[Error Type!{p.type}]";
                case SerializedPropertyType.Integer: return $"{name} = {p.intValue}";
                case SerializedPropertyType.Boolean: return $"{name} = {(p.boolValue ? "true" : "false")}";
                case SerializedPropertyType.Float: return $"{name} = {p.floatValue}f";
                case SerializedPropertyType.String: var strVal = p.stringValue; return $"{name} = \"{(string.IsNullOrEmpty(strVal) ? " "/* 确保至少有一个空格，不然GUIStyle.Check方法会失效 */ : strVal)}\"";
                case SerializedPropertyType.Color: var cVal = p.colorValue; return $"{name} = new Color({cVal.r}f, {cVal.g}f, {cVal.b}f{(cVal.a == 1 ? "" : $", {cVal.a}f")})";
                case SerializedPropertyType.ObjectReference: var o = p.objectReferenceValue; return $"{name} = {(o == null ? "null" : $"AssetDatabase.LoadAssetAtPath<{o.GetType().Name}>(\"{AssetDatabase.GetAssetPath(o)}\")")}";
                case SerializedPropertyType.LayerMask: return $"[Error Type!{p.propertyType}]";
                case SerializedPropertyType.Enum: var et = typeof(GUIStyle).GetProperty(name)?.PropertyType; return $"{name} = {et.Name}.{et.GetEnumName(p.enumValueIndex)}";
                case SerializedPropertyType.Vector2: var vec2 = p.vector2Value; return $"{name} = new Vector2({vec2.x}f, {vec2.y}f)";
                case SerializedPropertyType.Vector3: var vec3 = p.vector3Value; return $"{name} = new Vector3({vec3.x}f, {vec3.y}f, {vec3.z}f)";
                case SerializedPropertyType.Vector4: var vec4 = p.vector4Value; return $"{name} = new Vector4({vec4.x}f, {vec4.y}f, {vec4.z}f, {vec4.w}f)";
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.ManagedReference: return $"[Error Type!{p.propertyType}]";
            }
            return "";
        }

        Rect sideRect;
        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var mainRect, out var subRect, "HierachyArea", 512, IceGUIDirection.Left))
            {
                // 左侧区域
                using (Area(subRect)) using (BOX)
                {
                    // 搜索框
                    using (BOX) SearchField(stlMap.Keys, ref stlFiltered);

                    // 在这显示.
                    using (SCROLL)
                    {
                        // 显示样式列表
                        for (int i = 0; i < stlFiltered.Count; ++i)
                        {
                            using (Horizontal(GUILayout.Height(32)))
                            {

                                (var displayName, var name) = stlFiltered[i];
                                var stl = stlMap[name];
                                using (Vertical(GUILayout.Width(160)))
                                {
                                    // 标签
                                    if (GUILayout.Button(displayName, StlLabelBtn))
                                    {
                                        // 复制样式名
                                        GUIUtility.systemCopyBuffer = name;

                                        // 刷新监控对象
                                        stlInspectingOrigin = new GUIStyle(stl);
                                        stlInspecting = new GUIStyle(stl);
                                        PropertyObj.Update();
                                        SetBool("Property Dirty", false);
                                    }
                                    // 自定义按钮
                                    //if (CustomButton("B"))
                                    //{
                                    //    Debug.Log("B!");
                                    //}
                                }

                                // 显示
                                if (guiType == GUIType.GUILayout)
                                {
                                    DisplayStyle(stl);
                                }
                                else
                                {
                                    // GUI
                                    GUILayout.Box(GUIContent.none, GetBool("显示边框") ? StlGUIBorder : GUIStyle.none, GUILayout.Width(GetFloat("Val GUIRect width", 64)), GUILayout.Height(GetFloat("Val GUIRect height", 64)));
                                    var rect = GUILayoutUtility.GetLastRect();
                                    DisplayStyle(rect, stl);
                                }
                            }

                        }
                    }

                    // 显示选项
                    using (GROUP)
                    {
                        if (guiType == GUIType.GUILayout)
                        {
                            void LayoutOptionFloatField(string name)
                            {
                                if (GetBool($"Has Layout {name}"))
                                {
                                    using (BOX) using (HORIZONTAL)
                                    {
                                        FloatField($"Val Layout {name}", 0, name);
                                        if (GUILayout.Button(GUIContent.none, "OL Minus", GUILayout.ExpandWidth(false)))
                                        {
                                            SetBool($"Has Layout {name}", false);
                                        }
                                    }
                                }
                            }
                            void LayoutOptionBoolField(string name)
                            {
                                if (GetBool($"Has Layout {name}"))
                                {
                                    using (BOX) using (HORIZONTAL)
                                    {
                                        Toggle($"Val Layout {name}", false, name);
                                        if (GUILayout.Button(GUIContent.none, "OL Minus", GUILayout.ExpandWidth(false)))
                                        {
                                            SetBool($"Has Layout {name}", false);
                                        }
                                    }
                                }
                            }
                            EditorGUI.BeginChangeCheck();
                            foreach (var o in layoutFloatOptions) LayoutOptionFloatField(o.name);
                            foreach (var o in layoutBoolOptions) LayoutOptionBoolField(o.name);
                            if (EditorGUI.EndChangeCheck()) ConstructLayoutOptions();
                        }
                        else if (guiType == GUIType.GUI)
                        {
                            using (BOX) FloatField("Val GUIRect x", 0, "x");
                            using (BOX) FloatField("Val GUIRect y", 0, "y");
                            using (BOX) FloatField("Val GUIRect width", 64, "width");
                            using (BOX) FloatField("Val GUIRect height", 64, "height");
                            using (BOX) Toggle("显示边框");
                        }

                        using (HORIZONTAL)
                        {
                            // TODO!
                            Vector2 size;

                            GUI.color = new Color(0.306f, 0.788f, 0.690f);
                            size = StlCode.CalcSize(TempContent(guiType.ToString()));
                            guiType = (GUIType)EditorGUI.EnumPopup(GUILayoutUtility.GetRect(size.x + 2, size.y, GUILayout.ExpandWidth(false)), guiType, StlCode);

                            GUI.color = Color.white;
                            GUILayout.Label(".", StlCode, GUILayout.ExpandWidth(false));

                            GUI.color = new Color(0.863f, 0.863f, 0.667f);
                            size = StlCode.CalcSize(TempContent(displayType.ToString()));
                            displayType = (DisplayType)EditorGUI.EnumPopup(GUILayoutUtility.GetRect(size.x + 2, size.y, GUILayout.ExpandWidth(false)), displayType, StlCode);

                            GUI.color = Color.white;
                            GUILayout.Label("(", StlCode, GUILayout.ExpandWidth(false));

                            GUI.color = new Color(0.839f, 0.616f, 0.522f);
                            GUILayout.Label("\"", StlCode, GUILayout.ExpandWidth(false));

                            GUI.color = new Color(1, 0.66f, 0.56f);
                            displayText = GUILayout.TextField(displayText);

                            GUI.color = new Color(0.839f, 0.616f, 0.522f);
                            GUILayout.Label("\"", StlCode, GUILayout.ExpandWidth(false));

                            GUI.color = Color.white;

                            if (guiType == GUIType.GUILayout)
                            {
                                GUILayout.Label(",", StlCode, GUILayout.ExpandWidth(false));

                                if (GUILayout.Button(GUIContent.none, "OL Plus", GUILayout.ExpandWidth(false)))
                                {
                                    GenericMenu gm = new GenericMenu();
                                    void LayoutOptionToggle(string name) { if (!GetBool($"Has Layout {name}")) gm.AddItem(TempContent(name), false, () => { SetBool($"Has Layout {name}", true); ConstructLayoutOptions(); }); }
                                    foreach (var o in layoutFloatOptions) LayoutOptionToggle(o.name);
                                    foreach (var o in layoutBoolOptions) LayoutOptionToggle(o.name);
                                    gm.ShowAsContext();
                                }
                            }

                            GUILayout.Label(");", StlCode, GUILayout.ExpandWidth(false));
                        }
                    }
                }

                // 右侧区域
                using (Area(mainRect))
                {
                    using (Vertical(StlGroup, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
                    {
                        // 预览
                        if (guiType == GUIType.GUILayout)
                        {
                            DisplayStyle(stlInspecting);
                        }
                        else
                        {
                            var rect = new Rect(
                                GetFloat("Val GUIRect x"),
                                GetFloat("Val GUIRect y"),
                                GetFloat("Val GUIRect width", 64),
                                GetFloat("Val GUIRect height", 64));
                            if (GetBool("显示边框"))
                            {
                                GUI.Box(rect, GUIContent.none, StlGUIBorder);
                                GUI.Box(rect, GUIContent.none, StlGUIBorderHighlight);
                            }
                            DisplayStyle(rect, stlInspecting);
                        }
                    }

                    using (GROUP) using (SectionFolder("Inspector"))
                    {
                        // 显示属性
                        using (BOX)
                        {
                            if (GetAnimBoolFaded("GUIStyle属性", false) > 0.27f) GUI.Label(sideRect, GUIContent.none, GetBool("Property Dirty") ? "flow node 5" : "IN EditColliderButton");

                            using (SectionFolder("GUIStyle属性", false))
                            {
                                void PropertyField(SerializedProperty pActive, SerializedProperty pOrigin)
                                {
                                    using (HORIZONTAL)
                                    {
                                        EditorGUILayout.PropertyField(pActive, GUILayout.ExpandWidth(true), GUILayout.MinWidth(208));

                                        GUILayout.Space(2);
                                        if (!SerializedProperty.DataEquals(pActive, pOrigin))
                                        {
                                            SetBool("Property Dirty", true);
                                            if (IceButton("<color=#FA0>↺</color>", "重置数据"))
                                            {
                                                GUIUtility.keyboardControl = 0;
                                                Reset(pActive, pOrigin);
                                            }
                                        }
                                        else GUILayout.Space(20);
                                        //if (pActive.hasVisibleChildren) CustomButton("<color=#0FF>></color>");
                                        //CustomButton(pActive.propertyType.ToString());
                                        //CustomButton(pActive.type);
                                    }
                                }

                                PropertyObj.UpdateIfRequiredOrScript();

                                var pOrigin = PropertyObj.FindProperty("stlInspectingOrigin");
                                var pActive = PropertyObj.FindProperty("stlInspecting");

                                using (ScrollAuto(GUILayout.MaxHeight(512)))
                                {
                                    EditorGUI.BeginChangeCheck();

                                    pOrigin.NextVisible(true);
                                    pActive.NextVisible(true);

                                    do
                                    {
                                        if (pActive.type == "GUIStyleState")
                                        {
                                            // 展开GUIStyleState
                                            var foldoutKey = "StyleProperty_" + pActive.name;
                                            var foldout = false;

                                            using (HORIZONTAL)
                                            {
                                                foldout = SetBool(foldoutKey, EditorGUILayout.Foldout(GetBool(foldoutKey), pActive.displayName));

                                                GUILayout.Space(2);
                                                if (SerializedProperty.DataEquals(pActive, pOrigin)) GUILayout.Space(20);
                                                else
                                                {
                                                    SetBool("Property Dirty", true);
                                                    if (IceButton($"<color=#FA0>※</color>", "有更改", GUILayout.Width(17))) SetBool(foldoutKey, true);
                                                }
                                            }

                                            if (foldout)
                                            {
                                                ++EditorGUI.indentLevel;
                                                var subOrigin = pOrigin.Copy();
                                                var subActive = pActive.Copy();
                                                subOrigin.NextVisible(true);
                                                subActive.NextVisible(true);


                                                for (int i = 0; i < 3; ++i)
                                                {
                                                    PropertyField(subActive, subOrigin);
                                                    subActive.NextVisible(false);
                                                    subOrigin.NextVisible(false);
                                                }
                                                --EditorGUI.indentLevel;
                                            }
                                        }
                                        else PropertyField(pActive, pOrigin);
                                    } while (pActive.NextVisible(false) && pOrigin.NextVisible(false));

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        SetBool("Property Dirty", false);
                                        try
                                        {
                                            PropertyObj.ApplyModifiedPropertiesWithoutUndo();
                                        }
                                        catch (UnityException) { /* do nothing */}
                                    }
                                }

                            }
                            if (Event.current.type == EventType.Repaint)
                            {
                                sideRect = GUILayoutUtility.GetLastRect();
                                sideRect.x = sideRect.xMax - 23;
                                sideRect.width = 23;
                            }
                        }
                        // 生成代码
                        using (BOX) using (HORIZONTAL)
                        {
                            TextWithLabel("名字", ref styleName);
                            if (IceButton("生成代码"))
                            {
                                GUIUtility.systemCopyBuffer = GetStyleCode(styleName);
                                Log("样式代码已生成！");
                            }
                        }
                        // 样式列表
                        using (BOX) using (HORIZONTAL)
                        {
                            Action tempAction = null;
                            foreach (var sc in stlCacheMap)
                            {
                                GUILayout.Box(GUIContent.none, StlGUIBorder, GUILayout.Width(64), GUILayout.Height(64));
                                var rect = GUILayoutUtility.GetLastRect();

                                if (GUI.Button(rect, displayText, sc.Value.stl))
                                {
                                    var e = Event.current;
                                    if (e.button == 1)
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        menu.AddItem(new GUIContent("记录"), false, () =>
                                        {
                                            stlCacheMap[sc.Key] = new StyleCache(new GUIStyle(stlInspecting), new GUIStyle(stlInspectingOrigin));
                                        });
                                        menu.AddItem(new GUIContent("取出"), false, () =>
                                        {
                                            stlInspecting = new GUIStyle(sc.Value.stl);
                                            stlInspectingOrigin = new GUIStyle(sc.Value.stlOrigin);
                                            PropertyObj.Update();
                                            SetBool("Property Dirty", false);
                                        });
                                        menu.AddItem(new GUIContent("删除"), false, () =>
                                        {
                                            if (EditorUtility.DisplayDialog(TitleContent.text, $"确认删除样式：{sc.Key}", "确认", "取消"))
                                            {
                                                stlCacheMap.Remove(sc.Key);
                                            }
                                        });
                                        menu.ShowAsContext();
                                    }
                                    else
                                    {
                                        // 左键点击
                                        if (e.shift || e.control)
                                        {
                                            // 记录
                                            tempAction = () =>
                                            {
                                                stlCacheMap[sc.Key] = new StyleCache(new GUIStyle(stlInspecting), new GUIStyle(stlInspectingOrigin));
                                            };
                                        }
                                        else
                                        {
                                            // 取出
                                            stlInspecting = new GUIStyle(sc.Value.stl);
                                            stlInspectingOrigin = new GUIStyle(sc.Value.stlOrigin);
                                            PropertyObj.Update();
                                            SetBool("Property Dirty", false);
                                        }
                                    }
                                }

                                GUI.Box(rect, sc.Key, StlGUIBorderHighlight);
                            }
                            tempAction?.Invoke();
                            using (Vertical(GUILayout.ExpandWidth(false), GUILayout.MinHeight(17), GUILayout.MaxHeight(64)))
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(GUIContent.none, "OL Plus", GUILayout.ExpandWidth(false)))
                                {
                                    stlCacheMap[string.IsNullOrWhiteSpace(stlInspectingOrigin.name) ? "None" : stlInspectingOrigin.name] = new StyleCache(new GUIStyle(stlInspecting), new GUIStyle(stlInspectingOrigin));
                                }
                                GUILayout.FlexibleSpace();
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}