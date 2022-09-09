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
        #region BitConverter
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

        #region 临时log
        static event Action<string> Logger = null;

        // 外部接口
        /// <summary>
        /// 在Scope内监听<see cref="ToBytes(object)"/>中的日志
        /// </summary>
        public class LogScope : IDisposable
        {
            readonly Action<string> logAction;
            public LogScope(Action<string> onLog)
            {
                logAction = onLog;
                Logger += logAction;
            }
            void IDisposable.Dispose()
            {
                Logger -= logAction;
            }
        }

        // 内部接口
        [System.Diagnostics.Conditional("DEBUG")]
        static void Log(string message) => Logger?.Invoke(message);
        static string ASCII(this IList<byte> buffer, int count)
        {
            string res = "";
            for (int i = buffer.Count - count; i < buffer.Count; ++i)
            {
                var b = buffer[i];
                res += $"{(char)b}";
                if (i < buffer.Count - 1) res += " ";
            }
            return res.Color("#F0B");
        }
        [System.Diagnostics.Conditional("DEBUG")]
        static void LogBlock(this IList<byte> buffer, int count)
        {
            Log($"{count}".Color("#0AB"));
            Log(" [");
            Log(buffer.Hex(count).Color("#FA0"));
            Log("] ");
        }
        #endregion

        static class FieldBlockHeaderDefinitions
        {
            public const byte nullField = 0x00;
            public const byte byteField = 0x01;
            public const byte sbyteField = 0x02;
            public const byte boolField = 0x03;
            public const byte charField = 0x04;
            public const byte shortField = 0x05;
            public const byte ushortField = 0x06;
            public const byte intField = 0x07;
            public const byte uintField = 0x08;
            public const byte longField = 0x09;
            public const byte ulongField = 0x0A;
            public const byte floatField = 0x0B;
            public const byte doubleField = 0x0C;
            public const byte decimalField = 0x0D;
            public const byte bytesField = 0x0E;
            public const byte stringField = 0x0F;
            public const byte typeField = 0x10;
            public const byte typeHashField = 0x11;
            public const byte classField = 0x12;
            public const byte arrayField = 0x13;
            public const byte collectionField = 0x14;
            public const byte baseClassField = 0x15;
            public const byte packetField = 0x16; // TODO: 针对网络包做进一步优化（去掉type字段）
        }
        static class TypeDefinitions
        {
            public static readonly Type byteType = typeof(byte);
            public static readonly Type sbyteType = typeof(sbyte);
            public static readonly Type boolType = typeof(bool);
            public static readonly Type shortType = typeof(short);
            public static readonly Type ushortType = typeof(ushort);
            public static readonly Type intType = typeof(int);
            public static readonly Type uintType = typeof(uint);
            public static readonly Type charType = typeof(char);
            public static readonly Type longType = typeof(long);
            public static readonly Type ulongType = typeof(ulong);
            public static readonly Type floatType = typeof(float);
            public static readonly Type doubleType = typeof(double);
            public static readonly Type decimalType = typeof(decimal);
            //public static readonly Type dateTimeType = typeof(DateTime);
            public static readonly Type bytesType = typeof(byte[]);
            public static readonly Type stringType = typeof(string);
            //public static readonly Type dicType = typeof(IDictionary);
            public static readonly Type serializeFieldType = IceCoreUtility.GetType("UnityEngine.SerializeField");
            public static readonly Type iCollectionType = typeof(ICollection<>);
            public static readonly Type objectType = typeof(object);


            //public static readonly Type iEnumerableType = typeof(IEnumerable);
            //public static readonly Type arrayType = typeof(Array);
            //public static readonly Type listType = typeof(IList);
        }

        static readonly Dictionary<byte, Type> headerToTypeMap = new()
        {
            { 0x00, null },
            { 0x01, TypeDefinitions.byteType },
            { 0x02, TypeDefinitions.sbyteType },
            { 0x03, TypeDefinitions.boolType },
            { 0x04, TypeDefinitions.charType },
            { 0x05, TypeDefinitions.shortType },
            { 0x06, TypeDefinitions.ushortType },
            { 0x07, TypeDefinitions.intType },
            { 0x08, TypeDefinitions.uintType },
            { 0x09, TypeDefinitions.longType },
            { 0x0A, TypeDefinitions.ulongType },
            { 0x0B, TypeDefinitions.floatType },
            { 0x0C, TypeDefinitions.doubleType },
            { 0x0D, TypeDefinitions.decimalType },
            { 0x0E, TypeDefinitions.bytesType },
            { 0x0F, TypeDefinitions.stringType },
        };

        #region Serialize
        static bool HasHeader(this Type type)
        {
            if (type.IsNullable()) return true;
            if (type.IsValueType) return false;
            if (type.IsNotNullPacket()) return false;
            return true;
        }
        static void AddHeader(this List<byte> buffer, byte header)
        {
            buffer.Add(header);
            buffer.LogBlock(1);
        }
        static void AddBytes(this List<byte> buffer, byte[] bytes)
        {
            buffer.AddRange(bytes);
            buffer.LogBlock(bytes.Length);
        }
        static void AddFieldBlock(this List<byte> buffer, object obj, bool withHeader = true, string name = "_", Type baseType = null, bool withExtraInfo = false)
        {
            Log("\n");

            if (obj is null)
            {
                if (withHeader == false)
                {
                    if (baseType.IsNotNullPacket()) throw new ArgumentException($"Sealed packet can not be null! [{baseType?.FullName} {name}] (set IcePacketAttribute.IsNullable to false if it's intended to do so)");
                    throw new ArgumentException($"Nullable Object must have header! [{baseType?.FullName} {name}]");
                }

                Log("null \t| ");

                buffer.AddHeader(FieldBlockHeaderDefinitions.nullField);
            }
            else if (obj is byte bt) AddBlock("byte", bt, FieldBlockHeaderDefinitions.byteField, bt);
            else if (obj is sbyte sbt) AddBlock("sbyte", sbt, FieldBlockHeaderDefinitions.sbyteField, (byte)sbt);
            else if (obj is bool b) AddBlock("bool", b, FieldBlockHeaderDefinitions.boolField, GetBytes(b));
            else if (obj is char c) AddBlock("char", c, FieldBlockHeaderDefinitions.charField, GetBytes(c));
            else if (obj is short s) AddBlock("short", s, FieldBlockHeaderDefinitions.shortField, GetBytes(s));
            else if (obj is ushort us) AddBlock("ushort", us, FieldBlockHeaderDefinitions.ushortField, GetBytes(us));
            else if (obj is int it) AddBlock("int", it, FieldBlockHeaderDefinitions.intField, GetBytes(it));
            else if (obj is uint ui) AddBlock("uint", ui, FieldBlockHeaderDefinitions.uintField, GetBytes(ui));
            else if (obj is long l) AddBlock("long", l, FieldBlockHeaderDefinitions.longField, GetBytes(l));
            else if (obj is ulong ul) AddBlock("ulong", ul, FieldBlockHeaderDefinitions.ulongField, GetBytes(ul));
            else if (obj is float ft) AddBlock("float", ft, FieldBlockHeaderDefinitions.floatField, GetBytes(ft));
            else if (obj is double d) AddBlock("double", d, FieldBlockHeaderDefinitions.doubleField, GetBytes(d));
            else if (obj is decimal dd) AddBlock("decimal", dd, FieldBlockHeaderDefinitions.decimalField, GetBytes((double)dd));
            else if (obj is byte[] bts)
            {
                Log($"{"byte[]".Color("#4CA")} {name.Color("#AF0")} \t| ");

                if (withHeader) buffer.AddHeader(FieldBlockHeaderDefinitions.bytesField);

                var lengthBlock = GenLengthBlock(bts);

                buffer.AddBytes(lengthBlock);
                buffer.AddBytes(bts);
            }
            else if (obj is string str)
            {
                Log($"{"string".Color("#4CA")} {name.Color("#AF0")} = {str.Color("#F0B")} \t| ");

                if (withHeader) buffer.AddHeader(FieldBlockHeaderDefinitions.stringField);

                var bytes = str.GetBytes();
                var lengthBlock = GenLengthBlock(bytes);

                buffer.AddBytes(lengthBlock);
                buffer.AddBytes(bytes);

                Log($" 【{buffer.ASCII(bytes.Length)}】".Color("#F4A"));
            }
            else if (obj is Type tp)
            {
                string tName = tp.FullName;
                Log($"{"Type".Color("#4CA")} {name.Color("#AF0")} = {tName.Color("#F0B")} \t| ");

                if (withHeader)
                {
                    if (tp.IsPacketType())
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.typeHashField);
                        buffer.AddBytes(GetBytes(IceCoreUtility.PacketTypeToHashCode(tp)));
                        return;
                    }
                    else
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.typeField);
                    }
                }

                var bytes = tName.GetBytes();
                var lengthBlock = GenLengthBlock(bytes);

                buffer.AddBytes(lengthBlock);
                buffer.AddBytes(bytes);

                Log($" 【{buffer.ASCII(bytes.Length)}】".Color("#F4A"));
            }
            else
            {
                // 复杂情况
                Type type = obj.GetType();
                if (type.IsEnum)
                {
                    Log($"{type.Name.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");

                    if (withHeader)
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.classField);
                        buffer.AddFieldBlock(type, withHeader: false);
                    }

                    var enumValType = Enum.GetUnderlyingType(type);

                    if (enumValType == TypeDefinitions.byteType) buffer.AddFieldBlock((byte)obj, false);
                    else if (enumValType == TypeDefinitions.shortType) buffer.AddFieldBlock((short)obj, false);
                    else if (enumValType == TypeDefinitions.intType) buffer.AddFieldBlock((int)obj, false);
                    else buffer.AddFieldBlock((long)obj, false);

                    Log("\n}");
                }
                else if (type.IsArray)
                {
                    Log($"{type.Name.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");
                    if (withHeader)
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.arrayField);
                    }

                    var ar = (Array)obj;
                    Type iType = type.GetElementType();

                    int length = ar.Length;
                    buffer.AddFieldBlock(length, false, "length");

                    bool hasHeader = iType.HasHeader();
                    for (int i = 0; i < length; i++)
                    {
                        buffer.AddFieldBlock(ar.GetValue(i), hasHeader, $"[{i}]", iType);
                    }
                    Log("\n}");
                }
                else if (type.IsCollection())
                {
                    Log($"{type.Name.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");
                    if (withHeader)
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.collectionField);
                    }

                    var ic = (ICollection)obj;
                    var iType = ic.AsQueryable().ElementType;
                    int count = ic.Count;

                    buffer.AddFieldBlock(count, false, "count");

                    bool hasHeader = iType.HasHeader();

                    int i = 0;
                    foreach (var item in ic)
                    {
                        buffer.AddFieldBlock(item, hasHeader, $"[{i++}]", iType, withExtraInfo);
                    }
                    Log("\n}");
                }
                else if (type.IsClass || type.IsValueType)
                {
                    Log($"{(baseType ?? type).Name.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");
                    if (withHeader)
                    {
                        if (baseType == type) buffer.AddHeader(FieldBlockHeaderDefinitions.baseClassField);
                        else if (type.IsPacketType())
                        {
                            buffer.AddHeader(FieldBlockHeaderDefinitions.packetField);
                            buffer.AddFieldBlock(IceCoreUtility.PacketTypeToHashCode(type), false, "typeHash");
                        }
                        else
                        {
                            buffer.AddHeader(FieldBlockHeaderDefinitions.classField);
                            buffer.AddFieldBlock(type, false, "type");
                        }
                    }
                    string extraInfoColor = "#C5F";
                    if (withExtraInfo)
                    {
                        buffer.Add(0);
                        buffer.Add(0);
                        Log("\n");
                        Log("┍━━━━━━━━━".Color(extraInfoColor));
                        Log(" [");
                        Log(" LOCK ".Color(extraInfoColor));
                        Log("] ");
                        Log("━━━━━━━━━┑ ".Color(extraInfoColor));
                        Log($"{2}".Color("#0AB"));
                    }
                    int mark = buffer.Count;

                    List<FieldInfo> fieldList = new();
                    List<FieldInfo> changableFieldList = new();    // 容易改变的 field 信息
                    for (var t = type; t != TypeDefinitions.objectType; t = t.BaseType) fieldList.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
                    foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (f.DeclaringType == type) changableFieldList.Add(f);
                        else fieldList.Add(f);
                    }
                    fieldList.AddRange(changableFieldList);

                    foreach (var f in fieldList)
                    {
                        var fType = f.FieldType;
                        if (f.IsNotSerialized) continue;
                        if (!fType.IsSerialzableType() && !fType.IsPacketType()) continue;
                        if (fType.IsDelegate()) continue;
                        if (!type.IsValueType && TypeDefinitions.serializeFieldType != null && f.IsPrivate && f.GetCustomAttribute(TypeDefinitions.serializeFieldType) is null) continue;

                        var fobj = f.GetValue(obj);

                        buffer.AddFieldBlock(fobj, fType.HasHeader(), f.Name, fType, withExtraInfo);
                    }

                    if (withExtraInfo)
                    {
                        int length = buffer.Count - mark;
                        if (length > ushort.MaxValue) throw new OverflowException($"Length of block overflow! | {(baseType ?? type).Name.Color("#4CA")} {name.Color("#AF0")}");
                        var blockLengthBts = GetBytes((ushort)length);
                        buffer[mark - 2] = blockLengthBts[0];
                        buffer[mark - 1] = blockLengthBts[1];
                        Log("\n");
                        Log($"┕━━━━━━━━━".Color(extraInfoColor));
                        Log(" [ ");
                        Log(blockLengthBts.Hex(2).Color(extraInfoColor));
                        Log(" ] ");
                        Log($"━━━━━━━━━┙ {length}".Color(extraInfoColor));
                    }

                    Log("\n}");
                }

            }

            // Inner Functions --------------
            void AddBlock(string typeStr, object val, byte header, params byte[] bytes)
            {
                Log($"{typeStr.Color("#4CA")} {name.Color("#AF0")} = {val} \t| ");

                if (withHeader) buffer.AddHeader(header);

                buffer.AddBytes(bytes);
            }
            byte[] GenLengthBlock(byte[] bytes)
            {
                int length = bytes.Length;
                if (length > ushort.MaxValue) throw new OverflowException("Length of field bytes overflow!");
                return GetBytes((ushort)length);
            }
        }

        public static byte[] ToBytes(object obj, bool withHeader = true, Type baseType = null, bool withExtraInfo = false)
        {
            List<byte> buffer = new();

            // 处理值
            buffer.AddFieldBlock(obj, withHeader, baseType: baseType, withExtraInfo: withExtraInfo);

            // 打印所有数据
            Log("\n");
            Log("\n数据: ");
            Log(buffer.Hex(buffer.Count).Color("#FA0"));
            Log("\n长度: ");
            Log($"{buffer.Count}".Color("#0AB"));

            return buffer.ToArray();
        }
        #endregion

        #region Deserialize
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
            var data = bytes.GetString(offset, length);
            offset += length;
            return data;
        }
        static Type ReadType(this byte[] bytes, ref int offset)
        {
            return IceCoreUtility.GetType(bytes.ReadString(ref offset));
        }
        static object ReadValueOfType(this byte[] bytes, ref int offset, Type type, object instance = null, bool withExtraInfo = false)
        {
            if (type is null) return null;
            if (type.IsNullable()) type = type.GetGenericArguments()[0];
            if (type == TypeDefinitions.byteType) return bytes.ReadByte(ref offset);
            if (type == TypeDefinitions.sbyteType) return bytes.ReadSByte(ref offset);
            if (type == TypeDefinitions.boolType) return bytes.ReadBool(ref offset);
            if (type == TypeDefinitions.charType) return bytes.ReadChar(ref offset);
            if (type == TypeDefinitions.shortType) return bytes.ReadShort(ref offset);
            if (type == TypeDefinitions.ushortType) return bytes.ReadUShort(ref offset);
            if (type == TypeDefinitions.intType) return bytes.ReadInt(ref offset);
            if (type == TypeDefinitions.uintType) return bytes.ReadUInt(ref offset);
            if (type == TypeDefinitions.longType) return bytes.ReadLong(ref offset);
            if (type == TypeDefinitions.ulongType) return bytes.ReadULong(ref offset);
            if (type == TypeDefinitions.floatType) return bytes.ReadFloat(ref offset);
            if (type == TypeDefinitions.doubleType) return bytes.ReadDouble(ref offset);
            if (type == TypeDefinitions.decimalType) return bytes.ReadDecimal(ref offset);
            if (type == TypeDefinitions.bytesType) return bytes.ReadBytes(ref offset);
            if (type == TypeDefinitions.stringType) return bytes.ReadString(ref offset);
            string tName = type.FullName;
            if (tName == "System.RuntimeType") return bytes.ReadType(ref offset);
            if (type.IsEnum)
            {
                Type enumType = Enum.GetUnderlyingType(type);

                if (enumType == TypeDefinitions.byteType) return Enum.ToObject(type, bytes.ReadByte(ref offset));
                else if (enumType == TypeDefinitions.shortType) return Enum.ToObject(type, bytes.ReadShort(ref offset));
                else if (enumType == TypeDefinitions.intType) return Enum.ToObject(type, bytes.ReadInt(ref offset));
                else return Enum.ToObject(type, bytes.ReadLong(ref offset));
            }
            if (type.IsArray)
            {
                int length = bytes.ReadInt(ref offset);
                Type iType = type.GetElementType();
                bool hasHeader = iType.HasHeader();

                var ar = Array.CreateInstance(iType, length);
                if (hasHeader)
                {
                    for (int i = 0; i < length; ++i) ar.SetValue(bytes.ReadValueWithHeader(ref offset, iType), i);
                }
                else
                {
                    for (int i = 0; i < length; ++i) ar.SetValue(bytes.ReadValueOfType(ref offset, iType), i);
                }

                return ar;
            }
            if (type.IsCollection())
            {
                var ic = (ICollection)(instance ?? Activator.CreateInstance(type));
                Type iType = ic.AsQueryable().ElementType;
                var cType = TypeDefinitions.iCollectionType.MakeGenericType(iType);
                var addMethod = cType.GetMethod("Add");
                var clearMethod = cType.GetMethod("Clear");

                clearMethod.Invoke(ic, null);

                int count = bytes.ReadInt(ref offset);
                bool hasHeader = iType.HasHeader();

                if (hasHeader)
                {
                    for (int i = 0; i < count; ++i) addMethod.Invoke(ic, new object[] { bytes.ReadValueWithHeader(ref offset, iType, null, withExtraInfo) });
                }
                else
                {
                    for (int i = 0; i < count; ++i) addMethod.Invoke(ic, new object[] { bytes.ReadValueOfType(ref offset, iType, null, withExtraInfo) });
                }

                return ic;
            }
            if (type.IsClass || type.IsValueType)
            {
                var obj = instance ?? Activator.CreateInstance(type);
                bytes.ReadObjectOverride(ref offset, obj, type, withExtraInfo);
                return obj;
            }
            throw new NotSupportedException($"Not supported type: {type.FullName}");
        }
        static object ReadValueWithHeader(this byte[] bytes, ref int offset, Type baseType = null, object instance = null, bool withExtraInfo = false)
        {
            byte header = bytes.ReadByte(ref offset);

            if (headerToTypeMap.TryGetValue(header, out Type t)) return bytes.ReadValueOfType(ref offset, t);

            return header switch
            {
                FieldBlockHeaderDefinitions.typeField => bytes.ReadType(ref offset),
                FieldBlockHeaderDefinitions.typeHashField => IceCoreUtility.HashCodeToPacketType(bytes.ReadUShort(ref offset)),
                FieldBlockHeaderDefinitions.classField => bytes.ReadValueOfType(ref offset, bytes.ReadType(ref offset), instance, withExtraInfo),
                FieldBlockHeaderDefinitions.arrayField or
                FieldBlockHeaderDefinitions.collectionField or
                FieldBlockHeaderDefinitions.baseClassField => bytes.ReadValueOfType(ref offset, baseType, instance, withExtraInfo),
                FieldBlockHeaderDefinitions.packetField => bytes.ReadValueOfType(ref offset, IceCoreUtility.HashCodeToPacketType(bytes.ReadUShort(ref offset)), instance, withExtraInfo),
                _ => throw new Exception($"Not supported header! {header}"),
            };
        }
        static void ReadObjectOverride(this byte[] bytes, ref int offset, object obj, Type type = null, bool withExtraInfo = false)
        {
            if (type is null) type = obj.GetType();

            ushort blockLength = ushort.MaxValue;
            if (withExtraInfo)
            {
                blockLength = bytes.ReadUShort(ref offset);
            }
            int mark = offset;

            List<FieldInfo> fieldList = new();
            List<FieldInfo> changableFieldList = new();    // 容易改变的 field 信息
            for (var t = type; t != TypeDefinitions.objectType; t = t.BaseType) fieldList.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (f.DeclaringType == type) changableFieldList.Add(f);
                else fieldList.Add(f);
            }
            fieldList.AddRange(changableFieldList);

            foreach (var f in fieldList)
            {
                var fType = f.FieldType;
                if (f.IsNotSerialized) continue;
                if (!fType.IsSerialzableType() && !fType.IsPacketType()) continue;
                if (fType.IsDelegate()) continue;
                if (!type.IsValueType && TypeDefinitions.serializeFieldType != null && f.IsPrivate && f.GetCustomAttribute(TypeDefinitions.serializeFieldType) is null) continue;

                bool bHeader = fType.HasHeader();
                if (bHeader)
                {
                    object instance = bytes[offset] switch
                    {
                        FieldBlockHeaderDefinitions.classField or FieldBlockHeaderDefinitions.baseClassField => instance = f.GetValue(obj),
                        _ => null,
                    };

                    f.SetValue(obj, bytes.ReadValueWithHeader(ref offset, fType, instance, withExtraInfo));
                }
                else
                {
                    f.SetValue(obj, bytes.ReadValueOfType(ref offset, fType, null, withExtraInfo));
                }

                if (withExtraInfo)
                {
                    if (offset - mark == blockLength) break;
                    if (offset - mark > blockLength)
                    {
                        offset = mark + blockLength;
                        break;
                    }
                }
            }

            if (withExtraInfo && offset - mark < blockLength)
            {
                offset = mark + blockLength;
            }
        }

        public static object FromBytes(byte[] bytes, int offset = 0, Type baseType = null, bool withExtraInfo = false)
        {
            if (bytes.Length == 0) throw new ArgumentException("Bytes is null!");

            return bytes.ReadValueWithHeader(ref offset, baseType, null, withExtraInfo);
        }
        public static void FromBytesOverwrite(byte[] bytes, object objectToOverwrite, Type type = null, bool withHeader = true, int offset = 0, bool withExtraInfo = false)
        {
            if (bytes.Length == 0) throw new ArgumentException("Bytes is null!");
            if (objectToOverwrite is null) throw new ArgumentException("Target is null!");

            if (type is null) type = objectToOverwrite.GetType();

            if (withHeader)
            {
                bytes.ReadValueWithHeader(ref offset, type, objectToOverwrite, withExtraInfo);
            }
            else
            {
                if (type.IsEnum || type.IsArray || type.IsCollection() || (!type.IsClass && !type.IsValueType)) throw new ArgumentException($"Unsupported type {type}");
                bytes.ReadObjectOverride(ref offset, objectToOverwrite, type, withExtraInfo);
            }
        }
        #endregion

        #endregion

        #region Difference
        enum DiffStatus
        {
            start,
            preserve,
            diff,
        }
        public static byte[] GetDiff(byte[] oldBytes, byte[] newBytes)
        {
            Log("\nOld");
            Log("\n数据: ");
            Log(oldBytes.Hex(oldBytes.Length).Color("#FA0"));
            Log("\n长度: ");
            Log($"{oldBytes.Length}".Color("#0AB"));

            Log("\n");

            Log("\nNew");
            Log("\n数据: ");
            Log(newBytes.Hex(newBytes.Length).Color("#FA0"));
            Log("\n长度: ");
            Log($"{newBytes.Length}".Color("#0AB"));

            List<byte> buffer = new();

            int po = 0;
            int pn = 0;

            DiffStatus status = DiffStatus.start;
            ushort count = 0;

            void Skip(ushort count)
            {
                if (count == 0) return;
                Log("\n");
                buffer.AddHeader(DiffHeaderDefinitions.skip);
                buffer.AddBytes(GetBytes(count));

                ushort tCount = count;
                while (count > 0)
                {
                    buffer.Add(oldBytes[po++]);
                    --count;
                }
                buffer.LogBlock(tCount);
            }
            void SkipAll()
            {
                int l = oldBytes.Length - po;
                while (l > 0)
                {
                    count = l > ushort.MaxValue ? ushort.MaxValue : (ushort)l;
                    Skip(count);
                    l -= ushort.MaxValue;
                }
            }
            void Insert(ushort count)
            {
                if (count == 0) return;
                Log("\n");
                buffer.AddHeader(DiffHeaderDefinitions.insert);
                buffer.AddBytes(GetBytes(count));

                ushort tCount = count;
                while (count > 0)
                {
                    buffer.Add(newBytes[pn++]);
                    --count;
                }
                buffer.LogBlock(tCount);
            }
            void InsertAll()
            {
                int l = newBytes.Length - pn;
                while (l > 0)
                {
                    count = l > ushort.MaxValue ? ushort.MaxValue : (ushort)l;
                    Insert(count);
                    l -= ushort.MaxValue;
                }
            }
            void Preserve(ushort count)
            {
                Log("\n");
                buffer.AddHeader(DiffHeaderDefinitions.preserve);
                buffer.AddBytes(GetBytes(count));
            }
            void Next()
            {
                ++po;
                ++pn;
                ++count;
            }

            while (true)
            {
                if (pn >= newBytes.Length)
                {
                    if (status == DiffStatus.preserve) Preserve(count);
                    else if (status == DiffStatus.diff)
                    {
                        // insert current
                        po -= count;
                        pn -= count;
                        Insert(count);
                    }

                    SkipAll();
                    break;
                }
                else if (po >= oldBytes.Length)
                {
                    if (status == DiffStatus.preserve) Preserve(count);
                    else if (status == DiffStatus.diff)
                    {
                        // skip current
                        po -= count;
                        pn -= count;
                        Skip(count);
                    }

                    InsertAll();
                    break;
                }

                if (status == DiffStatus.start)
                {
                    if (oldBytes[po] == newBytes[pn])
                    {
                        // start preserve
                        status = DiffStatus.preserve;
                        count = 0;
                    }
                    else
                    {
                        // start diff
                        status = DiffStatus.diff;
                        count = 0;
                    }
                }
                else if (status == DiffStatus.preserve)
                {
                    if (oldBytes[po] == newBytes[pn])
                    {
                        // keep preserve
                        if (count == ushort.MaxValue)
                        {
                            Preserve(count);
                            count = 0;
                        }
                    }
                    else
                    {
                        // preserve current
                        Preserve(count);

                        // start diff
                        status = DiffStatus.diff;
                        count = 0;
                    }
                }
                else if (status == DiffStatus.diff)
                {
                    if (count == ushort.MaxValue)
                    {
                        // skip and insert
                        po -= count;
                        pn -= count;
                        Skip(count);
                        Insert(count);

                        count = 0;
                    }

                    for (ushort oi = 0; oi <= count; ++oi)
                    {
                        ushort ni = (ushort)(count - oi);
                        if (oldBytes[po - oi] == newBytes[pn - ni])
                        {
                            // skip [ni], insert [oi]
                            po -= count;
                            pn -= count;
                            Skip(ni);
                            Insert(oi);

                            // start preserve
                            status = DiffStatus.preserve;
                            count = 0;

                            break;
                        }
                    }
                }

                Next();
            }

            Log("\n");
            Log("\n数据: ");
            Log(buffer.Hex(buffer.Count).Color("#FA0"));
            Log("\n长度: ");
            Log($"{buffer.Count}".Color("#0AB"));

            return buffer.ToArray();
        }
        class DiffHeaderDefinitions
        {
            public const byte preserve = 0x01;
            public const byte skip = 0x02;
            public const byte insert = 0x03;
        }
        public static byte[] ApplyDiff(byte[] bytes, byte[] diff)
        {
            List<byte> buffer = new();

            int p = 0;
            int offset = 0;
            while (offset < diff.Length)
            {
                var header = diff.ReadByte(ref offset);
                ushort length = diff.ReadUShort(ref offset);
                switch (header)
                {
                    case DiffHeaderDefinitions.preserve:
                        for (int i = 0; i < length; ++i) buffer.Add(bytes[p++]);
                        break;
                    case DiffHeaderDefinitions.skip:
                        for (int i = 0; i < length; ++i) if (bytes[p++] != diff[offset++]) throw new Exception($"Bytes can not match the diff!");
                        break;
                    case DiffHeaderDefinitions.insert:
                        for (int i = 0; i < length; ++i) buffer.Add(diff[offset++]);
                        break;
                    default:
                        throw new Exception($"Unsupported diff header! {header}");
                }
            }

            return buffer.ToArray();
        }
        public static byte[] ReverseDiff(byte[] bytes, byte[] diff)
        {
            List<byte> buffer = new();

            int p = 0;
            int offset = 0;
            while (offset < diff.Length)
            {
                var header = diff.ReadByte(ref offset);
                ushort length = diff.ReadUShort(ref offset);
                switch (header)
                {
                    case DiffHeaderDefinitions.preserve:
                        for (int i = 0; i < length; ++i) buffer.Add(bytes[p++]);
                        break;
                    case DiffHeaderDefinitions.skip:
                        for (int i = 0; i < length; ++i) buffer.Add(diff[offset++]);
                        break;
                    case DiffHeaderDefinitions.insert:
                        for (int i = 0; i < length; ++i) if (bytes[p++] != diff[offset++]) throw new Exception($"Bytes can not match the diff!");
                        break;
                    default:
                        throw new Exception($"Unsupported diff header! {header}");
                }
            }

            return buffer.ToArray();
        }

        #endregion
    }
}