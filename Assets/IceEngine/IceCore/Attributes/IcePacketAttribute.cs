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
    }
}
