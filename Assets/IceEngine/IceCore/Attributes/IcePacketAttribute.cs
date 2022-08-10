using System;

namespace IceEngine
{
    /// <summary>
    /// 被标记位IcePacket的结构在被IceBinaryUtility序列化时会更小
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class IcePacketAttribute : Attribute
    {
        /// <summary>
        /// Override the hashcode of the packet
        /// </summary>
        public ushort Hashcode { get; set; } = 0;
        /// <summary>
        /// Can this field be null? (Only on sealed class can this be effective)<br/>
        /// Nullable field requires more storage space (1 byte per field)<br/>
        /// Not-nullable field throw an exception if it's null during serialization
        /// </summary>
        public bool IsNullable { get; set; } = false;
    }
}
