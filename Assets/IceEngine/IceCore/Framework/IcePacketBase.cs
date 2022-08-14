namespace IceEngine.Framework
{
    /// <summary>
    /// 继承自IcePacket的结构在被IceBinaryUtility序列化时会更小，但序列化时默认不能为空（可通过IcePacketAttribute配置）
    /// </summary>
    public abstract class IcePacketBase { }
}
