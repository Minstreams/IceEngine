using System;
using System.Collections.Generic;

namespace IceEngine
{
    /// <summary>
    /// 不依赖Unity的核心工具箱 & 扩展方法
    /// </summary>
    public static partial class IceCoreUtility
    {
        #region Extensions

        #region String
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);
        /// <summary>
        /// 等价于 <c><![CDATA[<color=]]><paramref name="colorExp"/>><paramref name="self"/><![CDATA[</color>]]></c>
        /// </summary>
        /// <param name="self">原字符串</param>
        /// <param name="colorExp">颜色表达式</param>
        /// <returns>结果表达式</returns>
        public static string Color(this string self, string colorExp) => $"<color={colorExp}>{self}</color>";
        /// <summary>
        /// 等价于 <c><![CDATA[<b>]]><paramref name="self"/><![CDATA[</b>]]></c>
        /// </summary>
        /// <returns>结果表达式</returns>
        public static string Bold(this string self) => $"<b>{self}</b>";
        /// <summary>
        /// 等价于 <c><![CDATA[<size=]]><paramref name="size"/>><paramref name="self"/><![CDATA[</size>]]></c>
        /// </summary>
        /// <returns>结果表达式</returns>
        public static string Size(this string self, int size) => $"<size={size}>{self}</size>";
        #endregion

        #region Byte
        /// <summary>
        /// 获取字节中的指定Bit的值
        /// </summary>
        /// <param name="self">字节</param>
        /// <param name="index">Bit的索引值(0-7)</param>
        /// <returns></returns>
        public static int GetBit(this byte self, short index) => (self >> index) & 1;
        #endregion

        #region Type

        static Dictionary<string, Type> typeCacheMap = new();
        public static Type GetType(string typeFullName)
        {
            if (typeCacheMap.TryGetValue(typeFullName, out var type)) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeFullName);
                if (type != null)
                {
                    return typeCacheMap[typeFullName] = type;
                }
            }
            return null;
        }
        public static Type GetRootType(this Type self)
        {
            var end = typeof(object);
            while (self != end)
            {
                var b = self.BaseType;
                if (b == end) break;
                self = b;
            }
            return self;
        }
        #endregion

        #endregion
    }
}
