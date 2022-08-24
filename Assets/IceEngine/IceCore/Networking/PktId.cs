using IceEngine.Framework;

namespace IceEngine.Networking.Framework
{
    /// <summary>
    /// Base class of packet tagged with netid, sent by server 
    /// <para>
    /// Prefix reference of subclass: Si【from Server with netId】
    /// </para>
    /// </summary>
    public class PktId : Pkt
    {
        public int id;
    }
}
