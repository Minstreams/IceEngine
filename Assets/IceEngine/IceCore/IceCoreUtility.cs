using System;
using System.Collections;
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

        #region Cache
        static readonly Dictionary<string, Type> _typeCacheMap = new();
        static readonly Type objectType = typeof(object);
        static readonly Type nullableType = typeof(Nullable<>);
        static readonly Type iCollectionType = typeof(ICollection);
        static readonly Type icePacketType = typeof(IcePacketAttribute);

        static Dictionary<int, Type> _hash2PktMap = null;
        static Dictionary<Type, int> _pkt2HashMap = null;

        static (Dictionary<int, Type> h2p, Dictionary<Type, int> p2h) CollectAllTypes()
        {
            _hash2PktMap = new();
            _pkt2HashMap = new();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var ts = a.GetTypes();
                foreach (var t in ts)
                {
                    var packetAttributes = t.GetCustomAttributes(icePacketType, false);
                    if (packetAttributes.Length > 0)
                    {
                        var attr = (IcePacketAttribute)packetAttributes[0];
                        int hash = attr.Hashcode;
                        if (hash == 0) hash = t.FullName.GetHashCode();

                        if (_hash2PktMap.TryAdd(hash, t))
                        {
                            _pkt2HashMap.Add(t, hash);
                        }
                        else
                        {
                            throw new Exception($"Packet hash ({hash}) of [{t}] already exists in [{_hash2PktMap[hash]}]");
                        }
                    }
                }
            }
            return (_hash2PktMap, _pkt2HashMap);
        }

        static Dictionary<int, Type> Hash2PktMap => _hash2PktMap ?? CollectAllTypes().h2p;
        static Dictionary<Type, int> Pkt2HashMap = _pkt2HashMap ?? CollectAllTypes().p2h;
        #endregion

        public static Type GetType(string typeFullName)
        {
            if (_typeCacheMap.TryGetValue(typeFullName, out var type)) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeFullName);
                if (type != null)
                {
                    return _typeCacheMap[typeFullName] = type;
                }
            }
            return null;
        }
        public static Type GetRootType(this Type self)
        {
            while (self != objectType)
            {
                var b = self.BaseType;
                if (b == objectType) break;
                self = b;
            }
            return self;
        }

        public static bool IsNullable(this Type type) => (type.IsGenericType && type.GetGenericTypeDefinition().Equals(nullableType));
        public static bool IsCollection(this Type type) => iCollectionType.IsAssignableFrom(type);
        public static Type HashCodeToPacketType(int hash)
        {
            if (Hash2PktMap.TryGetValue(hash, out var type)) return type;
            throw new Exception($"{hash} is not a packet hash!");
        }
        public static int PacketTypeToHashCode(Type type)
        {
            if (Pkt2HashMap.TryGetValue(type, out int hash)) return hash;
            throw new Exception($"{type} is not a packet type!");
        }
        public static bool IsPacketType(this Type type) => Pkt2HashMap.ContainsKey(type);
        #endregion

        #endregion
    }
}
