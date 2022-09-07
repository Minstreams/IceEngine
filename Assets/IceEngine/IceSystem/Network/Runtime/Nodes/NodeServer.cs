using System;
using System.Net;
using UnityEngine;
using IceEngine.Framework;
using IceEngine.Networking.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Ice/Network/Server")]
    public class NodeServer : IceprintNode
    {
        // Ports
        [IceprintPort] public void Launch() => Ice.Network.LaunchServer();
        [IceprintPort] public void Shutdown() => Ice.Network.ShutdownServer();
        [IceprintPort] public void OpenTCP() => Ice.Network.ServerOpenTCP();
        [IceprintPort] public void CloseTCP() => Ice.Network.ServerCloseTCP();
        [IceprintPort] public void Broadcast(Pkt pkt) => Ice.Network.ServerBroadcast(pkt);
        [IceprintPort] public void Send(Pkt pkt, ServerBase.Connection conn) => Ice.Network.ServerSend(pkt, conn);
        [IceprintPort] public void UDPBroadcast(Pkt pkt) => Ice.Network.ServerUDPBroadcast(pkt);
        [IceprintPort] public void UDPSend(Pkt pkt, IPEndPoint remote) => Ice.Network.ServerUDPSend(pkt, remote);
    }
}
