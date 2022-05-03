﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IceEngine;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using static IceEditor.IceGUI;
using UnityEngine.Rendering;

namespace IceEditor
{
    /// <summary>
    /// 自动临时数据GUI
    /// </summary>
    public static class IceGUIAuto
    {
        public static IceGUIUtility.GUIPackScope UsePack(IceGUIAutoPack pack) => new IceGUIUtility.GUIPackScope(pack);
        static IceGUIAutoPack Pack => IceGUIUtility.CurrentPack;

        #region 临时数据托管
        public static Color GetColor(string key) => Pack.GetColor(key);
        public static Color GetColor(string key, Color defaultVal) => Pack.GetColor(key, defaultVal);
        public static Color SetColor(string key, Color value) => Pack.SetColor(key, value);

        public static bool GetBool(string key, bool defaultVal = false) => Pack.GetBool(key, defaultVal);
        public static bool SetBool(string key, bool value) => Pack.SetBool(key, value);

        public static AnimBool GetAnimBool(string key, bool defaultVal = false) => Pack.GetAnimBool(key, defaultVal);
        public static bool GetAnimBoolValue(string key, bool defaultVal = false) => Pack.GetAnimBoolValue(key, defaultVal);
        public static bool GetAnimBoolTarget(string key, bool defaultVal = false) => Pack.GetAnimBoolTarget(key, defaultVal);
        public static float GetAnimBoolFaded(string key, bool defaultVal = false) => Pack.GetAnimBoolFaded(key, defaultVal);
        public static bool SetAnimBoolValue(string key, bool value) => Pack.SetAnimBoolValue(key, value);
        public static bool SetAnimBoolTarget(string key, bool value) => Pack.SetAnimBoolTarget(key, value);

        public static int GetInt(string key, int defaultVal = 0) => Pack.GetInt(key, defaultVal);
        public static int SetInt(string key, int value) => Pack.SetInt(key, value);

        public static float GetFloat(string key, float defaultVal = 0) => Pack.GetFloat(key, defaultVal);
        public static float SetFloat(string key, float value) => Pack.SetFloat(key, value);

        public static string GetString(string key, string defaultVal = "") => Pack.GetString(key, defaultVal);
        public static string SetString(string key, string value) => Pack.SetString(key, value);

        public static Vector2 GetVector2(int key, Vector2 defaultVal = default) => Pack.GetVector2(key, defaultVal);
        public static Vector2 SetVector2(int key, Vector2 value) => Pack.SetVector2(key, value);

        public static Vector2 GetVector2(string key, Vector2 defaultVal = default) => Pack.GetVector2(key, defaultVal);
        public static Vector2 SetVector2(string key, Vector2 value) => Pack.SetVector2(key, value);

        public static Vector3 GetVector3(string key, Vector3 defaultVal = default) => Pack.GetVector3(key, defaultVal);
        public static Vector3 SetVector3(string key, Vector3 value) => Pack.SetVector3(key, value);

        public static Vector4 GetVector4(string key, Vector4 defaultVal = default) => Pack.GetVector4(key, defaultVal);
        public static Vector4 SetVector4(string key, Vector4 value) => Pack.SetVector4(key, value);

        public static Vector2Int GetVector2Int(string key, Vector2Int defaultVal = default) => Pack.GetVector2Int(key, defaultVal);
        public static Vector2Int SetVector2Int(string key, Vector2Int value) => Pack.SetVector2Int(key, value);

        public static Vector3Int GetVector3Int(string key, Vector3Int defaultVal = default) => Pack.GetVector3Int(key, defaultVal);
        public static Vector3Int SetVector3Int(string key, Vector3Int value) => Pack.SetVector3Int(key, value);
        #endregion

