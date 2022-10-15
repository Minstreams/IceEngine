using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IceEngine.Framework;

namespace IceEngine
{
    /// <summary>
    /// 不依赖Unity的核心工具箱 & 扩展方法
    /// </summary>
    public static partial class IceCoreUtility
    {
        #region Extensions

        #region Char
        /// <summary>
        /// 比较两个char的值，并指定是否区分大小写
        /// </summary>
        /// <param name="caseSensitive">是否区分大小写</param>
        public static bool CompareChar(this char self, char other, bool caseSensitive = true)
        {
            if (caseSensitive) return self == other;
            else return char.ToLower(self) == char.ToLower(other);
        }
        #endregion

        #region String
        public readonly static Encoding DefaultEncoding = Encoding.UTF8;
        public static byte[] GetBytes(this string str) => DefaultEncoding.GetBytes(str);
        public static string GetString(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static string GetString(this byte[] bytes, int index, int count) => Encoding.UTF8.GetString(bytes, index, count);
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
        /// <summary>
        /// 用于关键字筛选
        /// </summary>
        /// <param name="text">待筛选的string</param>
        /// <param name="filter">关键字</param>
        /// <param name="useRegex">使用正则表达式</param>
        /// <param name="continuousMatching">连续匹配</param>
        /// <param name="caseSensitive">区分大小写</param>
        /// <returns></returns>
        public static bool IsMatch(this string text, string filter, bool useRegex = false, bool continuousMatching = false, bool caseSensitive = false)
        {
            if (filter.IsNullOrWhiteSpace()) return true;

            if (useRegex)
            {
                // 正则表达式匹配
                try
                {
                    var option = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    return Regex.IsMatch(text, filter, option);
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
                    return text.Contains(filter, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    int l = filter.Length;
                    // 离散匹配
                    int i = 0;
                    foreach (char c in text) if (c.CompareChar(filter[i], caseSensitive) && ++i == l) break;
                    // 不包含则跳过
                    return i == l;
                }
            }
        }
        /// <summary>
        /// 尝试创建指定路径文件夹
        /// </summary>
        public static bool TryCreateFolder(this string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 尝试创建指定路径文件夹
        /// </summary>
        public static bool TryCreateFolderOfPath(this string path) => TryCreateFolder(Path.GetDirectoryName(path));
        #endregion

        #region Byte
        /// <summary>
        /// 获取字节中的指定Bit的值
        /// </summary>
        /// <param name="self">字节</param>
        /// <param name="index">Bit的索引值(0-7)</param>
        /// <returns></returns>
        public static int GetBit(this byte self, short index) => (self >> index) & 1;
        public static string Hex(this IList<byte> buffer, int start, int count)
        {
            string res = "";
            for (int i = start; i < start + count; ++i)
            {
                var b = buffer[i];
                res += $"{b:X2}";
                if (i < buffer.Count - 1)
                {
                    if (((i + 1) & 3) == 0) res += " |";
                    res += " ";
                }
            }
            return res;
        }
        public static string Hex(this IList<byte> buffer, int count)
        {
            string res = "";
            for (int i = buffer.Count - count; i < buffer.Count; ++i)
            {
                var b = buffer[i];
                res += $"{b:X2}";
                if (i < buffer.Count - 1)
                {
                    if (((i + 1) & 3) == 0) res += " |";
                    res += " ";
                }
            }
            return res;
        }
        public static string Hex(this IList<byte> buffer, bool printColorAndCount = true)
        {
            string res = "";
            for (int i = 0; i < buffer.Count; ++i)
            {
                var b = buffer[i];
                res += $"{b:X2}";
                if (i < buffer.Count - 1)
                {
                    if (((i + 1) & 3) == 0) res += " |";
                    res += " ";
                }
            }
            if (printColorAndCount) res = "[" + buffer.Count.ToString().Color("#0AB") + "]\n" + res.Color("#FA0");
            return res;
        }
        #endregion

        #region Type

        #region Cache
        public static readonly Type[] actionTypes = new Type[]
        {
            typeof(Action),
            typeof(Action<>),
            typeof(Action<,>),
            typeof(Action<,,>),
            typeof(Action<,,,>),
            typeof(Action<,,,,>),
            typeof(Action<,,,,,>),
            typeof(Action<,,,,,,>),
            typeof(Action<,,,,,,,>),
            typeof(Action<,,,,,,,,>),
            typeof(Action<,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,,>),
        };

        static readonly Dictionary<string, Type> _typeCacheMap = new();
        static readonly Type objectType = typeof(object);
        static readonly Type nullableType = typeof(Nullable<>);
        static readonly Type iCollectionType = typeof(ICollection);
        static readonly Type delegateType = typeof(Delegate);
        static readonly Type icePacketBaseType = typeof(IcePacketBase);
        static readonly Type icePacketAttributeType = typeof(IcePacketAttribute);
        static readonly HashSet<Type> serializableCollection = new()
        {
            GetType("UnityEngine.Vector2"),
            GetType("UnityEngine.Vector3"),
            GetType("UnityEngine.Vector4"),
            GetType("UnityEngine.Vector2Int"),
            GetType("UnityEngine.Vector3Int"),
            GetType("UnityEngine.Quaternion"),
            GetType("UnityEngine.Matrix4x4"),
        };

        static readonly List<(ushort hash, Type type)> baseTypeCollection = new()
        {
            (0, typeof(byte)),
            (1, typeof(sbyte)),
            (2, typeof(bool)),
            (3, typeof(char)),
            (4, typeof(short)),
            (5, typeof(ushort)),
            (6, typeof(int)),
            (7, typeof(uint)),
            (8, typeof(long)),
            (9, typeof(ulong)),
            (10, typeof(float)),
            (11, typeof(double)),
            (12, typeof(decimal)),
            (13, typeof(string)),
        };

        static Dictionary<ushort, Type> _hash2PktMap = null;
        static Dictionary<Type, ushort> _pkt2HashMap = null;
        static HashSet<Type> _pktNotNullSet = null;

        static (Dictionary<ushort, Type> h2p, Dictionary<Type, ushort> p2h, HashSet<Type> nullableSet) CollectAllTypes()
        {
            _hash2PktMap = new();
            _pkt2HashMap = new();
            _pktNotNullSet = new();
            foreach ((ushort hash, Type t) in baseTypeCollection)
            {
                if (t is null)
                {
                    throw new Exception($"No base type reference to hash {hash}!");
                }
                _hash2PktMap.Add(hash, t);
                _pkt2HashMap.Add(t, hash);
            }
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var ts = a.GetTypes();
                foreach (var t in ts)
                {
                    var packetAttributes = t.GetCustomAttributes(icePacketAttributeType, false);
                    if (packetAttributes.Length > 0)
                    {
                        var attr = (IcePacketAttribute)packetAttributes[0];
                        ushort hash = attr.Hashcode;
                        if (hash == 0) hash = t.GetShortHashCode();

                        TryRecordPacket(t, hash, attr.IsNullable);
                    }
                    else if (!t.IsAbstract && t.IsSubclassOf(icePacketBaseType))
                    {
                        ushort hash = t.GetShortHashCode();

                        TryRecordPacket(t, hash, false);
                    }

                    void TryRecordPacket(Type t, ushort hash, bool isNullable)
                    {
                        if (_hash2PktMap.TryAdd(hash, t))
                        {
                            _pkt2HashMap.Add(t, hash);
                            if (!isNullable && t.IsSealed) _pktNotNullSet.Add(t);
                        }
                        else
                        {
                            throw new Exception($"Packet hash ({hash}) of [{t}] already exists in [{_hash2PktMap[hash]}]");
                        }
                    }
                }
            }
            return (_hash2PktMap, _pkt2HashMap, _pktNotNullSet);
        }

        static Dictionary<ushort, Type> Hash2PktMap => _hash2PktMap ?? CollectAllTypes().h2p;
        static Dictionary<Type, ushort> Pkt2HashMap = _pkt2HashMap ?? CollectAllTypes().p2h;
        static HashSet<Type> PktNotNullSet => _pktNotNullSet ?? CollectAllTypes().nullableSet;

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
        public static bool IsDelegate(this Type type) => delegateType.IsAssignableFrom(type);
        public static ushort GetShortHashCode(this Type type)
        {
            string key = type.Name;
            const ushort pow = 3;
            ushort hashCode = 0;
            if (key != null && key.Length > 0)
            {
                hashCode = 0;
                for (int i = 0; i < key.Length; i++)
                {
                    hashCode = (ushort)(hashCode * pow + key[i]);
                }
            }
            return hashCode;

        }
        public static Type HashCodeToPacketType(ushort hash)
        {
            if (Hash2PktMap.TryGetValue(hash, out var type)) return type;
            throw new Exception($"{hash} is not a packet hash!");
        }
        public static ushort PacketTypeToHashCode(Type type)
        {
            if (Pkt2HashMap.TryGetValue(type, out ushort hash)) return hash;
            throw new Exception($"{type} is not a packet type!");
        }
        public static bool IsPacketType(this Type type) => Pkt2HashMap.ContainsKey(type);
        public static bool IsNotNullPacket(this Type type) => PktNotNullSet.Contains(type);
        public static bool IsSerialzableType(this Type type) => type.IsSerializable || serializableCollection.Contains(type);
        #endregion

        #endregion
    }
}
