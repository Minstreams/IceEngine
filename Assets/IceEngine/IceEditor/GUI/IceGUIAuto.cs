using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Rendering;

using IceEngine;
using IceEngine.Internal;
using IceEditor.Internal;
using static IceEditor.IceGUI;

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
        public static FolderScope SectionFolder(string key, bool defaultVal = true, string labelOverride = null, bool changeWidth = true)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            if (IceGUIUtility.HasPack)
            {
                var ab = GetAnimBool(key, defaultVal);
                ab.target = GUILayout.Toggle(ab.target, label, StlSectionHeader);
                return new FolderScope(ab, changeWidth);
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
        /// <summary>
        /// 用于将一个区域拆分为可调整大小的两个区域
        /// </summary>
        public static SubAreaScope SubArea(Rect rect, out Rect mainRect, out Rect subRect, string key, float defaultVal = 0, IceGUIDirection direction = IceGUIDirection.Right, GUIStyle separatorStyleOverride = null, float width = 4, float border = 2) => IceGUI.SubArea(rect, out mainRect, out subRect, GetFloat(key, defaultVal), val => SetFloat(key, val), direction, separatorStyleOverride, width, border);
        #endregion

        #region GUI Elements
        /// <summary>
        /// 画一个自适应Layout的Texture，自带控制项
        /// </summary>
        public static void TextureBoxComplex(Texture texture, bool? expand = null, bool? foldout = null, bool showMip = true, string infoOverride = null, GUIStyle bakgroundStyleOverride = null, params (string text, Action callback)[] customBtns)
        {
            var name = texture.name;
            ColorWriteMask cwMask = ColorWriteMask.All;

            using (Vertical(bakgroundStyleOverride ?? StlNode))
            {
                using (HORIZONTAL)
                {
                    if (foldout != null)
                    {
                        SectionHeader(name, foldout.Value);
                        Space();
                        GUILayout.Label(infoOverride ?? $"{(texture.width == texture.height ? $"{texture.width}" : $"{texture.width} : {texture.height}")} | {(texture is Texture2D t2 ? t2.format.ToString() : texture is RenderTexture rt ? rt.format.ToString() : texture.GetType().ToString())}");
                    }

                    if (expand == null) IceToggle($"Expanded {name}", false, "Exp", "Is this texture Expanded");
                    else SetBool($"Expanded {name}", expand.Value);
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

                    if (foldout == null)
                    {
                        GUILayout.Label(infoOverride ?? $"{(texture.width == texture.height ? $"{texture.width}" : $"{texture.width} : {texture.height}")} | {(texture is Texture2D t2 ? t2.format.ToString() : texture is RenderTexture rt ? rt.format.ToString() : texture.GetType().ToString())}");
                        Space();
                    }

                    if (showMip) IntSliderNoLabel($"Mip Level {name}", 0, 0, texture.mipmapCount - 1);
                    else SetInt($"Mip Level {name}", 0);

                    foreach (var (text, callback) in customBtns)
                    {
                        if (text != null && IceButton(text)) callback?.Invoke();
                    }
                }

                using (foldout != null ? Folder(name) : null) using (Horizontal(StlIce))
                {
                    bool expanded = GetBool($"Expanded {name}");
                    if (!expanded) Space();
                    var rect = GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height, expanded ? GUILayout.ExpandWidth(true) : GUILayout.MaxWidth(texture.width >> GetInt($"Mip Level {name}")));
                    if (!expanded) Space();
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (cwMask == ColorWriteMask.Alpha)
                        {
                            EditorGUI.DrawTextureAlpha(rect, texture, ScaleMode.ScaleToFit, 0, GetInt($"Mip Level {name}"));
                        }
                        else if ((cwMask & ColorWriteMask.Alpha) == 0)
                        {
                            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit, 0, GetInt($"Mip Level {name}"), cwMask);
                        }
                        else
                        {
                            EditorGUI.DrawTextureTransparent(rect, texture, ScaleMode.ScaleToFit, 0, GetInt($"Mip Level {name}"), cwMask);
                        }
                    }
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

        public static string TextFieldNoLabel(string key, string defaultVal = "", GUIStyle styleOverride = null, params GUILayoutOption[] options) => SetString(key, _TextField(GetString(key, defaultVal), styleOverride, options));
        public static string TextField(string key, string defaultVal = "", string labelOverride = null, GUIStyle styleOverride = null, params GUILayoutOption[] options) { using (ControlLabel(key, labelOverride)) return TextFieldNoLabel(key, defaultVal, styleOverride, options); }

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

        /// <summary>
        /// 一个可缩放可移动的工作视图
        /// </summary>
        /// <param name="key">临时变量的key</param>
        /// <param name="baseScreenRect">所在GUI空间的屏幕区域</param>
        /// <param name="workspace">工作区Rect</param>
        /// <param name="canvasSize">画布的尺寸</param>
        /// <param name="defaultScale">默认画布缩放值</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="useWidthOrHeightOfWorkspaceAsSize">为true时，取工作区的宽作为尺寸<br/>为false时，取工作的高作为尺寸<br/>为null时，取二者中较小值作为尺寸</param>
        /// <param name="useAbsoluteScale">为true时，缩放比例应用于像素<br/>为false时，缩放比例应用于画布</param>
        /// <param name="useLimitedOffset">是否限制视图偏移范围，使画布始终可见</param>
        /// <param name="gridColor">指定一个颜色，沿画布边缘对齐绘制一个网格</param>
        /// <param name="styleBackground">可为工作区设定一个背景样式</param>
        /// <param name="styleCanvas">可为画布设定一个背景样式</param>
        /// <returns></returns>
        public static ViewportScope Viewport(string key, Rect baseScreenRect, Rect workspace, float canvasSize, float defaultScale = 1, float minScale = 0.5f, float maxScale = 2.0f, bool? useWidthOrHeightOfWorkspaceAsSize = null, bool useAbsoluteScale = false, bool useLimitedOffset = true, Color? gridColor = null, GUIStyle styleBackground = null, GUIStyle styleCanvas = null, bool hasOutterClip = true)
        {
            string keyViewScale = $"{key}_ViewScale";
            string keyViewOffset = $"{key}_ViewOffset";

            float viewScale = GetFloat(keyViewScale, defaultScale);
            Vector2 viewOffset = GetVector2(keyViewOffset);

            var viewport = IceGUI.Viewport(baseScreenRect, workspace, canvasSize, ref viewScale, ref viewOffset, minScale, maxScale, useWidthOrHeightOfWorkspaceAsSize, useAbsoluteScale, useLimitedOffset, gridColor, styleBackground, styleCanvas, hasOutterClip);

            SetFloat(keyViewScale, viewScale);
            SetVector2(keyViewOffset, viewOffset);
            return viewport;
        }
        /// <summary>
        /// 一个可缩放可移动的Canvas视图
        /// </summary>
        /// <param name="key">临时变量的key</param>
        /// <param name="baseScreenRect">所在GUI空间的屏幕区域</param>
        /// <param name="workspace">工作区Rect</param>
        /// <param name="canvasSize">画布的尺寸</param>
        /// <param name="defaultScale">默认画布缩放值</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="useWidthOrHeightOfWorkspaceAsSize">为true时，取工作区的宽作为尺寸<br/>为false时，取工作的高作为尺寸<br/>为null时，取二者中较小值作为尺寸</param>
        /// <param name="useAbsoluteScale">为true时，缩放比例应用于像素<br/>为false时，缩放比例应用于画布</param>
        /// <param name="styleBackground">可为工作区设定一个背景样式</param>
        /// <param name="styleCanvas">可为画布设定一个背景样式</param>
        /// <returns></returns>
        public static ViewportScope ViewportCanvas(string key, Rect baseScreenRect, Rect workspace, float canvasSize, float defaultScale = 0.9f, float minScale = 0.5f, float maxScale = 2.0f, bool? useWidthOrHeightOfWorkspaceAsSize = null, bool useAbsoluteScale = false, GUIStyle styleBackground = null, GUIStyle styleCanvas = null, bool hasOutterClip = true) => Viewport(key, baseScreenRect, workspace, canvasSize, defaultScale, minScale, maxScale, useWidthOrHeightOfWorkspaceAsSize, useAbsoluteScale, true, null, styleBackground, styleCanvas, hasOutterClip);
        /// <summary>
        /// 一个可缩放可移动的Grid视图
        /// </summary>
        /// <param name="key">临时变量的key</param>
        /// <param name="baseScreenRect">所在GUI空间的屏幕区域</param>
        /// <param name="workspace">工作区Rect</param>
        /// <param name="gridSize">grid块的尺寸</param>
        /// <param name="defaultScale">默认画布缩放值</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="useWidthOrHeightOfWorkspaceAsSize">为true时，取工作区的宽作为尺寸<br/>为false时，取工作的高作为尺寸<br/>为null时，取二者中较小值作为尺寸</param>
        /// <param name="gridColor">指定一个颜色，沿画布边缘对齐绘制一个网格</param>
        /// <param name="styleBackground">可为工作区设定一个背景样式</param>
        /// <returns></returns>
        public static ViewportScope ViewportGrid(string key, Rect baseScreenRect, Rect workspace, float gridSize, float defaultScale = 1, float minScale = 0.4f, float maxScale = 4.0f, bool? useWidthOrHeightOfWorkspaceAsSize = null, Color? gridColor = null, GUIStyle styleBackground = null, bool hasOutterClip = true) => Viewport(key, baseScreenRect, workspace, gridSize, defaultScale, minScale, maxScale, useWidthOrHeightOfWorkspaceAsSize, true, false, gridColor, styleBackground, null, hasOutterClip);

        /// <summary>
        /// 一个Graph操作区
        /// </summary>
        /// <param name="baseScreenRect">所在GUI空间的屏幕区域</param>
        /// <param name="area">工作区Rect</param>
        /// <param name="g">graph对象</param>
        /// <param name="stlBackGround">背景样式</param>
        /// <param name="gridSize">grid块的尺寸</param>
        public static void GraphArea(Rect baseScreenRect, Rect area, IceGraph g, float gridSize = 32, float defaultScale = 1, float minScale = 0.4f, float maxScale = 4.0f, GUIStyle stlBackGround = null, bool hasOutterClip = true)
        {
            using var viewport = ViewportGrid("GraphView", baseScreenRect, area, gridSize, defaultScale, minScale, maxScale, null, IceGUIUtility.CurrentThemeColor * 0.5f, stlBackGround, hasOutterClip);

            foreach (var node in g.nodeList) node.GetDrawer().OnGUI(gridSize);

            int idDragPort = GetControlID();
            foreach (var node in g.nodeList)
            {
                foreach (var port in node.inports) OnGUI_Port(port);
                foreach (var port in node.outports) OnGUI_Port(port);
            }
            void OnGUI_Port(IceGraphPort port)
            {
                Vector2 pos = port.GetPos();

                // Line
                if (E.type == EventType.Repaint && port.IsConnected)
                {
                    var tagent = port.GetTangent();
                    foreach (var pt in port.targetPortList)
                    {
                        IceGraphUtility.DrawPortLine(pos, pt.GetPos(), tagent, port.Color, pt.Color);
                    }
                }

                if (!port.node.folded)
                {
                    Rect rPort = pos.ExpandToRect(IceGraphPort.PORT_RADIUS);
                    switch (E.type)
                    {
                        case EventType.MouseDown:
                            // Drag Control
                            if (GUIHotControl == 0 && E.button == 0 && rPort.Contains(E.mousePosition))
                            {
                                offsetCache = rPort.center;
                                GUIHotControl = idDragPort;
                                if (!port.isMultiple) port.DisconnectAll();
                                port.GetAvailablePorts();
                                E.Use();
                            }
                            break;
                        case EventType.MouseUp:
                            if (GUIHotControl == idDragPort && E.button == 0 && rPort.Contains(E.mousePosition) && IceGraphUtility.AvailablePorts.Contains(port))
                            {
                                IceGraphUtility.EditingPort.ConnectTo(port);
                            }
                            break;
                        case EventType.Repaint:
                            // Port
                            {
                                var c = port.Color;
                                if (GUIHotControl == idDragPort)
                                {
                                    // 拖拽状态
                                    if (IceGraphUtility.EditingPort == port)
                                    {
                                        // 被拖拽的 Port
                                        DiscSolid(0.15f, c);
                                    }
                                    else
                                    {
                                        // 其他 Port
                                        if (port.IsConnected)
                                        {
                                            // 连接状态
                                            DiscSolid(0.3f, c);
                                        }

                                        if (IceGraphUtility.AvailablePorts.Contains(port))
                                        {
                                            // 备选状态
                                            DiscWire(0.2f, c * 0.8f);
                                            DiscWire(0.8f, c * 0.7f);

                                            if (rPort.Contains(E.mousePosition))
                                            {
                                                // hover
                                                DiscWire(0.8f, IceGUIUtility.CurrentThemeColor);
                                            }
                                            else
                                            {
                                                // 未hover
                                                DiscWire(0.8f, IceGUIUtility.CurrentThemeColor * 0.7f);
                                            }
                                        }
                                        else
                                        {
                                            // 非备选直接隐藏
                                        }
                                    }
                                }
                                else
                                {
                                    if (port.IsConnected)
                                    {
                                        // 连接状态
                                        DiscSolid(0.3f, c);
                                    }
                                    else
                                    {
                                        // 普通
                                        DiscWire(0.3f, c * 0.8f);
                                    }

                                    if (rPort.Contains(E.mousePosition))
                                    {
                                        // hover
                                        DiscWire(0.6f, c);
                                    }
                                }


                                // 画 Port 内圈
                                void DiscWire(float radius, Color color)
                                {
                                    radius *= IceGraphPort.PORT_RADIUS;
                                    using (HandlesColor(color)) Handles.DrawWireDisc(pos, Vector3.forward, radius);
                                    // 柔化边缘
                                    color.a *= 0.4f;
                                    using (HandlesColor(color))
                                    {
                                        float off = 0.3f / GUI.matrix[0];
                                        Handles.DrawWireDisc(pos, Vector3.forward, radius + off);
                                        Handles.DrawWireDisc(pos, Vector3.forward, radius - off);
                                    }
                                }
                                void DiscSolid(float radius, Color color)
                                {
                                    radius *= IceGraphPort.PORT_RADIUS;
                                    using (HandlesColor(color)) Handles.DrawSolidDisc(pos, Vector3.forward, radius);
                                    // 柔化边缘
                                    color.a *= 0.4f;
                                    using (HandlesColor(color)) Handles.DrawWireDisc(pos, Vector3.forward, radius + 0.3f / GUI.matrix[0]);
                                }
                            }
                            break;
                    }

                    // Text
                    float wText = StlGraphPortName.CalcSize(new GUIContent(port.name)).x;
                    Rect rText = new Rect(port.isOutport ? rPort.xMax : rPort.x - wText, rPort.y, wText, IceGraphPort.PORT_SIZE);
                    GUI.Label(rText, new GUIContent(port.name, port.valueType?.Name ?? "void"), StlGraphPortName);
                    StyleBox(rText, StlGraphPortName, port.name);
                }
            }

            switch (E.type)
            {
                case EventType.MouseUp:
                    if (GUIHotControl == idDragPort && E.button == 0)
                    {
                        GUIHotControl = 0;
                        IceGraphUtility.ClearAvailablePorts();
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == idDragPort)
                    {
                        E.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIHotControl == idDragPort)
                    {
                        // 正在拖动
                        var pos = IceGraphUtility.EditingPort.GetPos();
                        var tagent = IceGraphUtility.EditingPort.GetTangent();
                        var color = g.GetPortColor(IceGraphUtility.EditingPort);
                        IceGraphUtility.DrawPortLine(pos, E.mousePosition, tagent, color, color);
                        IceGraphUtility.DrawPortLine(E.mousePosition, pos, -tagent, color, color);
                    }
                    break;
            }

            //GUI.Label(viewport.ClipRect, $"Scale:{GUI.matrix[0]}");
        }

        #endregion
    }
}