        #region Scope
        public static FolderScope Folder(string key, bool defaultVal = false, bool changeWidth = false) => new FolderScope(GetAnimBool(key, defaultVal), changeWidth);
        /// <summary>
        /// 显示一个可展开的节
        /// </summary>
        public static FolderScope SectionFolder(string key, bool defaultVal = true, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            if (IceGUIUtility.HasPack)
            {
                var ab = GetAnimBool(key, defaultVal);
                ab.target = GUILayout.Toggle(ab.target, label, StlSectionHeader);
                return new FolderScope(ab, true);
            }
            else
            {
                IceGUI.SectionHeader(label);
                return null;
            }
        }

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope Scroll(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个竖直Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope ScrollVertical(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个水平Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope ScrollHorizontal(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个隐藏bar的Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope ScrollInvisible(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope Scroll(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个竖直Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope ScrollVertical(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个水平Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope ScrollHorizontal(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, "horizontalscrollbar", GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个隐藏bar的Scroll View
        /// </summary>
        public static GUILayout.ScrollViewScope ScrollInvisible(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new GUILayout.ScrollViewScope(GetVector2(key), false, false, GUIStyle.none, GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        #endregion

        #region GUI Elements
        /// <summary>
        /// 画一个自适应Layout的Texture，自带控制项
        /// </summary>
        public static void TextureBoxComplex(Texture texture)
        {
            var name = texture.name;
            ColorWriteMask cwMask = ColorWriteMask.All;

            using (VERTICAL) using (NODE)
            {
                using (HORIZONTAL)
                {
                    IceToggle($"Expanded {name}", false, "Exp", "Is this texture Expanded");
                    /*if (!texture.graphicsFormat.ToString().Contains("UNorm"))*/
                    using (HORIZONTAL)
                    {
                        ColorWriteMask mask = (ColorWriteMask)0;
                        if (IceToggle($"Color Write Mask R {name}", true, "R")) mask |= ColorWriteMask.Red;
                        if (IceToggle($"Color Write Mask G {name}", true, "G")) mask |= ColorWriteMask.Green;
                        if (IceToggle($"Color Write Mask B {name}", true, "B")) mask |= ColorWriteMask.Blue;
                        if (IceToggle($"Color Write Mask A {name}", true, "A")) mask |= ColorWriteMask.Alpha;
                        if (mask != 0) cwMask = mask;
                    }
                    GUILayout.Label($"{texture.width} : {texture.height} | {(texture is Texture2D t2 ? t2.format.ToString() : texture is RenderTexture rt ? rt.format.ToString() : texture.GetType().ToString())}");
                    GUILayout.FlexibleSpace();
                    IntSliderNoLabel($"Mip Level {name}", 0, 0, texture.mipmapCount - 1);
                }

                using (Horizontal(StlIce))
                {
                    bool expanded = GetBool($"Expanded {name}");
                    if (!expanded) GUILayout.FlexibleSpace();
                    var rect = GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width >> GetInt($"Mip Level {name}")));
                    if (!expanded) GUILayout.FlexibleSpace();
                    if (Event.current.type == EventType.Repaint) EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit, 0, GetInt($"Mip Level {name}"), cwMask);
                }
            }
        }

        public static bool SectionHeader(string key, bool defaultVal = true, string labelOverride = null, params GUILayoutOption[] options) => IceGUI.SectionHeader(GetAnimBool(key, defaultVal), string.IsNullOrEmpty(labelOverride) ? key : labelOverride, options);

        public static bool ToggleNoLabel(string key, bool defaultVal = false, GUIStyle styleOverride = null, params GUILayoutOption[] options) => SetBool(key, _Toggle(GetBool(key, defaultVal), styleOverride, options));
        public static bool Toggle(string key, bool defaultVal = false, string labelOverride = null, GUIStyle styleOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return ToggleNoLabel(key, defaultVal, styleOverride, options); }

        public static bool ToggleLeft(string key, bool defaultVal = false, string textOverride = null, GUIStyle styleOverride = null, params GUILayoutOption[] options) => SetBool(key, _ToggleLeft(GetBool(key, defaultVal), string.IsNullOrEmpty(textOverride) ? key : textOverride, styleOverride, options));

        public static int IntFieldNoLabel(string key, int defaultVal = 0, params GUILayoutOption[] options) => SetInt(key, _IntField(GetInt(key, defaultVal), options));
        public static int IntField(string key, int defaultVal = 0, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return IntFieldNoLabel(key, defaultVal, options); }

        public static int IntSliderNoLabel(string key, int defaultVal, int min, int max, params GUILayoutOption[] options) => SetInt(key, _IntSlider(GetInt(key, defaultVal), min, max, options));
        public static int IntSlider(string key, int defaultVal, int min, int max, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return IntSliderNoLabel(key, defaultVal, min, max, options); }
        public static int IntSliderNoLabel(string key, int min, int max, params GUILayoutOption[] options) => SetInt(key, _IntSlider(GetInt(key), min, max, options));
        public static int IntSlider(string key, int min, int max, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return IntSliderNoLabel(key, min, max, options); }

        public static float FloatFieldNoLabel(string key, float defaultVal = 0, params GUILayoutOption[] options) => SetFloat(key, _FloatField(GetFloat(key, defaultVal), options));
        public static float FloatField(string key, float defaultVal = 0, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return FloatFieldNoLabel(key, defaultVal, options); }

        public static float SliderNoLabel(string key, float defaultVal, float min = 0, float max = 1, params GUILayoutOption[] options) => SetFloat(key, _Slider(GetFloat(key, defaultVal), min, max, options));
        public static float Slider(string key, float defaultVal, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return SliderNoLabel(key, defaultVal, min, max, options); }
        public static float SliderNoLabel(string key, float min = 0, float max = 1, params GUILayoutOption[] options) => SetFloat(key, _Slider(GetFloat(key), min, max, options));
        public static float Slider(string key, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return SliderNoLabel(key, min, max, options); }

        public static string TextFieldNoLabel(string key, string defaultVal = "", params GUILayoutOption[] options) => SetString(key, _TextField(GetString(key, defaultVal), options));
        public static string TextField(string key, string defaultVal = "", string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return TextFieldNoLabel(key, defaultVal, options); }

        public static Vector2 Vector2FieldNoLabel(string key, Vector2 defaultVal, string xLabel = null, string yLabel = null) => SetVector2(key, _Vector2Field(GetVector2(key, defaultVal), xLabel, yLabel));
        public static Vector2 Vector2Field(string key, Vector2 defaultVal, string labelOverride = null, string xLabel = null, string yLabel = null) { using (ControlLabel(key, labelOverride)) return Vector2FieldNoLabel(key, defaultVal, xLabel, yLabel); }
        public static Vector2 Vector2FieldNoLabel(string key, string xLabel = null, string yLabel = null) => SetVector2(key, _Vector2Field(GetVector2(key), xLabel, yLabel));
        public static Vector2 Vector2Field(string key, string labelOverride = null, string xLabel = null, string yLabel = null) { using (ControlLabel(key, labelOverride)) return Vector2FieldNoLabel(key, xLabel, yLabel); }

        public static Vector2 Vector2SliderNoLabel(string key, Vector2 defaultVal, float min = 0, float max = 1, params GUILayoutOption[] options) => SetVector2(key, _Vector2Slider(GetVector2(key, defaultVal), min, max, options));
        public static Vector2 Vector2Slider(string key, Vector2 defaultVal, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return Vector2SliderNoLabel(key, defaultVal, min, max, options); }
        public static Vector2 Vector2SliderNoLabel(string key, float min = 0, float max = 1, params GUILayoutOption[] options) => SetVector2(key, _Vector2Slider(GetVector2(key), min, max, options));
        public static Vector2 Vector2Slider(string key, float min = 0, float max = 1, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return Vector2SliderNoLabel(key, min, max, options); }

        public static Vector3 Vector3FieldNoLabel(string key, Vector3 defaultVal, string xLabel = null, string yLabel = null, string zLabel = null) => SetVector3(key, _Vector3Field(GetVector3(key, defaultVal), xLabel, yLabel, zLabel));
        public static Vector3 Vector3Field(string key, Vector3 defaultVal, string labelOverride = null, string xLabel = null, string yLabel = null, string zLabel = null) { using (ControlLabel(key, labelOverride)) return Vector3FieldNoLabel(key, defaultVal, xLabel, yLabel, zLabel); }
        public static Vector3 Vector3FieldNoLabel(string key, string xLabel = null, string yLabel = null, string zLabel = null) => SetVector3(key, _Vector3Field(GetVector3(key), xLabel, yLabel, zLabel));
        public static Vector3 Vector3Field(string key, string labelOverride = null, string xLabel = null, string yLabel = null, string zLabel = null) { using (ControlLabel(key, labelOverride)) return Vector3FieldNoLabel(key, xLabel, yLabel, zLabel); }

        public static Vector4 Vector4FieldNoLabel(string key, Vector4 defaultVal, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) => SetVector4(key, _Vector4Field(GetVector4(key, defaultVal), xLabel, yLabel, zLabel, wLabel));
        public static Vector4 Vector4Field(string key, Vector4 defaultVal, string labelOverride = null, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) { using (ControlLabel(key, labelOverride)) return Vector4FieldNoLabel(key, defaultVal, xLabel, yLabel, zLabel, wLabel); }
        public static Vector4 Vector4FieldNoLabel(string key, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) => SetVector4(key, _Vector4Field(GetVector4(key), xLabel, yLabel, zLabel, wLabel));
        public static Vector4 Vector4Field(string key, string labelOverride = null, string xLabel = null, string yLabel = null, string zLabel = null, string wLabel = null) { using (ControlLabel(key, labelOverride)) return Vector4FieldNoLabel(key, xLabel, yLabel, zLabel, wLabel); }

        public static Vector2Int Vector2IntFieldNoLabel(string key, Vector2Int defaultVal, string xLabel = null, string yLabel = null) => SetVector2Int(key, _Vector2IntField(GetVector2Int(key, defaultVal), xLabel, yLabel));
        public static Vector2Int Vector2IntField(string key, Vector2Int defaultVal, string labelOverride = null, string xLabel = null, string yLabel = null) { using (ControlLabel(key, labelOverride)) return Vector2IntFieldNoLabel(key, defaultVal, xLabel, yLabel); }
        public static Vector2Int Vector2IntFieldNoLabel(string key, string xLabel = null, string yLabel = null) => SetVector2Int(key, _Vector2IntField(GetVector2Int(key), xLabel, yLabel));
        public static Vector2Int Vector2IntField(string key, string labelOverride = null, string xLabel = null, string yLabel = null) { using (ControlLabel(key, labelOverride)) return Vector2IntFieldNoLabel(key, xLabel, yLabel); }

        public static Vector3Int Vector3IntFieldNoLabel(string key, Vector3Int defaultVal, string xLabel = null, string yLabel = null, string zLabel = null) => SetVector3Int(key, _Vector3IntField(GetVector3Int(key, defaultVal), xLabel, yLabel, zLabel));
        public static Vector3Int Vector3IntField(string key, Vector3Int defaultVal, string labelOverride = null, string xLabel = null, string yLabel = null, string zLabel = null) { using (ControlLabel(key, labelOverride)) return Vector3IntFieldNoLabel(key, defaultVal, xLabel, yLabel, zLabel); }
        public static Vector3Int Vector3IntFieldNoLabel(string key, string xLabel = null, string yLabel = null, string zLabel = null) => SetVector3Int(key, _Vector3IntField(GetVector3Int(key), xLabel, yLabel, zLabel));
        public static Vector3Int Vector3IntField(string key, string labelOverride = null, string xLabel = null, string yLabel = null, string zLabel = null) { using (ControlLabel(key, labelOverride)) return Vector3IntFieldNoLabel(key, xLabel, yLabel, zLabel); }

        public static Color ColorFieldNoLabel(string key, Color defaultVal, params GUILayoutOption[] options) => SetColor(key, _ColorField(GetColor(key, defaultVal), options));
        public static Color ColorField(string key, Color defaultVal, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return ColorFieldNoLabel(key, defaultVal, options); }
        public static Color ColorFieldNoLabel(string key, params GUILayoutOption[] options) => SetColor(key, _ColorField(GetColor(key), options));
        public static Color ColorField(string key, string labelOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return ColorFieldNoLabel(key, options); }

        /// <summary>
        /// Texture2D Field
        /// </summary>
        /// <returns>return true on changed</returns>
        public static bool TextureField(string label, ref Texture2D tex, params GUILayoutOption[] options)
        {
            CheckOptions(ref options);
            bool res = false;
            var previewOn = GetAnimBool($"{label} Preview On");
            using (HORIZONTAL)
            {
                Rect labelRect = GUILayoutUtility.GetRect(GUIContent.none, StlPrefix, GUILayout.Width(EditorGUIUtility.labelWidth - 1));

                const string controlName = "TextureField";
                GUI.SetNextControlName(controlName);
                using (GUICHECK)
                {
                    tex = (Texture2D)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(options), tex, typeof(Texture2D), false);
                    res = GUIChanged;
                }

                {
                    string focusedControl = GUI.GetNameOfFocusedControl();
                    bool on = focusedControl == controlName;
                    labelRect.y += 2;
                    if (ToggleButton(labelRect, label, on, StlPrefix))
                    {
                        GUI.FocusControl(controlName);
                        if (Event.current.button != 0)
                        {
                            GenericMenu gm = new GenericMenu();
                            gm.AddItem(new GUIContent("预览"), previewOn.target, () => previewOn.target = !previewOn.target);
                            gm.ShowAsContext();
                        }
                    }
                }
            }
            if (tex != null) using (var fs = new FolderScope(previewOn)) if (fs.visible) TextureBoxComplex(tex);

            return res;
        }

        public static bool IceToggle(string key, bool defaultVal = false, string textOverride = null, string tooltip = null, params GUILayoutOption[] options) => SetBool(key, _IceToggle(string.IsNullOrEmpty(textOverride) ? key : textOverride, GetBool(key, defaultVal), tooltip, options));
        public static bool IceToggleAnim(string key, bool defaultVal = false, string textOverride = null, string tooltip = null, params GUILayoutOption[] options) => SetAnimBoolTarget(key, _IceToggle(string.IsNullOrEmpty(textOverride) ? key : textOverride, GetAnimBoolTarget(key, defaultVal), tooltip, options));
        #endregion
    }
}
