using IceEngine;
using IceEngine.Networking;
using IceEngine.Networking.Framework;
using System.Net;
using UnityEngine;
using static Ice.Network;

public class AWDASD : NetworkObject
{
    void Start()
    {
        LaunchServer();
        LaunchClient();
        ClientOpenUDP();
        ServerOpenTCP();
        ClientConnectTo(LocalIPAddressList[0]);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        ClientDisconnect();
        ServerCloseTCP();
        ClientCloseUDP();
        ShutdownClient();
        ShutdownServer();
    }
    [Button]
    public void SetId()
    {
        ID = Ice.Network.NetId;
    }
    [Button]
    public void TServerBroadcast()
    {
        ServerBroadcast(new PktTest());
    }
    [Button]
    public void TServerBroadcastId()
    {
        ServerBroadcast(new PktTestId() { id = ID });
    }
    [Button]
    public void TClientBroadcast()
    {
        ClientSend(new PktTest());
    }
    [Button]
    public void TClientBroadcastId()
    {
        ClientSend(new PktTestId() { id = ID });
    }
    [Button]
    public void TServerUDP()
    {
        ServerUDPBroadcast(new PktTest());
    }
    [Button]
    public void TServerUDPId()
    {
        ServerUDPBroadcast(new PktTestId() { id = ID });
    }
    [Button]
    public void TClientUDP()
    {
        ClientUDPSend(new PktTest(), new IPEndPoint(ServerIPAddress, Ice.Network.Setting.serverUDPPort));
    }
    [Button]
    public void TClientUDPId()
    {
        ClientUDPSend(new PktTestId() { id = ID }, new IPEndPoint(ServerIPAddress, Ice.Network.Setting.serverUDPPort));
    }

    [TCPReceive]
    public void OnRec(PktTest pkt)
    {
        Debug.Log("TCPReceive" + pkt.data.ToString());
    }
    [TCPReceive]
    public void OnRec(PktTestId pkt)
    {
        Debug.Log("TCPReceiveId" + pkt.data.ToString());
    }
    [TCPProcess]
    public void OnProc(PktTest pkt, ServerBase.Connection conn)
    {
        Debug.Log("TCPProcess" + pkt.data.ToString() + "|" + conn.NetId);
    }
    [TCPProcess]
    public void OnProc(PktTestId pkt, ServerBase.Connection conn)
    {
        Debug.Log("TCPProcessId" + pkt.data.ToString() + "|" + conn.NetId);
    }
    [UDPReceive]
    public void OnRec(PktTest pkt, IPEndPoint remote)
    {
        Debug.Log("UDPReceive" + pkt.data.ToString() + "|" + remote.ToString());
    }
    [UDPReceive]
    public void OnRec(PktTestId pkt, IPEndPoint remote)
    {
        Debug.Log("UDPReceiveId" + pkt.data.ToString() + "|" + remote.ToString());
    }
    [UDPProcess]
    public void OnProc(PktTest pkt, IPEndPoint remote)
    {
        Debug.Log("UDPProcess" + pkt.data.ToString() + "|" + remote.ToString());
    }
    [UDPProcess]
    public void OnProc(PktTestId pkt, IPEndPoint remote)
    {
        Debug.Log("UDPProcessId" + pkt.data.ToString() + "|" + remote.ToString());
    }
}

public class PktTest : Pkt
{
    public float data = 1.1231232f;
}
public class PktTestId : PktId
{
    public float data = 3.43432f;
}
