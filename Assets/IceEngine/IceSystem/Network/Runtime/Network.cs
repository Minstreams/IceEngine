using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

using IceEngine.Networking;
using IceEngine.Networking.Framework;

namespace Ice
{
    public sealed class Network : IceEngine.Framework.IceSystem<IceEngine.Internal.SettingNetwork>
    {
        #region Common
        public static IPAddress LocalIPAddress => LocalIPAddressList.Count > 0 ? LocalIPAddressList[0] : IPAddress.Loopback;
        public readonly static List<IPAddress> LocalIPAddressList = new();
        public static void DetectLocalIPAddress()
        {
            try
            {
                IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                LocalIPAddressList.Clear();
                foreach (var item in ipEntry.AddressList)
                {
                    if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        LocalIPAddressList.Add(item);
                        Log("LocalIP:" + item);
                    }
                }
                //string local = LocalIPAddress.ToString();
                //BroadcastAddress = IPAddress.Parse(local.Substring(0, local.LastIndexOf('.')) + ".255");
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        #region Life Circle
        static void Awake()
        {
            Log("Awake");
            _ = Setting;    // initialize Setting in main thread
        }
        static void Start()
        {
            Log("Start");
            StartCoroutine(MainThread());
            StartCoroutine(DelayThread());
        }
        static void Quitting()
        {
            Log("Quitting");

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                StopAllCoroutines();

            Server?.Destroy();
            Client?.Destroy();
        }
        #endregion
        #endregion

        #region Server

        #region Property
        public static Server Server { get; set; } = null;
        public static bool IsHost => Server != null;
        public static IPAddress BroadcastAddress { get; private set; } = IPAddress.Broadcast;
        #endregion

        #region Interface
        public static void LaunchServer()
        {
            if (Server != null)
            {
                LogError("Server already existed!");
                return;
            }
            Log("Launch Server");
            DetectLocalIPAddress();
            Server = new Server();
        }
        public static void ShutdownServer()
        {
            Log("Shutdown Server");
            Server?.Destroy();
            Server = null;
        }
        public static void ServerOpenTCP() => Server?.OpenTCP();
        public static void ServerCloseTCP() => Server?.CloseTCP();
        public static void ServerDisconnectAll() => Server?.DisconnectAll();
        public static void ServerSend(Pkt pkt, ServerBase.Connection connection) => connection.Send(pkt);
        public static void ServerBroadcast(Pkt pkt) => Server?.Broadcast(pkt);
        public static void ServerUDPSend(Pkt pkt, IPEndPoint endPoint) => Server?.UDPSend(pkt, endPoint);
        public static void ServerUDPBroadcast(Pkt pkt) => Server?.UDPBroadcast(pkt);
        #endregion

        #region Processors
        /// <summary>
        /// for server to process udp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt, IPEndPoint>> udpProcessors = new();
        /// <summary>
        /// for server to process udp packets from client with given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId, IPEndPoint>> udpIdProcessors = new();
        /// <summary>
        /// for server to process tcp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt, ServerBase.Connection>> tcpProcessors = new();
        /// <summary>
        /// for server to process tcp packets from client with given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId, ServerBase.Connection>> tcpIdProcessors = new();

        public static event Action<ServerBase.Connection> OnServerConnection;
        public static event Action<ServerBase.Connection> OnServerDisconnection;

        public static void ProcessUDPPacket(Type t, Action<Pkt, IPEndPoint> processor)
        {
            if (!udpProcessors.ContainsKey(t)) udpProcessors.Add(t, null);
            udpProcessors[t] += processor;
        }
        public static void StopProcessUDPPacket(Type t, Action<Pkt, IPEndPoint> processor)
        {
            if (udpProcessors.ContainsKey(t))
            {
                udpProcessors[t] -= processor;
            }
        }
        public static void ProcessUDPPacketFromId(int id, Action<PktId, IPEndPoint> processor)
        {
            if (!udpIdProcessors.ContainsKey(id)) udpIdProcessors.Add(id, null);
            udpIdProcessors[id] += processor;
        }
        public static void StopProcessUDPPacketFromId(int id, Action<PktId, IPEndPoint> processor)
        {
            if (udpIdProcessors.ContainsKey(id))
            {
                udpIdProcessors[id] -= processor;
            }
        }
        public static void ProcessPacket(Type t, Action<Pkt, ServerBase.Connection> processor)
        {
            if (!tcpProcessors.ContainsKey(t)) tcpProcessors.Add(t, null);
            tcpProcessors[t] += processor;
        }
        public static void StopProcessPacket(Type t, Action<Pkt, ServerBase.Connection> processor)
        {
            if (tcpProcessors.ContainsKey(t))
            {
                tcpProcessors[t] -= processor;
            }
        }
        public static void ProcessPacketFromId(int id, Action<PktId, ServerBase.Connection> processor)
        {
            if (!tcpIdProcessors.ContainsKey(id)) tcpIdProcessors.Add(id, null);
            tcpIdProcessors[id] += processor;
        }
        public static void StopProcessPacketFromId(int id, Action<PktId, ServerBase.Connection> processor)
        {
            if (tcpIdProcessors.ContainsKey(id))
            {
                tcpIdProcessors[id] -= processor;
            }
        }

        public static void CallServerConnection(ServerBase.Connection connection) => OnServerConnection?.Invoke(connection);
        public static void CallServerDisconnection(ServerBase.Connection connection) => OnServerDisconnection?.Invoke(connection);
        public static void CallUDPProcess(Pkt pkt, IPEndPoint remote)
        {
            if (pkt is null) return;
            if (pkt is PktId pktId)
            {
                if (udpIdProcessors.ContainsKey(pktId.id))
                {
                    udpIdProcessors[pktId.id]?.Invoke(pktId, remote);
                }
            }
            else
            {
                var t = pkt.GetType();
                if (udpProcessors.ContainsKey(t))
                {
                    udpProcessors[t]?.Invoke(pkt, remote);
                }
            }
        }
        public static void CallProcess(Pkt pkt, ServerBase.Connection connection)
        {
            if (pkt is null) return;
            if (pkt is PktId pktId)
            {
                if (tcpIdProcessors.ContainsKey(pktId.id))
                {
                    tcpIdProcessors[pktId.id]?.Invoke(pktId, connection);
                }
            }
            else
            {
                var t = pkt.GetType();
                if (tcpProcessors.ContainsKey(t))
                {
                    tcpProcessors[t]?.Invoke(pkt, connection);
                }
            }
        }
        #endregion

        #endregion

        #region Client

        #region Property
        public static Client Client { get; set; } = null;
        public static bool IsConnected => Client?.IsConnected ?? false;
        public static int NetId => Client?.NetId ?? 0;
        public static IPAddress ServerIPAddress => Client?.ServerIPAddress ?? Setting.DefaultServerAddress;
        #endregion

        #region Interface
        public static void LaunchClient()
        {
            if (Client != null)
            {
                LogError("Client already existed!");
                return;
            }
            Log("Launch Client");
            DetectLocalIPAddress();
            Client = new Client();
        }
        public static void ShutdownClient()
        {
            Log("Shutdown Client");
            Client?.Destroy();
            Client = null;
        }
        public static void ClientOpenUDP() => Client?.OpenUDP();
        public static void ClientCloseUDP() => Client?.CloseUDP();
        public static void ClientConnectTo(IPAddress serverIPAddress) => Client?.StartTCPConnecting(serverIPAddress);
        public static void ClientConnectToDefaultServer() => Client?.StartTCPConnecting(Setting.DefaultServerAddress);
        public static void ClientConnectToDefaultServerDomain() => Client?.StartTCPConnecting(Setting.DefaultServerDomain);
        public static void ClientDisconnect() => Client?.StopTCPConnecting();
        public static void ClientSend(Pkt pkt)
        {
            if (Client is null || !Client.IsConnected) return;
            void Send() => Client.Send(pkt);
            CallDelay(Send);
        }
        public static void ClientUDPSend(Pkt pkt, IPEndPoint endPoint)
        {
            if (Client is null) return;
            void Send() => Client.UDPSend(pkt, endPoint);
            CallDelay(Send);
        }
        #endregion

        #region Utility
        public static int LocalTCPPort => Client?.Port ?? 0;
        #endregion

        #region Ditributors
        /// <summary>
        /// for client to receive udp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt, IPEndPoint>> udpDistributors = new();
        /// <summary>
        /// for client to receive udp packets for given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId, IPEndPoint>> udpIdDistributors = new();
        /// <summary>
        /// for client to receive tcp packets
        /// </summary>
        static readonly Dictionary<Type, Action<Pkt>> tcpDistributors = new();
        /// <summary>
        /// for client to receive tcp packets for given id
        /// </summary>
        static readonly Dictionary<int, Action<PktId>> tcpIdDistributors = new();

        public static event Action OnConnection;
        public static event Action OnDisconnection;

        public static void ListenUDPPacket(Type t, Action<Pkt, IPEndPoint> listener)
        {
            if (!udpDistributors.ContainsKey(t)) udpDistributors.Add(t, null);
            udpDistributors[t] += listener;
        }
        public static void StopListenUDPPacket(Type t, Action<Pkt, IPEndPoint> listener)
        {
            if (udpDistributors.ContainsKey(t))
            {
                udpDistributors[t] -= listener;
            }
        }
        public static void ListenUDPPacketToId(int id, Action<PktId, IPEndPoint> listener)
        {
            if (!udpIdDistributors.ContainsKey(id)) udpIdDistributors.Add(id, null);
            udpIdDistributors[id] += listener;
        }
        public static void StopListenUDPPacketToId(int id, Action<PktId, IPEndPoint> listener)
        {
            if (udpIdDistributors.ContainsKey(id))
            {
                udpIdDistributors[id] -= listener;
            }
        }
        public static void ListenPacket(Type t, Action<Pkt> listener)
        {
            if (!tcpDistributors.ContainsKey(t)) tcpDistributors.Add(t, null);
            tcpDistributors[t] += listener;
        }
        public static void StopListenPacket(Type t, Action<Pkt> listener)
        {
            if (tcpDistributors.ContainsKey(t))
            {
                tcpDistributors[t] -= listener;
            }
        }
        public static void ListenPacketToId(int id, Action<PktId> listener)
        {
            if (!tcpIdDistributors.ContainsKey(id)) tcpIdDistributors.Add(id, null);
            tcpIdDistributors[id] += listener;
        }
        public static void StopListenPacketToId(int id, Action<PktId> listener)
        {
            if (tcpIdDistributors.ContainsKey(id))
            {
                tcpIdDistributors[id] -= listener;
            }
        }

        public static void CallConnection() => OnConnection?.Invoke();
        public static void CallDisconnection() => OnDisconnection?.Invoke();
        public static void CallUDPReceive(Pkt pkt, IPEndPoint remote)
        {
            void Receive()
            {
                if (pkt is null) return;
                if (pkt is PktId pktId)
                {
                    if (udpIdDistributors.ContainsKey(pktId.id))
                    {
                        udpIdDistributors[pktId.id]?.Invoke(pktId, remote);
                    }
                }
                else
                {
                    var t = pkt.GetType();
                    if (udpDistributors.ContainsKey(t))
                    {
                        udpDistributors[t]?.Invoke(pkt, remote);
                    }
                }
            }
            CallDelay(Receive);
        }
        public static void CallReceive(Pkt pkt)
        {
            void Receive()
            {
                if (pkt is null) return;
                if (pkt is PktId pktId)
                {
                    if (tcpIdDistributors.ContainsKey(pktId.id))
                    {
                        tcpIdDistributors[pktId.id]?.Invoke(pktId);
                    }
                }
                else
                {
                    var t = pkt.GetType();
                    if (tcpDistributors.ContainsKey(t))
                    {
                        tcpDistributors[t]?.Invoke(pkt);
                    }
                }
            }
            CallDelay(Receive);
        }
        #endregion

        #endregion

        #region Thread Control

        /// <summary>
        /// Call an action in main thread
        /// </summary>
        public static void CallMainThread(Action action)
        {
            lock (threadLocker)
            {
                mainThreadActionQueue.Enqueue(action);
            }
        }
        /// <summary>
        /// Call Debug.Log in main thread
        /// </summary>
        public static void CallLog(string message) => CallMainThread(() => Log(message));

        #region PRIVATE
        readonly static Queue<Action> mainThreadActionQueue = new Queue<Action>();
        readonly static object threadLocker = new();
        static IEnumerator MainThread()
        {
            while (true)
            {
                yield return 0;
                while (mainThreadActionQueue.Count > 0) mainThreadActionQueue.Dequeue()?.Invoke();
            }
        }
        #endregion

        #endregion

        #region Latency simulation

        public static float latencyOverride = 0;

        #region PRIVATE
        readonly static Queue<Action> delayThreadActionQueue = new Queue<Action>();
        static IEnumerator DelayThread()
        {
            while (true)
            {
                yield return 0;
                while (delayThreadActionQueue.Count > 0)
                {
                    StartCoroutine(DelayToMain(delayThreadActionQueue.Dequeue()));
                }
            }
        }
        static IEnumerator DelayToMain(Action action)
        {
            yield return new WaitForSecondsRealtime(latencyOverride);
            action();
        }
        static void CallDelay(Action action)
        {
            if (latencyOverride > 0 && !IsHost)
            {
                delayThreadActionQueue.Enqueue(action);
            }
            else action();
        }
        #endregion

        #endregion
    }
}