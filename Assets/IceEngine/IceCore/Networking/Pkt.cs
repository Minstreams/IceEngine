using IceEngine.Framework;

namespace IceEngine.Networking.Framework
{
    /// <summary>
    /// Base class of general packet.
    /// 
    /// <para>
    /// Prefix reference of subclass:
    /// </para>
    /// 
    /// <b>UDP</b>:
    /// <list type="bullet">
    ///     <item>U【UDP】</item>
    /// </list>
    /// 
    /// <b>Upload</b> (from client to server):
    /// <list type="bullet">
    ///     <item>I【Input】</item>
    ///     <item>R【Request】</item>
    ///     <item>D【Data】</item>
    /// </list>
    /// 
    /// <b>Download</b> (from server to client):
    /// <list type="bullet">
    ///     <item>S【from Server】</item>
    /// </list>
    /// </summary>
    public class Pkt : IcePacketBase
    {

    }
}
