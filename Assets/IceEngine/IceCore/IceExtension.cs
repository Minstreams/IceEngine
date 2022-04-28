using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这应该是一个项目，功能分为若干文件

namespace IceEngine
{
    /// <summary>
    /// 各系统依赖的核心扩展方法
    /// </summary>
    public static class IceExtension
    {
        #region GUIStyle
        /// <summary>
        /// 确保一个GUIStyle是带有名字的有效样式
        /// </summary>
        public static GUIStyle Check(this GUIStyle style) => string.IsNullOrEmpty(style.name) ? null : style;
        /// <summary>
        /// 用于链式初始化GUIStyle
        /// </summary>
        /// <param name="action">初始化行为</param>
        /// <returns>初始化后的自身</returns>
        public static GUIStyle Initialize(this GUIStyle style, System.Action<GUIStyle> action)
        {
            action?.Invoke(style);
            return style;
        }
        #endregion

        #region String
        /// <summary>
        /// 等价于&lt;color=<paramref name="colorExp"/>&gt;<paramref name="self"/>&lt;/color&gt;
        /// </summary>
        /// <param name="self">原字符串</param>
        /// <param name="colorExp">颜色表达式</param>
        /// <returns>结果表达式</returns>
        public static string Color(this string self, string colorExp) => $"<color={colorExp}>{self}</color>";
        /// <summary>
        /// 等价于&lt;color=#<paramref name="color"/>&gt;<paramref name="self"/>&lt;/color&gt;
        /// </summary>
        /// <param name="self">原字符串</param>
        /// <param name="color">颜色</param>
        /// <returns>结果表达式</returns>
        public static string Color(this string self, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{self}</color>";
        /// <summary>
        /// 等价于&lt;b&gt;<paramref name="self"/>&lt;/b&gt;
        /// </summary>
        /// <returns>结果表达式</returns>
        public static string Bold(this string self) => $"<b>{self}</b>";
        /// <summary>
        /// 等价于&lt;size=<paramref name="size"/>&gt;<paramref name="self"/>&lt;/size&gt;
        /// </summary>
        /// <returns>结果表达式</returns>
        public static string Size(this string self, int size) => $"<size={size}>{self}</size>";
        #endregion

        #region Rect
        /// <summary>
        /// 给Rect应用一个指定宽度的边框
        /// </summary>
        public static Rect ApplyBorder(this Rect self, float width) => new Rect(self.x - width, self.y - width, self.width + width + width, self.height + width + width);
        /// <summary>
        /// 给Rect应用一个指定xy两个轴向宽度的边框
        /// </summary>
        public static Rect ApplyBorder(this Rect self, float widthX, float widthY) => new Rect(self.x - widthX, self.y - widthY, self.width + widthX + widthX, self.height + widthY + widthY);
        /// <summary>
        /// 移动边缘
        /// </summary>
        public static Rect MoveEdge(this Rect self, float left, float right, float top, float bottom) => new Rect(self.x + left, self.y + top, self.width + right - left, self.height + bottom - top);
        /// <summary>
        /// 移动
        /// </summary>
        public static Rect Move(this Rect self, float x, float y) => new Rect(self.x + x, self.y + y, self.width, self.height);
        #endregion
    }
}