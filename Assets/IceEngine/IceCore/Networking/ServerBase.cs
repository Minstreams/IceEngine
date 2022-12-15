using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IceEngine.Threading;

namespace IceEngine.Networking.Framework
{
    public abstract class ServerBase
    {
        #region Configuration
        protected abstract IPAddress LocalIPAddress { get; }
        protected abstract IPAddress BroadcastAddress { get; }
        protected abstract int ServerUDPPort { get; }
        protected abstract int ServerTCPPort { get; }
        protected abstract int ClientUDPPort { get; }
        protected abstract int LocalTCPPort { get; }
        protected abstract byte MagicByte { get; }
        protected virtual int InitialBufferSize => 2048;

        // Methods
        protected abstract void CallLog(string message);

        // Events
        protected abstract void CallDestroy();
        protected virtual void CallUDPProcess(Pkt pkt, IPEndPoint remote) => IceNetworkUtility.CallUDPProcess(pkt, remote);
        protected virtual void CallProcess(Pkt pkt, Connection connection) => IceNetworkUtility.CallProcess(pkt, connection);
        protected virtual void CallServerConnection(Connection connection) => IceNetworkUtility.CallServerConnection(connection);
        protected virtual void CallServerDisconnection(Connection connection) => IceNetworkUtility.CallServerDisconnection(connection);
        #endregion

        #region Instance
        static ServerBase ServerInstance { get; set; }

        // interface
        public void Destroy()
        {
            if (isDestroyed)
            {
                Log("already disposed.");
                return;
            }
            isDestroyed = true;

            CloseUDP();
            CloseTCP();

            CallDestroy();

            Log("Destroyed.");
        }

        // inner code
        bool isDestroyed;
        public ServerBase()
        {
            ServerInstance = this;
            Log("Server Activated");
        }
        ~ServerBase()
        {
            Log("~Server");
            Destroy();
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("DEBUG")]
        protected void Log(object msg)
        {
            CallLog("[Server]" + msg.ToString());
        }
        protected void Log(SocketException ex)
        {
            CallLog("[Server Exception]" + ex.GetType().Name + "|" + ex.SocketErrorCode + ":" + ex.Message + "\n" + ex.StackTrace);
        }
        protected void Log(Exception ex)
        {
            CallLog("[Server Exception]" + ex.GetType().Name + ":" + ex.Message + "\n" + ex.StackTrace);
        }
        #endregion

        #region UDP

        #region Interface
        public bool IsUdpOn => udpClient is not null;
        public void OpenUDP()
        {
            if (IsUdpOn)
            {
                Log("UDP already opened!");
                return;
            }

            udpClient = new UdpClient(ServerUDPPort, AddressFamily.InterNetwork)
            {
                EnableBroadcast = true
            };
            udpReceiveThread = new IceThread(UDPReceiveThread);
            Log("UDP opened");
        }
        public void CloseUDP()
        {
            if (!IsUdpOn)
            {
                Log("UDP already closed!");
                return;
            }

            udpReceiveThread?.Dispose();
            udpReceiveThread = null;
            udpClient.Close();
            udpClient = null;
            Log("UDP closed");
        }

        public void UDPSend(Pkt pkt, IPEndPoint remote)
        {
            byte[] data = IceBinaryUtility.ToBytes(pkt);
            udpClient.Send(data, data.Length, remote);
            Log($"UDPSend{remote}:{pkt}|{data.Hex()}");
        }
        public void UDPBroadcast(Pkt pkt)
        {
            byte[] data = IceBinaryUtility.ToBytes(pkt);
            udpClient.Send(data, data.Length, new IPEndPoint(BroadcastAddress, ClientUDPPort));
            Log($"UDPBroadcast:{pkt}|{data.Hex()}");
        }

        #endregion

        #region PRIVATE
        UdpClient udpClient;
        IceThread udpReceiveThread;
        void UDPReceiveThread(CancellationTokenSource cancel)
        {
            Log($"Start receiving udp packet from {ClientUDPPort}……|Port: {ServerUDPPort}");
            while (true)
            {
                try
                {
                    IPEndPoint remoteIP = new(IPAddress.Any, ClientUDPPort);
                    byte[] buffer = udpClient.Receive(ref remoteIP);
                    // Block --------------------------------
                    cancel.Token.ThrowIfCancellationRequested();

                    Pkt pkt = IceBinaryUtility.FromBytes(buffer) as Pkt;
                    Log($"UDPReceive{remoteIP}:{pkt}|{buffer.Hex()}");
                    CallUDPProcess(pkt, remoteIP);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset) Log("Target ip doesn't has a udp receiver!");
                    else Log(ex);
                    continue;
                }
                catch (OperationCanceledException)
                {
                    Log("UDPReceive Thread Aborted.");
                    return;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    CloseUDP();
                    return;
                }
            }
        }
        #endregion

        #endregion

        #region TCP

        #region Connection
        /// <summary>
        /// A tcp connection on server
        /// </summary>
        public class Connection
        {
            /// <summary>
            /// Unique identification for each connection
            /// </summary>
            public int NetId { get; private set; }
            public bool IsHost => NetId == -1;
            public IPEndPoint RemoteEndPoint => _remoteEndPoint ??= client.Client.RemoteEndPoint as IPEndPoint; IPEndPoint _remoteEndPoint = null;
            public IPEndPoint UDPEndPoint => _udpEndPoint ??= new IPEndPoint(RemoteEndPoint.Address, ServerInstance.ClientUDPPort); IPEndPoint _udpEndPoint = null;

