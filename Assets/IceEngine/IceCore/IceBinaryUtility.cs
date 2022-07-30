﻿using System;
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
        [System.Diagnostics.Conditional("DEBUG")]
        static void LogBlock(this List<byte> buffer, int count)
        {
            Log($"{count}".Color("#0AB"));
            Log(" [");
            Log(buffer.Hex(count).Color("#FA0"));
            Log("] ");
        }
        #endregion

        /// <summary>
        /// 字符串编码
        /// </summary>
        public readonly static Encoding DefaultEncoding = Encoding.UTF8;

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
            public const byte classField = 0x11;
            public const byte arrayField = 0x12;
            public const byte collectionField = 0x13;
            public const byte baseClassField = 0x14;
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
            public static readonly Type dicType = typeof(IDictionary);

            public static readonly Type iEnumerableType = typeof(IEnumerable);
            public static readonly Type arrayType = typeof(Array);
            public static readonly Type listType = typeof(IList);
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

        #region Optimization
        //static List<(short, Type)>
        #endregion

        #region Serialize
        static bool HasHeader(this Type type)
        {
            if (type.IsNullable()) return true;
            if (type.IsCollection()) return false;
            if (type.IsSealed) return false;
            if (type.IsGenericType) return false;
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
        public static void AddFieldBlock(this List<byte> buffer, object obj, bool withHeader = true, string name = "_", Type baseType = null)
        {
            Log("\n");

            if (obj is null)
            {
                if (withHeader == false) throw new ArgumentException("Nullable Object must have header!");

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
                Log($"{"string".Color("#4CA")} {name.Color("#AF0")} = {str} \t| ");

                if (withHeader) buffer.AddHeader(FieldBlockHeaderDefinitions.stringField);

                var bytes = DefaultEncoding.GetBytes(str);
                var lengthBlock = GenLengthBlock(bytes);

                buffer.AddBytes(lengthBlock);
                buffer.AddBytes(bytes);

                Log($"({buffer.ASCII(bytes.Length)})");
            }
            else if (obj is Type tp)
            {
                string tName = tp.FullName;
                // TODO: 这里可以做压缩
                Log($"{"Type".Color("#4CA")} {name.Color("#AF0")} = {tName} \t| ");

                if (withHeader) buffer.AddHeader(FieldBlockHeaderDefinitions.typeField);

                var bytes = DefaultEncoding.GetBytes(tName);
                var lengthBlock = GenLengthBlock(bytes);

                buffer.AddBytes(lengthBlock);
                buffer.AddBytes(bytes);

                Log($"({buffer.ASCII(bytes.Length)})");
            }
            else
            {
                // 复杂情况
                Type type = obj.GetType();
                if (type.IsEnum)
                {
                    Log($"{type.FullName.Color("#4CA")} {name.Color("#AF0")} = {obj}");

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
                    Log($"{type.FullName.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");
                    if (withHeader)
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.arrayField);
                        buffer.AddFieldBlock(type, withHeader: false);
                    }

                    var ar = (Array)obj;
                    Type iType = type.GetElementType();

                    int length = ar.Length;
                    if (length > ushort.MaxValue) throw new OverflowException("Length of array length overflow!");
                    buffer.AddFieldBlock((ushort)length, false, "length");

                    bool hasHeader = iType.HasHeader();
                    for (int i = 0; i < length; i++)
                    {
                        buffer.AddFieldBlock(ar.GetValue(i), hasHeader, $"[{i}]", iType);
                    }
                    Log("\n}");
                }
                else if (type.IsCollection())
                {
                    Log($"{type.FullName.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");
                    if (withHeader)
                    {
                        buffer.AddHeader(FieldBlockHeaderDefinitions.collectionField);
                        buffer.AddFieldBlock(type, withHeader: false);
                    }

                    var ic = (ICollection)obj;
                    var iType = ic.AsQueryable().ElementType;
                    int count = ic.Count;

                    if (count > ushort.MaxValue) throw new OverflowException("Length of collection count overflow!");
                    buffer.AddFieldBlock((ushort)count, false, "count");

                    bool hasHeader = iType.HasHeader();

                    int i = 0;
                    foreach (var item in ic)
                    {
                        buffer.AddFieldBlock(item, hasHeader, $"[{i++}]", iType);
                    }
                    Log("\n}");
                }
                else if (type.IsClass)
                {
                    Log($"{type.FullName.Color("#4CA")} {name.Color("#AF0")} = {obj}");

                    Log("\n{       ");
                    if (withHeader)
                    {
                        if (baseType == type) buffer.AddHeader(FieldBlockHeaderDefinitions.baseClassField);
                        else
                        {
                            buffer.AddHeader(FieldBlockHeaderDefinitions.classField);
                            buffer.AddFieldBlock(type, withHeader: false, name: "type");
                        }
                    }

                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var f in fields)
                    {
                        var fType = f.FieldType;
                        if (!fType.IsSerializable) continue;
                        if (TypeDefinitions.dicType.IsAssignableFrom(fType)) continue;

                        var fobj = f.GetValue(obj);

                        buffer.AddFieldBlock(fobj, withHeader: fType.HasHeader(), name: f.Name);
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
            var data = DefaultEncoding.GetString(bytes, offset, length);
            offset += length;
            return data;
        }
        static Type ReadType(this byte[] bytes, ref int offset)
        {
            return IceCoreUtility.GetType(bytes.ReadString(ref offset));
        }
        static object ReadValueOfType(this byte[] bytes, ref int offset, Type type)
        {
            if (type == null) return null;
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
                ushort length = bytes.ReadUShort(ref offset);
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
                var ic = (ICollection)Activator.CreateInstance(type);
                var addMethod = type.GetMethod("Add");

                ushort count = bytes.ReadUShort(ref offset);
                Type iType = ic.AsQueryable().ElementType;
                bool hasHeader = iType.HasHeader();

                if (hasHeader)
                {
                    for (int i = 0; i < count; ++i) addMethod.Invoke(ic, new object[] { bytes.ReadValueWithHeader(ref offset, iType) });
                }
                else
                {
                    for (int i = 0; i < count; ++i) addMethod.Invoke(ic, new object[] { bytes.ReadValueOfType(ref offset, iType) });
                }

                return ic;
            }
            if (type.IsClass)
            {
                var obj = Activator.CreateInstance(type);
                bytes.ReadObjectOverride(ref offset, obj, type);
                return obj;
            }
            return null;
        }
        static object ReadValueWithHeader(this byte[] bytes, ref int offset, Type baseType = null)
        {
            byte header = bytes.ReadByte(ref offset);

            if (headerToTypeMap.TryGetValue(header, out Type t)) return bytes.ReadValueOfType(ref offset, t);

            switch (header)
            {
                case FieldBlockHeaderDefinitions.typeField: return bytes.ReadType(ref offset);
                case FieldBlockHeaderDefinitions.classField:
                case FieldBlockHeaderDefinitions.arrayField:
                case FieldBlockHeaderDefinitions.collectionField: return bytes.ReadValueOfType(ref offset, bytes.ReadType(ref offset));
                case FieldBlockHeaderDefinitions.baseClassField: return bytes.ReadValueOfType(ref offset, baseType);
            }

            throw new Exception($"Not supported header! {header}");
        }
        static void ReadObjectOverride(this byte[] bytes, ref int offset, object obj, Type type = null)
        {
            if (type == null) type = obj.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                var fType = f.FieldType;
                if (!fType.IsSerializable) continue;
                if (TypeDefinitions.dicType.IsAssignableFrom(fType)) continue;

                bool bHeader = fType.HasHeader();
                if (bHeader) f.SetValue(obj, bytes.ReadValueWithHeader(ref offset));
                else f.SetValue(obj, bytes.ReadValueOfType(ref offset, fType));
            }
        }

        public static object FromBytes(byte[] inBytes)
        {
            if (inBytes.Length == 0) throw new ArgumentException("数据为空！");

            int offset = 0;
            return inBytes.ReadValueWithHeader(ref offset);
        }
        #endregion

        #endregion
    }
}