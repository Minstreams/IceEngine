namespace IceEngine.Framework
{

    /// <summary>
    /// 用于重载序列化时的Hashcode（不影响反序列化）
    /// </summary>
    public interface IPacketSerializationHashcode
    {
        public ushort GetHashcode();
    }
}
