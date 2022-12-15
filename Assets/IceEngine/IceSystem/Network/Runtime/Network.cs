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
        }
        public static void ServerOpenUDP() => Server?.OpenUDP();
        public static void ServerCloseUDP() => Server?.CloseUDP();
        public static void ServerOpenTCP() => Server?.OpenTCP();
        public static void ServerCloseTCP() => Server?.CloseTCP();
        public static void ServerDisconnectAll() => Server?.DisconnectAll();
        public static void ServerSend(Pkt pkt, ServerBase.Connection connection) => connection.Send(pkt);
        public static void ServerBroadcast(Pkt pkt) => Server?.Broadcast(pkt);
        public static void ServerUDPSend(Pkt pkt, IPEndPoint endPoint) => Server?.UDPSend(pkt, endPoint);
        public static void ServerUDPBroadcast(Pkt pkt) => Server?.UDPBroadcast(pkt);
        #endregion

        #region Events
        public static void CallServerDestroy() => Server = null;
        #endregion

        #endregion

        #region Client

        #region Property
        public static Client Client { get; set; } = null;
        public static bool IsConnected => Client?.IsConnected ?? false;
        public static int NetId => Client?.NetId ?? 0;
        public static int LocalTCPPort => Client?.Port ?? 0;
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
        }
        public static void ClientOpenUDP() => Client?.OpenUDP();
        public static void ClientCloseUDP() => Client?.CloseUDP();
        public static void ClientConnectTo(IPAddress serverIPAddress) => Client?.StartTCPConnecting(serverIPAddress);
        public static void ClientConnectToDefaultServerIP() => Client?.StartTCPConnecting(Setting.DefaultServerAddress);
        public static void ClientConnectToDefaultServerDomain() => Client?.StartTCPConnecting(Setting.DefaultServerDomain);
        public static void ClientDisconnect() => Client?.StopTCPConnecting();
        public static void ClientSend(Pkt pkt)
        {
            if (Client is null || !Client.IsConnected) return;
            if (!LatencySimulationEnabled) Client.Send(pkt);
            else CallDelay(() => Client.Send(pkt));
        }
        public static void ClientUDPSend(Pkt pkt, IPEndPoint endPoint)
        {
            if (Client is null) return;
            if (!LatencySimulationEnabled) Client.UDPSend(pkt, endPoint);
            else CallDelay(() => Client.UDPSend(pkt, endPoint));
        }
        #endregion

        #region Events
        public static void CallClientDestroy() => Client = null;
        public static void CallUDPReceive(Pkt pkt, IPEndPoint remote)
        {
            if (!LatencySimulationEnabled) IceNetworkUtility.CallUDPReceive(pkt, remote);
            else CallDelay(() => IceNetworkUtility.CallUDPReceive(pkt, remote));
        }
        public static void CallReceive(Pkt pkt)
        {
            if (!LatencySimulationEnabled) IceNetworkUtility.CallReceive(pkt);
            else CallDelay(() => IceNetworkUtility.CallReceive(pkt));
        }
        #endregion

        #endregion


        #region Interface
        /// <summary>
        /// Call Debug.Log in main thread
        /// </summary>
        public static void CallLog(string message) => CallMainThread(() => Log(message));
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

        #region PRIVATE
        readonly static Queue<Action> mainThreadActionQueue = new();
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

        public static float LatencyOverrideMS { get; set; } = 0;
        public static bool LatencySimulationEnabled => LatencyOverrideMS > 0 && !IsHost;

        public static void CallDelay(Action action)
        {
            if (LatencySimulationEnabled)
            {
                lock (delayLocker)
                {
                    delayThreadActionQueue.Enqueue(action);
                }
            }
            else action();
        }

        #region PRIVATE
        readonly static Queue<Action> delayThreadActionQueue = new();
        readonly static object delayLocker = new();
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
            yield return new WaitForSecondsRealtime(LatencyOverrideMS / 1000.0f);
            action();
        }
        #endregion

        #endregion
    }
}