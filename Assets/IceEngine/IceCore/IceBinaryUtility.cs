using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IceEngine
{
    /// <summary>
    /// 二进制序列化 & 反序列化
    /// </summary>
    public static class IceBinaryUtility
    {
        #region BitConverter 字节转换器
        // 将基数据类型转换为指定端的一个字节数组，或将一个字节数组转换为指定端基数据类型。

        /// <summary>
        /// 字节转换器是否为小端模式，即低地址存放字数据的低字节，高地址存放字数据的高字节
        /// </summary>
        public readonly static bool isLittleEndian = true;
        static bool IsSameEndian => BitConverter.IsLittleEndian == isLittleEndian;

        #region bool
        /// <summary>
        /// 转换为指定端1字节
        /// </summary>
        public static byte[] GetBytes(bool value) => BitConverter.GetBytes(value);
        /// <summary>
        /// 将指定端模式的1字节转换为bool数据
        /// </summary>
        public static bool ToBoolean(this byte[] buffer, int startIndex = 0) => BitConverter.ToBoolean(buffer, startIndex);
        #endregion

        #region char
        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        public static byte[] GetBytes(char value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的2字节转换为16位char数据
        /// </summary>
        public static char ToChar(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToChar(buffer, startIndex);

            byte[] bytes = new byte[2];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToChar(bytes);
        }
        #endregion

        #region short
        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        public static byte[] GetBytes(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的2字节转换为16位short数据
        /// </summary>
        public static short ToInt16(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToInt16(buffer, startIndex);

            byte[] bytes = new byte[2];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes);
        }
        #endregion

        #region ushort
        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        public static byte[] GetBytes(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的2字节转换为16位ushort数据
        /// </summary>
        public static ushort ToUInt16(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToUInt16(buffer, startIndex);

            byte[] bytes = new byte[2];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes);
        }
        #endregion

        #region int
        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        public static byte[] GetBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的4字节转换为32位int数据
        /// </summary>
        public static int ToInt32(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToInt32(buffer, startIndex);

            byte[] bytes = new byte[4];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes);
        }
        #endregion

        #region uint
        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        public static byte[] GetBytes(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的4字节转换为32位uint数据
        /// </summary>
        public static uint ToUInt32(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToUInt32(buffer, startIndex);

            byte[] bytes = new byte[4];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes);
        }
        #endregion

        #region float
        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        public static byte[] GetBytes(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的4字节转换为32位float数据
        /// </summary>
        public static float ToSingle(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToSingle(buffer, startIndex);

            byte[] bytes = new byte[4];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes);
        }
        #endregion

        #region long
        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        public static byte[] GetBytes(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的8字节转换为64位long数据
        /// </summary>
        public static long ToInt64(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToInt64(buffer, startIndex);

            byte[] bytes = new byte[8];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes);
        }
        #endregion

        #region ulong
        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        public static byte[] GetBytes(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的8字节转换为64位ulong数据
        /// </summary>
        public static ulong ToUInt64(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToUInt64(buffer, startIndex);

            byte[] bytes = new byte[8];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes);
        }
        #endregion

        #region double
        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        public static byte[] GetBytes(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!IsSameEndian) Array.Reverse(bytes);
            return bytes;
        }
        /// <summary>
        /// 将指定端模式的8字节转换为64位double数据
        /// </summary>
        public static double ToDouble(this byte[] buffer, int startIndex = 0)
        {
            if (IsSameEndian) return BitConverter.ToDouble(buffer, startIndex);

            byte[] bytes = new byte[8];
            Array.Copy(buffer, startIndex, bytes, 0, bytes.Length);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes);
        }
        #endregion

        #endregion

        #region Serialization

        #region 待删除
        // 在这里定义基础类型的缩写
        static readonly List<(string compressed, string origin)> baseTypeMapPageList = new()
        {
            ("^S", "System.String"),
            ("+B", "System.Byte"),
            ("=B", "System.SByte"),
            ("^b", "System.Boolean"),
            ("=C", "System.Char"),
            ("=S", "System.Int16"),
            ("+S", "System.UInt16"),
            ("=I", "System.Int32"),
            ("+I", "System.UInt32"),
            ("=L", "System.Int64"),
            ("+L", "System.UInt64"),
            ("=F", "System.Single"),
            ("=D", "System.Double"),
            ("^D", "System.Decimal"),   // Decimal 会转换为double, 造成精度损失
            ("[B", "System.Byte[]"),
            ("^T", "System.RuntimeType"),
        };
        static Dictionary<string, string> BaseTypeMap_ToBytes => _typeMap_ToBytes ??= new Dictionary<string, string>(baseTypeMapPageList.Select(p => new KeyValuePair<string, string>(p.origin, p.compressed))); static Dictionary<string, string> _typeMap_ToBytes = null;
        static Dictionary<string, string> BaseTypeMap_FromBytes => _typeMap_FromBytes ??= new Dictionary<string, string>(baseTypeMapPageList.Select(p => new KeyValuePair<string, string>(p.compressed, p.origin))); static Dictionary<string, string> _typeMap_FromBytes = null;

        static string Hex(byte[] bytes)
        {
            string res = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                res += $"{b:x2}";
                if (i < bytes.Length - 1)
                {
                    if (((i + 1) & 3) == 0) res += " |";
                    res += " ";
                }
            }
            return res;
        }
        static string ASCII(byte[] bytes)
        {
            string res = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                res += $"{(char)b}";
                if (i < bytes.Length - 1) res += " ";
            }
            return res;
        }
        #endregion

        #region 临时log
        // 外部接口
        [System.Diagnostics.Conditional("DEBUG")]
        public static void RegisterLogger(Action<string> onLog) => logger = onLog;
        static Action<string> logger = null;

        // 内部接口
        [System.Diagnostics.Conditional("DEBUG")]
        static void Log(string message) => logger?.Invoke(message);
        static string Hex(this IList<byte> buffer, int count)
        {
            string res = "";
            for (int i = buffer.Count - count; i < buffer.Count; ++i)
            {
                var b = buffer[i];
                res += $"{b:x2}";
                if (i < buffer.Count - 1)
                {
                    if (((i + 1) & 3) == 0) res += " |";
                    res += " ";
                }
            }
            return res;
        }
        static string ASCII(this List<byte> buffer, int count)
        {
            string res = "";
            for (int i = buffer.Count - count; i < buffer.Count; ++i)
            {
                var b = buffer[i];
                res += $"{(char)b}";
                if (i < buffer.Count - 1) res += " ";
            }
            return res;
        }
        #endregion

        /// <summary>
        /// 字符串编码
        /// </summary>
        public readonly static Encoding DefaultEncoding = Encoding.UTF8;

        static class FieldBlockHeaderDefinitions
        {
            public const byte nullField = 0;
            public const byte byteField = 1;
            public const byte sbyteField = 2;
            public const byte boolField = 3;
            public const byte charField = 4;
            public const byte shortField = 5;
            public const byte ushortField = 6;
            public const byte intField = 7;
            public const byte uintField = 8;
            public const byte longField = 9;
            public const byte ulongField = 10;
            public const byte floatField = 11;
            public const byte doubleField = 12;
            public const byte decimalField = 13;
            public const byte bytesField = 14;
            public const byte stringField = 15;
            public const byte typeField = 16;
            public const byte enumField = 17;
        }

        public static void AddFieldBlock(this List<byte> buffer, object obj, bool withHeader = true)
        {
            Log("\n");

            if (obj is null)
            {
                if (withHeader == false) throw new ArgumentException("Nullable Object must have header!");

                Log("null | ");

                buffer.Add(FieldBlockHeaderDefinitions.nullField);
                LogBlock(1);
            }
            else if (obj is byte bt) AddBlock("byte", bt, FieldBlockHeaderDefinitions.byteField, bt);
            else if (obj is sbyte sbt) AddBlock("sbyte", sbt, FieldBlockHeaderDefinitions.sbyteField, (byte)sbt);
            else if (obj is bool b) AddBlock("bool", b, FieldBlockHeaderDefinitions.boolField, GetBytes(b));
            else if (obj is char c) AddBlock("char", c, FieldBlockHeaderDefinitions.charField, GetBytes(c));
            else if (obj is short s) AddBlock("short", s, FieldBlockHeaderDefinitions.shortField, GetBytes(s));
            else if (obj is ushort us) AddBlock("ushort", us, FieldBlockHeaderDefinitions.ushortField, GetBytes(us));
            else if (obj is int i) AddBlock("int", i, FieldBlockHeaderDefinitions.intField, GetBytes(i));
            else if (obj is uint ui) AddBlock("uint", ui, FieldBlockHeaderDefinitions.uintField, GetBytes(ui));
            else if (obj is long l) AddBlock("long", l, FieldBlockHeaderDefinitions.longField, GetBytes(l));
            else if (obj is ulong ul) AddBlock("ulong", ul, FieldBlockHeaderDefinitions.ulongField, GetBytes(ul));
            else if (obj is float ft) AddBlock("float", ft, FieldBlockHeaderDefinitions.floatField, GetBytes(ft));
            else if (obj is double d) AddBlock("double", d, FieldBlockHeaderDefinitions.doubleField, GetBytes(d));
            else if (obj is decimal dd) AddBlock("decimal", dd, FieldBlockHeaderDefinitions.decimalField, GetBytes((double)dd));
            else if (obj is byte[] bts)
            {
                Log($"{"byte[]".Color("#4CA")} | ");

                if (withHeader)
                {
                    buffer.Add(FieldBlockHeaderDefinitions.bytesField);
                    LogBlock(1);
                }

                var lengthBlock = GenLengthBlock(bts);

                buffer.AddRange(lengthBlock);
                LogBlock(lengthBlock.Length);

                buffer.AddRange(bts);
                LogBlock(bts.Length);
            }
            else if (obj is string str)
            {
                Log($"{"string".Color("#4CA")} {str} | ");

                if (withHeader)
                {
                    buffer.Add(FieldBlockHeaderDefinitions.stringField);
                    LogBlock(1);
                }

                var bytes = DefaultEncoding.GetBytes(str);
                var lengthBlock = GenLengthBlock(bytes);

                buffer.AddRange(lengthBlock);
                LogBlock(lengthBlock.Length);

                buffer.AddRange(bytes);
                LogBlock(bytes.Length);

                Log($"({buffer.ASCII(bytes.Length)})");
            }
            else if (obj is Type tp)
            {
                string tName = tp.FullName;
                // TODO: 这里可以做压缩
                Log($"{"Type".Color("#4CA")} {tName} | ");

                if (withHeader)
                {
                    buffer.Add(FieldBlockHeaderDefinitions.typeField);
                    LogBlock(1);
                }

                var bytes = DefaultEncoding.GetBytes(tName);
                var lengthBlock = GenLengthBlock(bytes);

                buffer.AddRange(lengthBlock);
                LogBlock(lengthBlock.Length);

                buffer.AddRange(bytes);
                LogBlock(bytes.Length);

                Log($"({buffer.ASCII(bytes.Length)})");
            }
            else
            {
                // 复杂情况
                Type type = obj.GetType();
                if (type.IsEnum)
                {
                    Log($"{type.FullName.Color("#4CA")} {obj} | ");

                    if (withHeader)
                    {
                        buffer.Add(FieldBlockHeaderDefinitions.enumField);
                        LogBlock(1);
                    }
                    Log("\n{");

                    if (withHeader) buffer.AddFieldBlock(type, withHeader: false);

                    var enumValType = Enum.GetUnderlyingType(type);

                    if (enumValType == TypeDefinitions.byteType) buffer.AddFieldBlock((byte)obj, false);
                    else if (enumValType == TypeDefinitions.shortType) buffer.AddFieldBlock((short)obj, false);
                    else if (enumValType == TypeDefinitions.intType) buffer.AddFieldBlock((int)obj, false);
                    else buffer.AddFieldBlock((long)obj, false);

                    Log("\n}");
                }

            }

            // Inner Functions --------------
            void AddBlock(string typeStr, object val, byte header, params byte[] bytes)
            {
                Log($"{typeStr.Color("#4CA")} {val} | ");

                if (withHeader)
                {
                    buffer.Add(header);
                    LogBlock(1);
                }

                buffer.AddRange(bytes);
                LogBlock(bytes.Length);
            }
            byte[] GenLengthBlock(byte[] bytes)
            {
                int length = bytes.Length;
                if (length > ushort.MaxValue) throw new OverflowException("Length of field bytes overflow!");
                return GetBytes((ushort)length);
            }
            void LogBlock(int count)
            {
                Log($"{count}".Color("#0AB"));
                Log(" [");
                Log(buffer.Hex(count).Color("#FA0"));
                Log("] ");
            }
        }
        public static byte[] ToBytes(object obj)
        {
            List<byte> buffer = new();

            // 处理值
            buffer.AddFieldBlock(obj);

            // 打印所有数据
            Log("\n");
            Log("\n数据: ");
            Log(buffer.Hex(buffer.Count).Color("#FA0"));
            Log("\n长度: ");
            Log($"{buffer.Count}".Color("#0AB"));

            return buffer.ToArray();
        }

        static byte ReadByte(this byte[] bytes, ref int offset)
        {
            var data = bytes[offset];
            offset += 1;
            return data;
        }
        static sbyte ReadSByte(this byte[] bytes, ref int offset)
        {
            var data = (sbyte)bytes[offset];
            offset += 1;
            return data;
        }
        static bool ReadBool(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToBoolean(offset);
            offset += 1;
            return data;
        }
        static char ReadChar(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToChar(offset);
            offset += 2;
            return data;
        }
        static short ReadShort(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToInt16(offset);
            offset += 2;
            return data;
        }
        static ushort ReadUShort(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToUInt16(offset);
            offset += 2;
            return data;
        }
        static int ReadInt(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToInt32(offset);
            offset += 4;
            return data;
        }
        static uint ReadUInt(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToUInt32(offset);
            offset += 4;
            return data;
        }
        static long ReadLong(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToInt64(offset);
            offset += 8;
            return data;
        }
        static ulong ReadULong(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToUInt64(offset);
            offset += 8;
            return data;
        }
        static float ReadFloat(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToSingle(offset);
            offset += 4;
            return data;
        }
        static double ReadDouble(this byte[] bytes, ref int offset)
        {
            var data = bytes.ToDouble(offset);
            offset += 8;
            return data;
        }
        static decimal ReadDecimal(this byte[] bytes, ref int offset)
        {
            return (decimal)bytes.ReadDouble(ref offset);
        }
        static byte[] ReadBytes(this byte[] bytes, ref int offset)
        {
            var length = bytes.ReadUShort(ref offset);
            var data = new byte[length];
            Array.Copy(bytes, offset, data, 0, length);
            return data;
        }
        static string ReadString(this byte[] bytes, ref int offset)
        {
            var length = bytes.ReadUShort(ref offset);
            var data = DefaultEncoding.GetString(bytes, offset, length);
            offset += length;
            return data;
        }
        static Type ReadType(this byte[] bytes, ref int offset)
        {
            return IceCoreUtility.GetType(bytes.ReadString(ref offset));
        }


        public static object FromBytes(byte[] inBytes)
        {
            if (inBytes.Length == 0) throw new ArgumentException("数据为空！");

            byte header = inBytes[0];
            int offset = 1;

            switch (header)
            {
                case FieldBlockHeaderDefinitions.nullField: return null;
                case FieldBlockHeaderDefinitions.byteField: return inBytes.ReadByte(ref offset);
                case FieldBlockHeaderDefinitions.sbyteField: return inBytes.ReadSByte(ref offset);
                case FieldBlockHeaderDefinitions.boolField: return inBytes.ReadBool(ref offset);
                case FieldBlockHeaderDefinitions.charField: return inBytes.ReadChar(ref offset);
                case FieldBlockHeaderDefinitions.shortField: return inBytes.ReadShort(ref offset);
                case FieldBlockHeaderDefinitions.ushortField: return inBytes.ReadUShort(ref offset);
                case FieldBlockHeaderDefinitions.intField: return inBytes.ReadInt(ref offset);
                case FieldBlockHeaderDefinitions.uintField: return inBytes.ReadUInt(ref offset);
                case FieldBlockHeaderDefinitions.longField: return inBytes.ReadLong(ref offset);
                case FieldBlockHeaderDefinitions.ulongField: return inBytes.ReadULong(ref offset);
                case FieldBlockHeaderDefinitions.floatField: return inBytes.ReadFloat(ref offset);
                case FieldBlockHeaderDefinitions.doubleField: return inBytes.ReadDouble(ref offset);
                case FieldBlockHeaderDefinitions.decimalField: return inBytes.ReadDecimal(ref offset);
                case FieldBlockHeaderDefinitions.bytesField: return inBytes.ReadBytes(ref offset);
                case FieldBlockHeaderDefinitions.stringField: return inBytes.ReadString(ref offset);
                case FieldBlockHeaderDefinitions.typeField: return inBytes.ReadType(ref offset);
                case FieldBlockHeaderDefinitions.enumField:
                    var type = inBytes.ReadType(ref offset);

                    Type enumType = Enum.GetUnderlyingType(type);

                    if (enumType == TypeDefinitions.byteType) return Enum.ToObject(type, inBytes.ReadByte(ref offset));
                    else if (enumType == TypeDefinitions.shortType) return Enum.ToObject(type, inBytes.ReadShort(ref offset));
                    else if (enumType == TypeDefinitions.intType) return Enum.ToObject(type, inBytes.ReadInt(ref offset));
                    else return Enum.ToObject(type, inBytes.ReadLong(ref offset));
            }

            return null;
        }


        // ------------------------------- 下面的都要删
        public static byte[] _ToBytes(object obj, out string outLog, bool withHeader = false, int indent = 0)
        {
            // 处理null
            if (obj is null)
            {
                outLog = "数据为空，不用存储任何内容！\n";
                return new byte[0];
            }

            string log = "";

            List<byte> buffer = new();

            byte[] GetFieldHeader(byte[] bytes)
            {
                int length = bytes.Length;
                if (length > ushort.MaxValue) throw new OverflowException("Length of field bytes overflow!");
                return GetBytes((ushort)length);
            }

            void AppendBuffer(byte[] bytes, string record, bool ascii = false)
            {
                string pStr = "";
                for (int i = 0; i < indent; ++i)
                {
                    pStr += "\t";
                }
                if (withHeader)
                {
                    var header = GetFieldHeader(bytes);

                    buffer.AddRange(header);
                    buffer.AddRange(bytes);

                    log += pStr + record;
                    log += $"\n{pStr}----Bytes: 【{Hex(header)}】【{Hex(bytes)}】";
                    if (ascii) log += $"【{ASCII(bytes)}】";
                    log += $"\n{pStr}----长度: {bytes.Length + 2}\t = {2} + {bytes.Length}\n";
                }
                else
                {
                    buffer.AddRange(bytes);

                    log += record;
                    log += $"\n{pStr}----Bytes: 【{Hex(bytes)}】";
                    if (ascii) log += $"【{ASCII(bytes)}】";
                    log += $"\n{pStr}----长度: {bytes.Length}\n";
                }
            }

            // 先搞定类型信息
            Type type = obj.GetType();

            // 处理值
            if (obj is string str) AppendBuffer(DefaultEncoding.GetBytes(str), $"[string] {str}", true);
            else if (obj is byte by) AppendBuffer(new byte[] { by }, $"[byte] {by}");
            else if (obj is sbyte sby) AppendBuffer(new byte[] { (byte)sby }, $"[sbyte] {sby}");
            else if (obj is bool b) AppendBuffer(GetBytes(b), $"[bool] {b}");
            else if (obj is char c) AppendBuffer(GetBytes(c), $"[char] {c}");
            else if (obj is short s) AppendBuffer(GetBytes(s), $"[short] {s}");
            else if (obj is ushort us) AppendBuffer(GetBytes(us), $"[ushort] {us}");
            else if (obj is int i) AppendBuffer(GetBytes(i), $"[int] {i}");
            else if (obj is uint ui) AppendBuffer(GetBytes(ui), $"[uint] {ui}");
            else if (obj is long l) AppendBuffer(GetBytes(l), $"[long] {l}");
            else if (obj is ulong ul) AppendBuffer(GetBytes(ul), $"[ulong] {ul}");
            else if (obj is float ft) AppendBuffer(GetBytes(ft), $"[float] {ft}");
            else if (obj is double d) AppendBuffer(GetBytes(d), $"[double] {d}");
            else if (obj is decimal dd) AppendBuffer(GetBytes((double)dd), $"[decimal] {dd}");
            else if (obj is Type tp)
            {
                string tName = tp.FullName;
                if (BaseTypeMap_ToBytes.TryGetValue(tName, out var bt)) tName = bt;
                AppendBuffer(DefaultEncoding.GetBytes(tName), $"[Type] {tp.FullName} [{tName}]", true);
            }
            else if (obj is byte[] bs) AppendBuffer(bs, $"[bytes]");
            else if (type.IsEnum)
            {
                var enumValType = Enum.GetUnderlyingType(type);

                if (enumValType == TypeDefinitions.byteType) AppendBuffer(new byte[] { (byte)obj }, $"[Enum(byte)] {obj}");
                else if (enumValType == TypeDefinitions.shortType) AppendBuffer(GetBytes((short)obj), $"[Enum(short)] {obj}");
                else if (enumValType == TypeDefinitions.intType) AppendBuffer(GetBytes((int)obj), $"[Enum(int)] {obj}");
                else AppendBuffer(GetBytes((long)obj), $"[Enum(long)] {obj}");
            }
            else if (type.IsClass)
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var f in fields)
                {
                    if (!f.FieldType.IsSerializable) continue;

                    string pStr = "";
                    for (int j = 0; j <= indent; ++j) pStr += "\t";

                    log += $"{pStr}[{f.Name}]\n".Color("#FA0");
                    var fobj = f.GetValue(obj);
                    var bytes = _ToBytes(fobj, out string olog, true, indent + 1);
                    buffer.AddRange(bytes);
                    log += olog;
                    //f.GetValue(obj)
                }
            }

            outLog = log;

            return buffer.ToArray();
        }
        public static string ToBytes(object obj, out byte[] outBytes)
        {
            // 处理null
            if (obj is null)
            {
                outBytes = new byte[0];
                return "数据为空，不用存储任何内容！";
            }

            string log = "";


            List<byte> buffer = new();

            // 先搞定类型信息
            Type type = obj.GetType();
            buffer.AddRange(_ToBytes(type, out string tlog, withHeader: true));
            log += tlog;

            // 处理值
            buffer.AddRange(_ToBytes(obj, out string oLog));
            log += oLog;

            // 打印所有数据
            outBytes = buffer.ToArray();
            log += $"\n{Hex(outBytes)}";
            log += $"\n总长度: {outBytes.Length}";

            return log;
        }

        static class TypeDefinitions
        {
            public static readonly Type byteType = typeof(byte);
            public static readonly Type shortType = typeof(short);
            public static readonly Type intType = typeof(int);

            public static readonly Type stringType = typeof(string);
            public static readonly Type sbyteType = typeof(sbyte);
            public static readonly Type ushortType = typeof(ushort);
            public static readonly Type uintType = typeof(uint);
            public static readonly Type boolType = typeof(bool);
            public static readonly Type charType = typeof(char);
            public static readonly Type longType = typeof(long);
            public static readonly Type ulongType = typeof(ulong);
            public static readonly Type floatType = typeof(float);
            public static readonly Type doubleType = typeof(double);
            public static readonly Type decimalType = typeof(decimal);
            //public static readonly Type dateTimeType = typeof(DateTime);
            public static readonly Type bytesType = typeof(byte[]);
            public static readonly Type dicType = typeof(IDictionary);
            public static readonly Type iEnumerableType = typeof(IEnumerable);
            public static readonly Type arrayType = typeof(Array);
            public static readonly Type listType = typeof(IList);
            public static readonly Type nullableType = typeof(Nullable<>);
        }

        public static object FromBytesT(byte[] inBytes)
        {
            // 处理null
            if (inBytes.Length == 0) return null;

            // 处理类型
            ushort count = ToUInt16(inBytes);
            int offset = 2;
            string tName = DefaultEncoding.GetString(inBytes, offset, count);
            if (BaseTypeMap_FromBytes.TryGetValue(tName, out var bt)) tName = bt;
            Type type = IceCoreUtility.GetType(tName);
            offset += count;

            //if (offset >= inBytes.Length) return null;

            //count = ToUInt16(inBytes, offset);
            //offset += 2;

            // 处理值
            if (type == null) throw new InvalidCastException($"Type cast failed! type: {tName}");
            else if (type == TypeDefinitions.stringType) return DefaultEncoding.GetString(inBytes, offset, inBytes.Length - offset);
            else if (type == TypeDefinitions.byteType) return inBytes[offset];
            else if (type == TypeDefinitions.sbyteType) return (sbyte)inBytes[offset];
            else if (type == TypeDefinitions.boolType) return inBytes.ToBoolean(offset);
            else if (type == TypeDefinitions.charType) return inBytes.ToChar(offset);
            else if (type == TypeDefinitions.shortType) return inBytes.ToInt16(offset);
            else if (type == TypeDefinitions.ushortType) return inBytes.ToUInt16(offset);
            else if (type == TypeDefinitions.intType) return inBytes.ToInt32(offset);
            else if (type == TypeDefinitions.uintType) return inBytes.ToUInt32(offset);
            else if (type == TypeDefinitions.longType) return inBytes.ToInt64(offset);
            else if (type == TypeDefinitions.ulongType) return inBytes.ToUInt64(offset);
            else if (type == TypeDefinitions.floatType) return inBytes.ToSingle(offset);
            else if (type == TypeDefinitions.doubleType) return inBytes.ToDouble(offset);
            else if (type == TypeDefinitions.decimalType) return (decimal)inBytes.ToDouble(offset);
            else if (tName == "System.RuntimeType")
            {
                string ttName = DefaultEncoding.GetString(inBytes, offset, inBytes.Length - offset);
                if (BaseTypeMap_FromBytes.TryGetValue(ttName, out var btt)) ttName = btt;
                return IceCoreUtility.GetType(ttName);
            }
            else if (type.IsEnum)
            {
                Type enumType = Enum.GetUnderlyingType(type);

                if (enumType == TypeDefinitions.byteType) return Enum.ToObject(type, inBytes[offset]);
                else if (enumType == TypeDefinitions.shortType) return Enum.ToObject(type, ToInt16(inBytes, offset));
                else if (enumType == TypeDefinitions.intType) return Enum.ToObject(type, ToInt32(inBytes, offset));
                else return Enum.ToObject(type, ToInt64(inBytes, offset));
            }
            else if (type == TypeDefinitions.bytesType)
            {
                byte[] bytes = new byte[inBytes.Length - offset];
                Array.Copy(inBytes, offset, bytes, 0, bytes.Length);
                return bytes;
            }

            return null;

            throw new Exception($"Unsupported type! type: {tName}");
        }
        #endregion
    }
}