            public void Destroy()
            {
                if (isDestroyed)
                {
                    Log("Server.connection already destroyed.");
                    return;
                }
                isDestroyed = true;

                receiveThread?.Dispose();
                ServerInstance.CallServerDisconnection(this);
                ServerInstance.connections.Remove(this);
                stream?.Close();
                client?.Close();
                Log("Server.connection Destroyed.");
            }
            public void SendRaw(byte[] data)
            {
                if (isDestroyed) return;
                stream?.Write(data, 0, data.Length);
                //Log("Send to " + NetId + ": " + data.Hex());
            }
            public void Send(byte[] data)
            {
                if (isDestroyed) return;
                stream?.Write(bufferMagic, 0, 1);
                stream?.Write(IceBinaryUtility.GetBytes((ushort)data.Length), 0, 2);
                SendRaw(data);
            }
            public void Send(Pkt pkt)
            {
                Send(IceBinaryUtility.ToBytes(pkt));
                Log("Send to " + NetId + ": " + pkt);
            }

            #region PRIVATE

            readonly TcpClient client;
            readonly NetworkStream stream;
            readonly IceThread receiveThread;
            byte[] buffer = new byte[ServerInstance.InitialBufferSize];
            readonly byte[] bufferMagic = new byte[1] { ServerInstance.MagicByte };

            bool isDestroyed = false;

            public Connection(TcpClient client, int netId)
            {
                this.client = client;
                NetId = netId;
                stream = client.GetStream();

                receiveThread = new IceThread(ReceiveThread);

                // confirm the net id once connected
                SendRaw(IceBinaryUtility.GetBytes(netId));

                ServerInstance.CallServerConnection(this);

                Log("Connected!");
            }
            ~Connection()
            {
                Log("~Connection.");
                Destroy();
            }
            void ReadStream(ushort length)
            {
                if (buffer.Length < length)
                {
                    Log($"Resize the buffer to {length}");
                    buffer = new byte[length];
                }

                int offset = 0;
                while (offset < length)
                {
                    int count = stream.Read(buffer, offset, length - offset);
                    // Block --------------------------------
                    if (count <= 0) throw new Exception("Disconnected from client.");

                    offset += count;
                }
            }
            void ReceiveThread(CancellationTokenSource cancel)
            {
                try
                {
                    while (true)
                    {
                        // Magic Byte
                        do
                        {
                            int count = stream.Read(buffer, 0, 1);
                            // Block --------------------------------
                            cancel.Token.ThrowIfCancellationRequested();
                            if (count <= 0) throw new Exception("Disconnected from client.");
                        }
                        while (buffer[0] != ServerInstance.MagicByte);

                        // Length
                        ReadStream(2);
                        ushort length = buffer.ToUInt16();

                        // Data
                        ReadStream(length);
                        Pkt pkt = IceBinaryUtility.FromBytes(buffer) as Pkt;
                        Log($"Receive{client.Client.LocalEndPoint}:{pkt}|{buffer.Hex(0, length)}");
                        ServerInstance.CallProcess(pkt, this);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log("Receive Thread Aborted.");
                }
                catch (SocketException ex)
                {
                    ServerInstance.Log(ex);
                    Destroy();
                }
                catch (Exception ex)
                {
                    ServerInstance.Log(ex);
                    Destroy();
                }
            }
            void Log(string message) => ServerInstance.Log(message + $"(id:{NetId})");
            #endregion
        }
        #endregion

        #region Interface
        public bool IsTcpOn => listener is not null;
        public void OpenTCP()
        {
            if (IsTcpOn)
            {
                Log("TCP already opened!");
                return;
            }

            listener = new TcpListener(new IPEndPoint(LocalIPAddress, ServerTCPPort));
            listenThread = new IceThread(ListenThread);
            Log("TCP opened");
        }
        public void CloseTCP()
        {
            if (!IsTcpOn)
            {
                Log("TCP already closed!");
                return;
            }
            listenThread?.Dispose();
            listenThread = null;
            DisconnectAll();
            listener?.Stop();
            listener = null;
            Log("TCP closed");
        }

        public int ConnectionCount => connections.Count;
        public Connection GetConnection(int index) => connections[index];
        public void DisconnectAll()
        {
            for (int i = connections.Count - 1; i >= 0; --i) connections[i].Destroy();
            connections?.Clear();
            Log("All connections closed!");
        }

        public void Broadcast(byte[] data) => connections.ForEach(conn => conn.Send(data));
        public void Broadcast(Pkt pkt) => Broadcast(IceBinaryUtility.ToBytes(pkt));
        #endregion

        #region PRIVATE
        readonly List<Connection> connections = new();
        TcpListener listener;
        IceThread listenThread;
        int connectionCounter = 0;

        void ListenThread(CancellationTokenSource cancel)
        {
            try
            {
                listener.Start();
                while (true)
                {
                    Log("Listening……:" + ServerTCPPort);
                    var client = listener.AcceptTcpClient();
                    // Block --------------------------------
                    cancel.Token.ThrowIfCancellationRequested();
                    connections.Add(new Connection(client, NewTcpId(client.Client.RemoteEndPoint as IPEndPoint)));
                }
            }
            catch (OperationCanceledException)
            {
                Log("Listen Thread Aborted");
            }
            catch (Exception ex)
            {
                Log(ex);
                listener?.Stop();
                listener = null;
            }
        }
        int NewTcpId(IPEndPoint ip)
        {
            if (ip.Address.Equals(LocalIPAddress) && ip.Port.Equals(LocalTCPPort)) return -1;
            // generate a new net id
            return ++connectionCounter;
        }
        #endregion

        #endregion
    }
}
