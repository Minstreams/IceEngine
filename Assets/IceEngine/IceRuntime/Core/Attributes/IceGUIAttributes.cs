using System;
using UnityEngine;

namespace IceEngine
{
    #region Class
    /// <summary>
    /// 标记使用自定义Drawer
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class HasPropertyDrawerAttribute : Attribute { }
    /// <summary>
    /// 自定义主题颜色
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ThemeColorAttribute : Attribute
    {
        public Color Color { get; private set; }
        public ThemeColorAttribute(float r, float g, float b) => Color = new Color(r, g, b);
    }
    /// <summary>
    /// 自定义标签宽度
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class LabelWidthAttribute : Attribute
    {
        public float Width { get; private set; }
        public LabelWidthAttribute(float width) => Width = width;
    }
    #endregion

    #region Field
    /// <summary>
    /// 字段绘制时代替标签，或者给一个类加一个Title
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class LabelAttribute : Attribute
    {
        public string Label { get; private set; }
        public LabelAttribute(string label) => Label = label;
    }

    /// <summary>
    /// 字段或IceprintNode运行时不可更改
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RuntimeConstAttribute : Attribute { }
    /// <summary>
    /// 标记一个Group
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class GroupAttribute : Attribute
    {
        public string Label { get; private set; }
        public GroupAttribute(string label = null) => Label = label;
    }
    #endregion

    #region Method
    /// <summary>
    /// 给方法在面板上添加按钮
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ButtonAttribute : Attribute
    {
        public string Label { get; set; } = null;

        public ButtonAttribute() { }
        public ButtonAttribute(string label) => Label = label;
    }
    #endregion
}