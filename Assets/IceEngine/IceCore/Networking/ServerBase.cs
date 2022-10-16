using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        protected abstract int InitialBufferSize { get; }
        protected abstract byte MagicByte { get; }

        // Methods
        protected abstract void CallLog(string message);
        protected abstract void CallShutdownServer();

        // Events
        protected abstract void CallUDPProcess(Pkt pkt, IPEndPoint remote);
        protected abstract void CallProcess(Pkt pkt, Connection connection);
        protected abstract void CallServerConnection(Connection connection);
        protected abstract void CallServerDisconnection(Connection connection);
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

            udpReceiveThread?.Abort();
            udpClient.Close();

            DisconnectAll();
            CloseTCP();

            Log("Destroyed.");
        }

        // inner code
        bool isDestroyed;
        public ServerBase()
        {
            ServerInstance = this;
            udpClient = new UdpClient(ServerUDPPort, AddressFamily.InterNetwork)
            {
                EnableBroadcast = true
            };
            udpReceiveThread = new Thread(UDPReceiveThread);
            udpReceiveThread.Start();

            Log("Server Activated……|UDP:" + ServerUDPPort);
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
        readonly UdpClient udpClient;
        readonly Thread udpReceiveThread;

        void UDPReceiveThread()
        {
            Log("Start receiving udp packet……");
            while (true)
            {
                try
                {
                    IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, ClientUDPPort);
                    byte[] buffer = udpClient.Receive(ref remoteIP);
                    // Block --------------------------------
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
                catch (ThreadAbortException)
                {
                    Log("UDPReceive Thread Aborted.");
                    return;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    CallShutdownServer();
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
                    Log("Server.connection already disposed.");
                    return;
                }
                isDestroyed = true;

                ServerInstance.CallServerDisconnection(this);
                stream?.Close();
                client?.Close();
                Log("Server.connection Destroyed.");
                receiveThread?.Abort();
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
            readonly Thread receiveThread;
            byte[] buffer = new byte[ServerInstance.InitialBufferSize];
            readonly byte[] bufferMagic = new byte[1] { ServerInstance.MagicByte };

            bool isDestroyed = false;

            public Connection(TcpClient client, int netId)
            {
                this.client = client;
                NetId = netId;
                stream = client.GetStream();

                receiveThread = new Thread(ReceiveThread);
                receiveThread.Start();

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
            void ReceiveThread()
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
                catch (ThreadAbortException)
                {
                    Log("Receive Thread Aborted.");
                }
                catch (SocketException ex)
                {
                    ServerInstance.Log(ex);
                    ServerInstance.CloseConnection(this);
                }
                catch (Exception ex)
                {
                    ServerInstance.Log(ex);
                    ServerInstance.CloseConnection(this);
                }
            }
            void Log(string message) => ServerInstance.Log(message + $"(id:{NetId})");
            #endregion
        }
        #endregion

        #region Interface
        public Connection GetConnection(int index) => connections[index];
        public bool TcpOn => listener != null && listener.Pending();
        public void OpenTCP()
        {
            if (listener != null)
            {
                Log("TCP already opened!");
                return;
            }
            listener = new TcpListener(new IPEndPoint(LocalIPAddress, ServerTCPPort));
            listenThread = new Thread(ListenThread);
            listenThread.Start();
        }
        public void CloseTCP()
        {
            if (listener is null)
            {
                Log("TCP already closed!");
                return;
            }
            DisconnectAll();
            listener?.Stop();
            listener = null;
            listenThread?.Abort();
        }
        public void DisconnectAll()
        {
            for (int i = connections.Count - 1; i >= 0; --i) connections[i].Destroy();
            connections?.Clear();
            Log("All connections closed!");
        }
        public void CloseConnection(Connection conn)
        {
            conn.Destroy();
            connections.Remove(conn);
        }
        public void Broadcast(byte[] data) => connections.ForEach(conn => conn.Send(data));
        public void Broadcast(Pkt pkt) => Broadcast(IceBinaryUtility.ToBytes(pkt));
        #endregion

        #region PRIVATE
        readonly List<Connection> connections = new List<Connection>();
        TcpListener listener;
        Thread listenThread;
        int connectionCounter = 0;

        void ListenThread()
        {
            try
            {
                listener.Start();
                while (true)
                {
                    Log("Listening……:" + ServerTCPPort);
                    var client = listener.AcceptTcpClient();
                    // Block --------------------------------
                    connections.Add(new Connection(client, NewTcpId(client.Client.RemoteEndPoint as IPEndPoint)));
                }
            }
            catch (ThreadAbortException)
            {
                Log("Listen Thread Aborted");
            }
            catch (Exception ex)
            {
                Log(ex);
                listener?.Stop();
                listener = null;
                return;
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
