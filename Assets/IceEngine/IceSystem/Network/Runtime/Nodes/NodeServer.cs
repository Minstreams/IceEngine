using System;
using System.Net;
using UnityEngine;
using IceEngine.Framework;
using IceEngine.Networking.Framework;
using Sys = Ice.Network;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Ice/Network/Server")]
    public class NodeServer : IceprintNode
    {
        // Ports
        [IceprintPort] public void Launch() => Sys.LaunchServer();
        [IceprintPort] public void Shutdown() => Sys.ShutdownServer();
        [IceprintPort] public void OpenTCP() => Sys.ServerOpenTCP();
        [IceprintPort] public void CloseTCP() => Sys.ServerCloseTCP();
        [IceprintPort] public void Broadcast(Pkt pkt) => Sys.ServerBroadcast(pkt);
        [IceprintPort] public void Send(Pkt pkt, ServerBase.Connection conn) => Sys.ServerSend(pkt, conn);
        [IceprintPort] public void UDPBroadcast(Pkt pkt) => Sys.ServerUDPBroadcast(pkt);
        [IceprintPort] public void UDPSend(Pkt pkt, IPEndPoint remote) => Sys.ServerUDPSend(pkt, remote);
    }
}
