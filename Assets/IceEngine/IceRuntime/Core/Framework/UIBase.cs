﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.DebugUI
{
    /// <summary>
    /// Base class of a debug ui. Need a UI displayer to display
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        protected void SetStyle(ref GUIStyle target, string styleName)
        {
#if UNITY_EDITOR
            target = new GUIStyle(UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Game).GetStyle(styleName));
#endif
        }
        public GUIStyle boxStyle;
        protected virtual void Reset() => SetStyle(ref boxStyle, "box");

        UIDisplayer displayer;
        protected virtual void OnEnable()
        {
            displayer = GetComponentInParent<UIDisplayer>();
            if (displayer != null) displayer.UIAction += UIAction;
            else this.enabled = false;
        }
        protected virtual void OnDisable()
        {
            if (displayer != null) displayer.UIAction -= UIAction;
        }
        public void UIAction()
        {
            GUILayout.BeginVertical(boxStyle, GUILayout.ExpandWidth(false));
            OnUI();
            GUILayout.EndVertical();
        }
        protected abstract void OnUI();


        #region Scope
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
        /// 指定一个ScrollArea
        /// </summary>
        public class ScrollScope : IDisposable
        {
            public Vector2 scrollPosition;
            public ScrollScope(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options)
            {
                this.scrollPosition = GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background, options);
            }

            void IDisposable.Dispose()
            {
                GUILayout.EndScrollView();
            }
        }

        protected static GUILayout.HorizontalScope HORIZONTAL => Horizontal();
        protected static GUILayout.VerticalScope VERTICAL => Vertical();
        protected static ChangeCheckScope GUICHECK => new ChangeCheckScope();
        protected static bool GUIChanged => GUI.changed;
        protected GUILayout.VerticalScope BOX => Vertical(boxStyle);
        protected static GUILayout.HorizontalScope Horizontal(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.HorizontalScope(style ?? GUIStyle.none, options);
        protected static GUILayout.HorizontalScope Horizontal(params GUILayoutOption[] options) => new GUILayout.HorizontalScope(options);
        protected static GUILayout.VerticalScope Vertical(GUIStyle style, params GUILayoutOption[] options) => new GUILayout.VerticalScope(style ?? GUIStyle.none, options);
        protected static GUILayout.VerticalScope Vertical(params GUILayoutOption[] options) => new GUILayout.VerticalScope(options);

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected ScrollScope Scroll(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, "horizontalscrollbar", "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个竖直Scroll View
        /// </summary>
        protected ScrollScope ScrollVertical(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, GUIStyle.none, "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个水平Scroll View
        /// </summary>
        protected ScrollScope ScrollHorizontal(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, "horizontalscrollbar", GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个隐藏bar的Scroll View
        /// </summary>
        protected ScrollScope ScrollInvisible(int key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, GUIStyle.none, GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }

        /// <summary>
        /// 在Using语句中使用的Scope，指定一个Scroll View
        /// </summary>
        protected ScrollScope Scroll(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, "horizontalscrollbar", "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个竖直Scroll View
        /// </summary>
        protected ScrollScope ScrollVertical(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, GUIStyle.none, "verticalscrollbar", style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个水平Scroll View
        /// </summary>
        protected ScrollScope ScrollHorizontal(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, "horizontalscrollbar", GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        /// <summary>
        /// 在Using语句中使用的Scope，指定一个隐藏bar的Scroll View
        /// </summary>
        protected ScrollScope ScrollInvisible(string key, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var res = new ScrollScope(GetVector2(key), false, false, GUIStyle.none, GUIStyle.none, style ?? GUIStyle.none, options);
            SetVector2(key, res.scrollPosition);
            return res;
        }
        #endregion

        #region GUI Elements
        static GUIStyle DefaultLabelStyle => "label";
        /// <summary>
        /// Calculate the width of given content if rendered with default label style. Return the "width" layout option object.
        /// </summary>
        protected GUILayoutOption AutoWidth(string label) => GUILayout.Width(DefaultLabelStyle.CalcSize(new GUIContent(label + "\t")).x);

        protected static void Space(float pixels) => GUILayout.Space(pixels);
        protected static void Space() => GUILayout.FlexibleSpace();

        protected static void Label(string text, GUIStyle style, params GUILayoutOption[] options) => GUILayout.Label(text, style, options);
        protected static void Label(string text, params GUILayoutOption[] options) => Label(text, DefaultLabelStyle, options);
        protected void Title(string text) => Label(text, boxStyle, GUILayout.ExpandWidth(true));

        protected bool Button(string text, bool expandWidth = false) => GUILayout.Button(text, GUILayout.ExpandWidth(expandWidth));

        protected bool _Toggle(string label, bool value) => GUILayout.Toggle(value, label);
        protected bool Toggle(string label, ref bool value) => value = _Toggle(label, value);
        protected bool Toggle(string key, bool defaultValue = false, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetBool(key, defaultValue);

            return SetBool(key, _Toggle(label, value));
        }

        protected bool _ToggleButton(string label, bool value, bool expandWidth = false)
        {
            if (GUILayout.Button(label.Color(value ? Color.white : Color.gray), GUILayout.ExpandWidth(expandWidth))) return !value;
            return value;
        }
        protected bool ToggleButton(string label, ref bool value, bool expandWidth = false) => value = _ToggleButton(label, value, expandWidth);
        protected bool ToggleButton(string key, bool defaultValue = false, string labelOverride = null, bool expandWidth = false)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetBool(key, defaultValue);

            return SetBool(key, _ToggleButton(label, value, expandWidth));
        }

        protected string _TextField(string label, string value)
        {
            using var _ = HORIZONTAL;
            Label(label, AutoWidth(label));
            return GUILayout.TextField(value);
        }
        protected string TextField(string label, ref string value) => value = _TextField(label, value);
        protected string TextField(string key, string defaultValue = null, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetString(key, defaultValue);

            return SetString(key, _TextField(label, value));
        }

        protected int _IntField(string label, int value)
        {
            using var _ = HORIZONTAL;
            Label(label, AutoWidth(label));
            int res;
            try
            {
                res = int.Parse(GUILayout.TextField(value.ToString()));
                return res;
            }
            catch
            {
                return value;
            }
        }
        protected int IntField(string label, ref int value) => value = _IntField(label, value);
        protected int IntField(string key, int defaultValue = 0, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetInt(key, defaultValue);

            return SetInt(key, _IntField(label, value));
        }

        protected int _SliderInt(string label, int value, int min = 0, int max = 1)
        {
            using var _ = HORIZONTAL;
            Label(label, AutoWidth(label));
            try
            {
                int res = (int)GUILayout.HorizontalSlider(value, min, max);
                return res;
            }
            catch
            {
                return value;
            }
        }
        protected int SliderInt(string label, ref int value, int min = 0, int max = 1) => value = _SliderInt(label, value, min, max);
        protected int SliderInt(string key, int defaultValue = 0, int min = 0, int max = 1, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetInt(key, defaultValue);

            return SetInt(key, SliderInt(label, value, min, max));
        }

        protected float _FloatField(string label, float value)
        {
            using var _ = HORIZONTAL;
            Label(label, AutoWidth(label));
            try
            {
                float res = float.Parse(GUILayout.TextField(value.ToString()));
                return res;
            }
            catch
            {
                return value;
            }
        }
        protected float FloatField(string label, ref float value) => value = _FloatField(label, value);
        protected float FloatField(string key, float defaultValue = 0, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetFloat(key, defaultValue);

            return SetFloat(key, _FloatField(label, value));
        }

        protected float _Slider(string label, float value, float min = 0, float max = 1)
        {
            using var _ = HORIZONTAL;
            Label(label, AutoWidth(label));
            try
            {
                float res = GUILayout.HorizontalSlider(value, min, max);
                return res;
            }
            catch
            {
                return value;
            }
        }
        protected float Slider(string label, ref float value, float min = 0, float max = 1) => value = _Slider(label, value, min, max);
        protected float Slider(string key, float defaultValue = 0, float min = 0, float max = 1, string labelOverride = null)
        {
            var label = string.IsNullOrEmpty(labelOverride) ? key : labelOverride;
            var value = GetFloat(key, defaultValue);

            return SetFloat(key, _Slider(label, value, min, max));
        }

        #endregion

        #region 临时数据托管
        internal Dictionary<string, bool> _stringBoolMap = new();
        public bool GetBool(string key, bool defaultVal = false) => _stringBoolMap.TryGetValue(key, out bool res) ? res : _stringBoolMap[key] = defaultVal;
        public bool SetBool(string key, bool value) => _stringBoolMap[key] = value;


        internal Dictionary<string, int> _stringIntMap = new();
        public int GetInt(string key, int defaultVal = 0) => _stringIntMap.TryGetValue(key, out int res) ? res : _stringIntMap[key] = defaultVal;
        public int SetInt(string key, int value) => _stringIntMap[key] = value;


        internal Dictionary<string, float> _stringFloatMap = new();
        public float GetFloat(string key, float defaultVal = 0) => _stringFloatMap.TryGetValue(key, out float res) ? res : _stringFloatMap[key] = defaultVal;
        public float SetFloat(string key, float value) => _stringFloatMap[key] = value;


        internal Dictionary<string, string> _stringStringMap = new();
        public string GetString(string key, string defaultVal = "") => _stringStringMap.TryGetValue(key, out string res) ? res : _stringStringMap[key] = defaultVal;
        public string SetString(string key, string value) => _stringStringMap[key] = value;


        internal Dictionary<int, Vector2> _intVec2Map = new();
        public Vector2 GetVector2(int key, Vector2 defaultVal = default) => _intVec2Map.TryGetValue(key, out Vector2 res) ? res : _intVec2Map[key] = defaultVal;
        public Vector2 SetVector2(int key, Vector2 value) => _intVec2Map[key] = value;


        internal Dictionary<string, Vector2> _stringVec2Map = new();
        public Vector2 GetVector2(string key, Vector2 defaultVal = default) => _stringVec2Map.TryGetValue(key, out Vector2 res) ? res : _stringVec2Map[key] = defaultVal;
        public Vector2 SetVector2(string key, Vector2 value) => _stringVec2Map[key] = value;


        internal Dictionary<string, Vector3> _stringVec3Map = new();
        public Vector3 GetVector3(string key, Vector3 defaultVal = default) => _stringVec3Map.TryGetValue(key, out Vector3 res) ? res : _stringVec3Map[key] = defaultVal;
        public Vector3 SetVector3(string key, Vector3 value) => _stringVec3Map[key] = value;


        [SerializeField] internal Dictionary<string, Vector4> _stringVec4Map = new();
        public Vector4 GetVector4(string key, Vector4 defaultVal = default) => _stringVec4Map.TryGetValue(key, out Vector4 res) ? res : _stringVec4Map[key] = defaultVal;
        public Vector4 SetVector4(string key, Vector4 value) => _stringVec4Map[key] = value;


        internal Dictionary<string, Vector2Int> _stringVec2IntMap = new();
        public Vector2Int GetVector2Int(string key, Vector2Int defaultVal = default) => _stringVec2IntMap.TryGetValue(key, out Vector2Int res) ? res : _stringVec2IntMap[key] = defaultVal;
        public Vector2Int SetVector2Int(string key, Vector2Int value) => _stringVec2IntMap[key] = value;


        internal Dictionary<string, Vector3Int> _stringVec3IntMap = new();
        public Vector3Int GetVector3Int(string key, Vector3Int defaultVal = default) => _stringVec3IntMap.TryGetValue(key, out Vector3Int res) ? res : _stringVec3IntMap[key] = defaultVal;
        public Vector3Int SetVector3Int(string key, Vector3Int value) => _stringVec3IntMap[key] = value;
        #endregion
    }
}
