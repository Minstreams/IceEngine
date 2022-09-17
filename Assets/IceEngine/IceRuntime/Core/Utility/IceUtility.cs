using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// 依赖Unity的核心工具箱 & 扩展方法
    /// </summary>
    public static class IceUtility
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
        public static GUIStyle Initialize(this GUIStyle style, Action<GUIStyle> action)
        {
            action?.Invoke(style);
            return style;
        }
        #endregion

        #region String
        /// <summary>
        /// 等价于 <c><![CDATA[<color=#]]><paramref name="color"/>><paramref name="self"/><![CDATA[</color>]]></c>
        /// </summary>
        /// <param name="self">原字符串</param>
        /// <param name="color">颜色</param>
        /// <returns>结果表达式</returns>
        public static string Color(this string self, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{self}</color>";
        /// <summary>
        /// 用于关键字筛选
        /// </summary>
        /// <param name="text">待筛选的string</param>
        /// <param name="filter">关键字</param>
        /// <param name="highlightColor">设定高亮颜色</param>
        /// <param name="highlightedText">高亮后的名字</param>
        /// <param name="useRegex">使用正则表达式</param>
        /// <param name="continuousMatching">连续匹配</param>
        /// <param name="caseSensitive">区分大小写</param>
        /// <returns></returns>
        public static bool IsMatch(this string text, string filter, Color highlightColor, out string highlightedText, bool useRegex = false, bool continuousMatching = false, bool caseSensitive = false)
        {
            highlightedText = null;
            if (filter.IsNullOrWhiteSpace())
            {
                highlightedText = text;
                return true;
            }

            if (useRegex)
            {
                // 正则表达式匹配
                try
                {
                    var option = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    if (Regex.IsMatch(text, filter, option))
                    {
                        highlightedText = Regex.Replace(text, filter, "$0".Color(highlightColor), option);
                        return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                // 判断是否包含filter关键字
                if (continuousMatching)
                {
                    // 连续匹配
                    int index = text.IndexOf(filter, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                    if (index < 0) return false;

                    // 替换关键字为指定颜色
                    highlightedText = text
                        .Insert(index + filter.Length, "</color>")
                        .Insert(index, $"<color={highlightColor}>");

                    return true;
                }
                else
                {
                    int l = filter.Length;
                    {
                        // 离散匹配
                        int i = 0;
                        foreach (char c in text) if (c.CompareChar(filter[i], caseSensitive) && ++i == l) break;
                        if (i < l) return false;
                    }

                    highlightedText = string.Empty;
                    {
                        // 替换关键字为指定颜色
                        int i = 0;
                        foreach (char c in text)
                        {
                            if (i < l && c.CompareChar(filter[i], caseSensitive))
                            {
                                highlightedText += c.ToString().Color(highlightColor);
                                ++i;
                            }
                            else highlightedText += c;
                        }
                    }
                    return true;
                }
            }
        }
        #endregion

        #region Rect
        static float InverseLerpUnclamped(float a, float b, float value) => a == b ? 0 : (value - a) / (b - a);

        /// <summary>
        /// 给Rect应用一个指定宽度的边框
        /// </summary>
        public static Rect ApplyBorder(this Rect self, float width) => Rect.MinMaxRect(self.xMin - width, self.yMin - width, self.xMax + width, self.yMax + width);
        /// <summary>
        /// 给Rect应用一个指定xy两个轴向宽度的边框
        /// </summary>
        public static Rect ApplyBorder(this Rect self, float widthX, float widthY) => Rect.MinMaxRect(self.xMin - widthX, self.yMin - widthY, self.xMax + widthX, self.yMax + widthY);
        /// <summary>
        /// 移动边缘
        /// </summary>
        public static Rect MoveEdge(this Rect self, float left = 0, float right = 0, float top = 0, float bottom = 0) => Rect.MinMaxRect(self.xMin + left, self.yMin + top, self.xMax + right, self.yMax + bottom);
        /// <summary>
        /// 移动
        /// </summary>
        public static Rect Move(this Rect self, float x = 0, float y = 0) => new Rect(self.x + x, self.y + y, self.width, self.height);
        /// <summary>
        /// 移动
        /// </summary>
        public static Rect Move(this Rect self, Vector2 offset) => new Rect(self.x + offset.x, self.y + offset.y, self.width, self.height);

        /// <summary>
        /// 采样
        /// </summary>
        public static Vector2 Sample(this Rect self, Vector2 uv, bool inverseY = false) => new Vector2(Mathf.LerpUnclamped(self.xMin, self.xMax, uv.x), Mathf.LerpUnclamped(self.yMin, self.yMax, inverseY ? 1 - uv.y : uv.y));
        /// <summary>
        /// 反采样
        /// </summary>
        public static Vector2 InverseSample(this Rect self, Vector2 pos, bool inverseY = false) => new Vector2(InverseLerpUnclamped(self.xMin, self.xMax, pos.x), inverseY ? 1 - InverseLerpUnclamped(self.yMin, self.yMax, pos.y) : InverseLerpUnclamped(self.yMin, self.yMax, pos.y));
        /// <summary>
        /// 根据一个viewport取子Rect
        /// </summary>
        public static Rect Viewport(this Rect self, Rect viewport, bool inverseY = false) => Rect.MinMaxRect(Mathf.LerpUnclamped(self.xMin, self.xMax, viewport.xMin), Mathf.LerpUnclamped(self.yMin, self.yMax, inverseY ? 1 - viewport.yMax : viewport.yMin), Mathf.LerpUnclamped(self.xMin, self.xMax, viewport.xMax), Mathf.LerpUnclamped(self.yMin, self.yMax, inverseY ? 1 - viewport.yMin : viewport.yMax));
        /// <summary>
        /// 根据一个子Rect取viewport
        /// </summary>
        public static Rect InverseViewport(this Rect self, Rect subRect, bool inverseY = false) => Rect.MinMaxRect(InverseLerpUnclamped(self.xMin, self.xMax, subRect.xMin), inverseY ? 1 - InverseLerpUnclamped(self.yMin, self.yMax, subRect.yMax) : InverseLerpUnclamped(self.yMin, self.yMax, subRect.yMin), InverseLerpUnclamped(self.xMin, self.xMax, subRect.xMax), inverseY ? 1 - InverseLerpUnclamped(self.yMin, self.yMax, subRect.yMin) : InverseLerpUnclamped(self.yMin, self.yMax, subRect.yMax));
        /// <summary>
        /// 是否包含另一个Rect
        /// </summary>
        public static bool Contains(this Rect self, Rect other) => self.xMin <= other.xMin && self.xMax >= other.xMax && self.yMin <= other.yMin && self.yMax >= other.yMax;
        #endregion

        #region Vector2
        public static Rect RangeRect(this Vector2 from, Vector2 to) => Rect.MinMaxRect(Mathf.Min(from.x, to.x), Mathf.Min(from.y, to.y), Mathf.Max(from.x, to.x), Mathf.Max(from.y, to.y));
        #endregion

        #region Render Texture
        /// <summary>
        /// 新建一个Texture2D并将此RT拷贝上去
        /// </summary>
        public static Texture2D ReadPixels(this RenderTexture self)
        {
            var format = self.format.ToTextureFormat();
            var tex = new Texture2D(self.width, self.height, format, self.useMipMap, !self.sRGB);

            RenderTexture.active = self;
            tex.ReadPixels(new Rect(0, 0, self.width, self.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            return tex;
        }
        /// <summary>
        /// 新建一个Texture2D并将此RT拷贝上去
        /// </summary>
        public static Texture2D ReadPixels(this RenderTexture self, TextureFormat textureFormat, bool mipChain = true, bool linear = false)
        {
            var tex = new Texture2D(self.width, self.height, textureFormat, mipChain, linear);

            RenderTexture.active = self;
            tex.ReadPixels(new Rect(0, 0, self.width, self.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            return tex;
        }
        /// <summary>
        /// 将此RT拷贝到一个Texture2D上
        /// </summary>
        public static void ReadPixelsTo(this RenderTexture self, Texture2D tex)
        {
            RenderTexture.active = self;
            tex.ReadPixels(new Rect(0, 0, self.width, self.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
        }

        #endregion

        #region Other
        public static TextureFormat ToTextureFormat(this RenderTextureFormat source)
        {
            return source switch
            {
                RenderTextureFormat.ARGB32 => TextureFormat.ARGB32,
                RenderTextureFormat.Depth => TextureFormat.RFloat,
                RenderTextureFormat.ARGBHalf => TextureFormat.ARGB32,
                RenderTextureFormat.Shadowmap => TextureFormat.RFloat,
                RenderTextureFormat.RGB565 => TextureFormat.RGB565,
                RenderTextureFormat.ARGB4444 => TextureFormat.ARGB4444,
                RenderTextureFormat.ARGB1555 => TextureFormat.ARGB32,
                RenderTextureFormat.Default => TextureFormat.ARGB32,
                RenderTextureFormat.ARGB2101010 => TextureFormat.ARGB32,
                RenderTextureFormat.DefaultHDR => TextureFormat.RGBAFloat,
                RenderTextureFormat.ARGB64 => TextureFormat.ARGB32,
                RenderTextureFormat.ARGBFloat => TextureFormat.RGBAFloat,
                RenderTextureFormat.RGFloat => TextureFormat.RGFloat,
                RenderTextureFormat.RGHalf => TextureFormat.RGHalf,
                RenderTextureFormat.RFloat => TextureFormat.RFloat,
                RenderTextureFormat.RHalf => TextureFormat.RHalf,
                RenderTextureFormat.R8 => TextureFormat.R8,
                RenderTextureFormat.ARGBInt => TextureFormat.ARGB32,
                RenderTextureFormat.RGInt => TextureFormat.RG32,
                RenderTextureFormat.RInt => TextureFormat.R16,
                RenderTextureFormat.BGRA32 => TextureFormat.BGRA32,
                RenderTextureFormat.RGB111110Float => TextureFormat.RGB9e5Float,
                RenderTextureFormat.RG32 => TextureFormat.RG32,
                RenderTextureFormat.RGBAUShort => TextureFormat.RGBA64,
                RenderTextureFormat.RG16 => TextureFormat.RG16,
                RenderTextureFormat.BGRA10101010_XR => TextureFormat.BGRA32,
                RenderTextureFormat.BGR101010_XR => TextureFormat.BGRA32,
                RenderTextureFormat.R16 => TextureFormat.R16,
                _ => TextureFormat.ARGB32,
            };
        }
        public static RenderTextureFormat ToRenderTextureFormat(this TextureFormat source)
        {
            return source switch
            {
                TextureFormat.ARGB32 => RenderTextureFormat.ARGB32,
                TextureFormat.R8 => RenderTextureFormat.R8,
                TextureFormat.R16 => RenderTextureFormat.R16,
                TextureFormat.RFloat => RenderTextureFormat.RFloat,
                TextureFormat.RG16 => RenderTextureFormat.RG16,
                TextureFormat.RG32 => RenderTextureFormat.RG32,
                TextureFormat.BGRA32 => RenderTextureFormat.BGRA32,
                TextureFormat.RGBA64 => RenderTextureFormat.RGBAUShort,
                TextureFormat.ARGB4444 => RenderTextureFormat.ARGB4444,
                TextureFormat.RGBA4444 => RenderTextureFormat.ARGB4444,
                _ => RenderTextureFormat.ARGB32,
            };
        }
        #endregion

        #region Vector
        /// <summary>
        /// 沿某刻度对齐
        /// </summary>
        /// <param name="snap">刻度</param>
        public static Vector2 Snap(this Vector2 self, float snap = 1) => new Vector2(Mathf.Round(self.x / snap) * snap, Mathf.Round(self.y / snap) * snap);
        /// <summary>
        /// 从中心点向外扩展成Rect
        /// </summary>
        public static Rect ExpandToRect(this Vector2 center, float radius) => new Rect(center.x - radius, center.y - radius, radius + radius, radius + radius);
        /// <summary>
        /// 从中心点向外扩展成Rect
        /// </summary>
        public static Rect ExpandToRect(this Vector2 center, Vector2 halfSize) => new Rect(center - halfSize, halfSize + halfSize);
        #endregion

        #region MonoBehaviour
        /// <summary>
        /// 组件在Hierarchy中的路径
        /// </summary>
        /// <param name="component">组件</param>
        public static string GetPath(this MonoBehaviour component)
        {
            string path = component.ToString();
            var trans = component.transform.parent;
            while (trans != null)
            {
                path = trans.name + "/" + path;
                trans = trans.parent;
            }
            return path;
        }
        #endregion
    }
}