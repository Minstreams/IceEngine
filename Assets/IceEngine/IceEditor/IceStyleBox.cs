﻿// 需要整理，非最终版

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using IceEngine;

namespace IceEditor
{
    /// <summary>
    /// GUI Style Box
    /// </summary>
    public sealed class IceStyleBox : IceEditorWindow
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
        const string filterColorStr = "#F80";
        const string activeColorStr = "#F80";
        #endregion

        #region 【接口】
        /// <summary>
        /// 单例
        /// </summary>
        public static IceStyleBox Instance => _instance != null ? _instance : (_instance = OpenWindow()); static IceStyleBox _instance;
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
                Instance.stlCacheMap.Add(key, new StyleCache(stl, new GUIStyle(stl.name)));
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
        /// 筛选并生成列表
        /// </summary>
        public void DoFilterStyleList()
        {
            /// <summary>
            /// 比较两个char的值，并指定是否区分大小写
            /// </summary>
            /// <param name="caseSensitive">是否区分大小写</param>
            static bool CompareChar(char self, char other, bool caseSensitive = true)
            {
                if (caseSensitive) return self == other;
                else return char.ToLower(self) == char.ToLower(other);
            }

            bool useRegex = GetBool("正则表达式");
            bool continuousMatching = GetBool("连续匹配");
            bool caseSensitive = GetBool("区分大小写");

            stlListFiltered.Clear();
            if (string.IsNullOrWhiteSpace(filterStr))
            {
                stlListFiltered.Add(new KeyValuePair<string, GUIStyle>("None", new GUIStyle()));
                foreach (var stl in stlList)
                {
                    stlListFiltered.Add(new KeyValuePair<string, GUIStyle>(stl.name, stl));
                }
            }
            else
            {
                if (useRegex)
                {
                    // 正则表达式匹配
                    foreach (var stl in stlList)
                    {
                        try
                        {
                            var option = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                            if (Regex.IsMatch(stl.name, filterStr, option))
                            {
                                string name = Regex.Replace(stl.name, filterStr, $"<color={filterColorStr}>$0</color>", option);
                                stlListFiltered.Add(new KeyValuePair<string, GUIStyle>(name, stl));
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    // 判断是否包含filter关键字
                    if (continuousMatching)
                    {
                        foreach (var stl in stlList)
                        {
                            // 连续匹配
                            int index = stl.name.IndexOf(filterStr, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                            if (index < 0) continue;

                            // 替换关键字为指定颜色
                            string name = stl.name
                                .Insert(index + filterStr.Length, "</color>")
                                .Insert(index, $"<color={filterColorStr}>");

                            stlListFiltered.Add(new KeyValuePair<string, GUIStyle>(name, stl));
                        }
                    }
                    else
                    {
                        foreach (var stl in stlList)
                        {
                            int l = filterStr.Length;
                            {
                                // 离散匹配
                                int i = 0;
                                foreach (char c in stl.name) if (CompareChar(c, filterStr[i], caseSensitive) && ++i == l) break;
                                // 不包含则跳过
                                if (i < l) continue;
                            }


                            string name = string.Empty;
                            {
                                // 替换关键字为指定颜色
                                int i = 0;
                                foreach (char c in stl.name)
                                {
                                    if (i < l && CompareChar(c, filterStr[i], caseSensitive))
                                    {
                                        name += $"<color={filterColorStr}>{c}</color>";
                                        ++i;
                                    }
                                    else name += c;
                                }
                            }
                            stlListFiltered.Add(new KeyValuePair<string, GUIStyle>(name, stl));
                        }
                    }
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

            if (!hasItor && !hasInit) return $"GUIStyle Stl{styleName} => \"{stlInspectingOrigin.name}\";";
            else return $"GUIStyle Stl{styleName} => _stl{styleName}?.Check() ?? (_stl{styleName} = new GUIStyle({(string.IsNullOrEmpty(stlInspectingOrigin.name) ? "" : $"\"{stlInspectingOrigin.name}\"")}){ (hasItor ? $" {{ {itorCode}}}" : "")}{(hasInit ? $".Initialize(stl => {{ {initCode}}})" : "")}); GUIStyle _stl{styleName};";

        }
        #endregion

        #region 【字段】
        bool _inGameSkin = false;

        readonly List<GUIStyle> stlList = new List<GUIStyle>();
        readonly List<KeyValuePair<string, GUIStyle>> stlListFiltered = new List<KeyValuePair<string, GUIStyle>>();
        string filterStr = string.Empty;

        [SerializeField] GUIStyle stlInspectingOrigin = new GUIStyle();
        [SerializeField] GUIStyle stlInspecting = new GUIStyle();

        string styleName = string.Empty;
        #endregion

        #region 【定制】

        [MenuItem("IceSystem/Style Box", false, 20)]
        static IceStyleBox OpenWindow()
        {
            var window = GetWindow<IceStyleBox>();
            window.InGameSkin = false;
            return window;
        }
        public override GUIContent TitleContent => new GUIContent(InGameSkin ? "GUIStyle 样例窗口 - InGameSkin" : "GUIStyle 样例窗口");
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
            var skin = EditorGUIUtility.GetBuiltinSkin(InGameSkin ? EditorSkin.Game : EditorSkin.Scene);
            var sList = skin.customStyles;

            stlList.Clear();
            foreach (var d in dList) stlList.Add(skin.GetStyle(d));
            stlList.AddRange(sList);

            DoFilterStyleList();

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

        readonly Dictionary<string, StyleCache> stlCacheMap = new Dictionary<string, StyleCache>();
        #endregion

        #endregion

        #region 【GUI】
        #region Style
        // Style
        GUIStyle StlLabelBtn => _stlLabelBtn?.Check() ?? (_stlLabelBtn = new GUIStyle("ToolbarTextField") { richText = true, stretchWidth = true, fixedHeight = 0, wordWrap = true }); GUIStyle _stlLabelBtn;
        GUIStyle StlCode => _stlCode?.Check() ?? (_stlCode = new GUIStyle("label") { margin = new RectOffset(1, 1, 0, 0), padding = new RectOffset(0, 0, 1, 1), fontSize = 14, fontStyle = FontStyle.Bold, richText = true }).Initialize(stl => stl.normal.textColor = Color.white); GUIStyle _stlCode;
        GUIStyle StlGUIBorder => _stlGUIBorder?.Check() ?? (_stlGUIBorder = new GUIStyle("grey_border") { margin = new RectOffset(0, 16, 0, 0), }); GUIStyle _stlGUIBorder;
        GUIStyle StlGUIBorderHighlight => _stlGUIBorderHighlight?.Check() ?? (_stlGUIBorderHighlight = new GUIStyle("LightmapEditorSelectedHighlight") { overflow = new RectOffset(7, 7, 7, 7), fontSize = 10, alignment = TextAnchor.LowerCenter, wordWrap = true, imagePosition = ImagePosition.TextOnly, contentOffset = new Vector2(0f, -70f), }.Initialize(stl => { stl.normal.textColor = new Color(1f, 0.9862055f, 0f); })); GUIStyle _stlGUIBorderHighlight;
        GUIStyle StlSearchTextField => _stlSearchTextField?.Check() ?? (_stlSearchTextField = new GUIStyle("SearchTextField") { padding = new RectOffset(14, 3, 2, 1), fontSize = 12, fixedHeight = 0f, }); GUIStyle _stlSearchTextField;
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
                                    LogError($"贴图路径不在Assets中！Property: { Regex.Replace(p.propertyPath, "(?:stlInspecting\\.)?m_(.)", m => m.Groups[1].Value.ToLower())}[{i}] | Path: {path}");
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
            const float mWidth = 8; //中间分隔栏的宽
            const string useRegexKey = "正则表达式";
            const string continuousMatchingKey = "连续匹配";
            const string caseSensitiveKey = "区分大小写";

            const string leftPanelWidthKey = "Left Panel Width";

            var lpWidth = Mathf.Clamp(GetFloat(leftPanelWidthKey, Mathf.Min(512, position.width - 2)), mWidth + 2, position.width - 2);

            // 左侧区域
            Rect rL = new Rect(0, 0, lpWidth - mWidth, position.height);
            using (Area(rL)) using (BOX)
            {
                // 搜索框
                using (BOX) using (HORIZONTAL)
                {
                    EditorGUI.BeginChangeCheck();

                    //GUILayout.Label((GetBool(useRegexKey) ? $"<color={activeColorStr}>表达式</color>" : "关键字"), StlLabel, GUILayout.ExpandWidth(false));
                    filterStr = EditorGUILayout.TextField(filterStr, StlSearchTextField);
                    if (!GetBool(useRegexKey))
                    {
                        IceToggle(continuousMatchingKey, false, "连", "连续匹配");
                    }
                    IceToggle(caseSensitiveKey, false, "Aa", "区分大小写");
                    IceToggle(useRegexKey, false, ".*".Bold(), "使用正则表达式");

                    if (EditorGUI.EndChangeCheck()) DoFilterStyleList();
                }

                // 在这显示.
                using (SCROLL)
                {
                    // 显示样式列表
                    for (int i = 0; i < stlListFiltered.Count; ++i)
                    {
                        using (Horizontal(GUILayout.Height(32)))
                        {
                            var stl = stlListFiltered[i];
                            using (Vertical(GUILayout.Width(160)))
                            {
                                // 标签
                                if (GUILayout.Button(stl.Key, StlLabelBtn))
                                {
                                    // 复制样式名
                                    GUIUtility.systemCopyBuffer = stl.Value.name;

                                    // 刷新监控对象
                                    stlInspectingOrigin = new GUIStyle(stl.Value);
                                    stlInspecting = new GUIStyle(stl.Value);
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
                                DisplayStyle(stl.Value);
                            }
                            else
                            {
                                // GUI
                                GUILayout.Box(GUIContent.none, GetBool("显示边框") ? StlGUIBorder : GUIStyle.none, GUILayout.Width(GetFloat("Val GUIRect width", 64)), GUILayout.Height(GetFloat("Val GUIRect height", 64)));
                                var rect = GUILayoutUtility.GetLastRect();
                                DisplayStyle(rect, stl.Value);
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
                        size = StlCode.CalcSize(new GUIContent(guiType.ToString()));
                        guiType = (GUIType)EditorGUI.EnumPopup(GUILayoutUtility.GetRect(size.x + 2, size.y, GUILayout.ExpandWidth(false)), guiType, StlCode);

                        GUI.color = Color.white;
                        GUILayout.Label(".", StlCode, GUILayout.ExpandWidth(false));

                        GUI.color = new Color(0.863f, 0.863f, 0.667f);
                        size = StlCode.CalcSize(new GUIContent(displayType.ToString()));
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
                                void LayoutOptionToggle(string name) { if (!GetBool($"Has Layout {name}")) gm.AddItem(new GUIContent(name), false, () => { SetBool($"Has Layout {name}", true); ConstructLayoutOptions(); }); }
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
            Rect rR = new Rect(lpWidth, 0, position.width - lpWidth, position.height);
            using (Area(rR))
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

                            var pOrigin = PropertyObj.FindProperty("stlInspectingOrigin");
                            var pActive = PropertyObj.FindProperty("stlInspecting");

                            using (Scroll(GUILayout.MaxHeight(512)))
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
                            LogImportant("样式代码已生成！");
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

            // 分隔栏
            Rect rM = new Rect(lpWidth - mWidth, 0, mWidth, position.height);
            {
                int id = GUIUtility.GetControlID(FocusType.Passive);
                const string draggingSeparatorKey = "Dragging Separator In Window";
                var draggingSeparator = GetBool(draggingSeparatorKey);

                var e = Event.current;
                switch (e.GetTypeForControl(id))
                {
                    case EventType.MouseDown:
                        if (rM.Contains(e.mousePosition))
                        {
                            GUIUtility.hotControl = id;

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

                            if (!docked)
                            {
                                float tLast = GetFloat("Last MouseUp Time");
                                float t = (float)EditorApplication.timeSinceStartup;
                                if (t - tLast < 0.4f)
                                {
                                    // 双击
                                    float w = GetFloat(leftPanelWidthKey);
                                    float wp = position.width - mWidth - 2;
                                    if (wp <= w)
                                    {
                                        // 展开
                                        w = wp;
                                        SetFloat(leftPanelWidthKey, w);
                                        var pos = this.position;
                                        pos.width += 384;
                                        this.position = pos;
                                    }
                                    else
                                    {
                                        // 缩放
                                        var pos = this.position;
                                        pos.width += w + mWidth + 2 - position.width;
                                        this.position = pos;
                                    }
                                    SetFloat("Last MouseUp Time", -1);
                                }
                                else
                                {
                                    SetFloat("Last MouseUp Time", (float)EditorApplication.timeSinceStartup);
                                }
                            }
                            e.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (draggingSeparator)
                        {
                            SetFloat(leftPanelWidthKey, lpWidth + e.delta.x);
                            Repaint();
                            e.Use();
                        }
                        break;
                }
                GUI.Box(rM, GUIContent.none, draggingSeparator ? StlSeparatorOn : StlSeparator);

                EditorGUIUtility.AddCursorRect(rM, MouseCursor.ResizeHorizontal);
            }
        }
        protected override void OnDebugGUI(Rect position)
        {

        }
        #endregion
    }
}