using System;

namespace IceEngine.Networking
{
    #region Server
    /// <summary>
    /// Called on the server when establishing tcp connection
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class TCPServerConnectedAttribute : Attribute { }
    /// <summary>
    /// Called on the server when disconnected
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class TCPServerDisconnectedAttribute : Attribute { }
    /// <summary>
    /// Called on the server to process tcp packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class TCPProcessAttribute : Attribute { }
    /// <summary>
    /// Called on the server to process udp packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class UDPProcessAttribute : Attribute { }
    #endregion

    // --------------------------------------------------

    #region Client
    /// <summary>
    /// Called on the client when establishing tcp connection
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class TCPConnectedAttribute : Attribute { }
    /// <summary>
    /// Called on the client when disconnected
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class TCPDisconnectedAttribute : Attribute { }
    /// <summary>
    /// Called on the client to receive tcp packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class TCPReceiveAttribute : Attribute { }
    /// <summary>
    /// Called on the client to receive udp packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] public class UDPReceiveAttribute : Attribute { }
    #endregion
}
