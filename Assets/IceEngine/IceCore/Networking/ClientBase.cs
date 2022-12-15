using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IceEngine.Threading;

namespace IceEngine.Networking.Framework
{
    public abstract class ClientBase
    {
        #region Configuration
        protected abstract IPAddress LocalIPAddress { get; }
        protected abstract int ServerTCPPort { get; }
        protected abstract int ClientUDPPort { get; }
        protected abstract byte MagicByte { get; }
        protected virtual int InitialBufferSize => 2048;

        // Methods
        protected abstract void CallLog(string message);

        // Events
        protected abstract void CallDestroy();
        protected virtual void CallConnection() => IceNetworkUtility.CallConnection();
        protected virtual void CallDisconnection() => IceNetworkUtility.CallDisconnection();
        protected virtual void CallReceive(Pkt pkt) => IceNetworkUtility.CallReceive(pkt);
        protected virtual void CallUDPReceive(Pkt pkt, IPEndPoint remote) => IceNetworkUtility.CallUDPReceive(pkt, remote);
        #endregion

        #region Instance
        bool isDestroyed = false;
        public void Destroy()
        {
            if (isDestroyed)
            {
                Log("already disposed.");
                return;
            }
            isDestroyed = true;

            CloseUDP();
            StopTCPConnecting();
            CallDestroy();

            Log("Destroyed");
        }

        public ClientBase()
        {
            try
            {
                buffer = new byte[InitialBufferSize];
                bufferMagic = new byte[1] { MagicByte };
                Log("Client activated");
            }
            catch (Exception ex)
            {
                Log(ex);
                Destroy();
                return;
            }
        }
        ~ClientBase()
        {
            Log("~Client");
            Destroy();
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("DEBUG")]
        protected void Log(object msg)
        {
            CallLog("[Client]" + msg.ToString());
        }
        protected void Log(SocketException ex)
        {
            CallLog("[Client Exception:" + Port + "]" + ex.GetType().Name + "|" + ex.SocketErrorCode + ":" + ex.Message + "\n" + ex.StackTrace);
        }
        protected void Log(Exception ex)
        {
            CallLog("[Client Exception]" + ex.GetType().Name + ":" + ex.Message + "\n" + ex.StackTrace);
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

            try
            {
                udpClient = new UdpClient(ClientUDPPort, AddressFamily.InterNetwork) { EnableBroadcast = true };
            }
            catch (Exception ex)
            {
                Log(ex);
                CloseUDP();
                return;
            }

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
            udpClient?.Close();
            udpClient = null;
            Log("UDP closed");
        }
        public void UDPSend(Pkt pkt, IPEndPoint remote)
        {
            if (!IsUdpOn)
            {
                Log("UDP not opened!");
                return;
            }
            byte[] data = IceBinaryUtility.ToBytes(pkt);
            udpClient.Send(data, data.Length, remote);
            Log($"UDPSend{remote}:{pkt}|{data.Hex()}");
        }
        #endregion

        #region PRIVATE
        UdpClient udpClient;
        IceThread udpReceiveThread;

        void UDPReceiveThread(CancellationTokenSource cancel)
        {
            Log("Start receiving udp packet……:" + ClientUDPPort);
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
                    CallUDPReceive(pkt, remoteIP);
                }
                catch (SocketException ex)
                {
                    Log(ex);
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
                    udpClient?.Close();
                    udpClient = null;
                    return;
                }
            }
        }
        #endregion

        #endregion

        #region TCP

        #region Interface
        public int NetId { get; private set; }
        public bool IsConnected => client != null && client.Connected;
        public int Port => (client?.Client?.LocalEndPoint as IPEndPoint)?.Port ?? 0;
        public IPAddress ServerIPAddress { get; private set; }


        /// <summary>
        /// Start connecting to given IP address (LoaclIPAddress & ServerIPAddress should be set)
        /// </summary>
        public void StartTCPConnecting(IPAddress serverIPAddress)
        {
            if (client != null)
            {
                Log("Connection already existed!");
                return;
            }

            try
            {
                client = new TcpClient(AddressFamily.InterNetwork);
            }
            catch (SocketException ex)
            {
                Log(ex);
                client?.Close();
                client = null;

                if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    // 系统自动分配的port被占用，理论上不可能出现这种情况
                }
                return;
            }
            catch (Exception ex)
            {
                Log(ex);
                client?.Close();
                client = null;
                return;
            }

            ServerIPAddress = serverIPAddress;
            connectThread = new IceThread(ConnectThread);
        }
        public void StopTCPConnecting()
        {
            if (IsConnected) CallDisconnection();
            else if (client is null)
            {
                Log("Connection already closed!");
                return;
            }
            connectThread?.Dispose();
            connectThread = null;
            receiveThread?.Dispose();
            receiveThread = null;
            stream?.Close();
            client?.Close();
            client = null;
            Log("Connection closed");
        }
        public void Send(byte[] data)
        {
            if (stream is null || !stream.CanWrite)
            {
                Log("Sending failed.");
                return;
            }
            stream?.Write(bufferMagic, 0, 1);
            stream?.Write(IceBinaryUtility.GetBytes((ushort)data.Length), 0, 2);
            stream?.Write(data, 0, data.Length);
            Log("Send: " + data.Hex());
        }
        public void Send(Pkt pkt) => Send(IceBinaryUtility.ToBytes(pkt));
        #endregion

        #region PRIVATE
        //int port;
        TcpClient client;
        IceThread connectThread;
        NetworkStream stream;
        IceThread receiveThread;
        byte[] buffer;
        readonly byte[] bufferMagic;

        void ConnectThread(CancellationTokenSource cancel)
        {
            do
            {
                Log($"Connecting……|ip:{LocalIPAddress}:{Port}|remote:{ServerIPAddress}:{ServerTCPPort}");
                try
                {
                    client.Connect(new IPEndPoint(ServerIPAddress, ServerTCPPort));
                    // Block --------------------------------
                    cancel.Token.ThrowIfCancellationRequested();
                }
                catch (SocketException ex)
                {
                    Log(ex);
                    Log("Connection failed! Reconnecting……:" + Port);
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        // 系统自动分配的port被占用，理论上不可能出现这种情况
                        throw ex;
                    }
                    else if (ex.SocketErrorCode == SocketError.AddressNotAvailable)
                    {
                        Log(ex);
                        client.Close();
                        client = null;
                        return;
                    }
                    Thread.Sleep(1000);
                    continue;
                }
                catch (OperationCanceledException)
                {
                    Log("Connect Thread Aborted.");
                    return;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    client.Close();
                    client = null;
                    return;
                }
            } while (!client.Connected);

            Log("Connected!");

            stream = client.GetStream();
            receiveThread = new IceThread(ReceiveThread);
        }
        void ReadStream(ushort length, CancellationTokenSource cancel)
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
                cancel.Token.ThrowIfCancellationRequested();
                if (count <= 0) throw new Exception("Disconnected from server!");

                offset += count;
            }
        }
        void ReceiveThread(CancellationTokenSource cancel)
        {
            try
            {
                ReadStream(4, cancel);
                NetId = buffer.ToInt32();
                CallConnection();

                while (true)
                {
                    // Magic Byte
                    do
                    {
                        int count = stream.Read(buffer, 0, 1);
                        // Block --------------------------------
                        cancel.Token.ThrowIfCancellationRequested();
                        if (count <= 0) throw new Exception("Disconnected from server!");
                    }
                    while (buffer[0] != MagicByte);

                    // Length
                    ReadStream(2, cancel);
                    ushort length = buffer.ToUInt16();

                    // Data
                    ReadStream(length, cancel);
                    Pkt pkt = IceBinaryUtility.FromBytes(buffer) as Pkt;
                    Log($"Receive{client.Client.LocalEndPoint}:{pkt}|{buffer.Hex(0, length)}");
                    CallReceive(pkt);
                }
            }
            catch (OperationCanceledException)
            {
                Log("Receive Thread Aborted.");
            }
            catch (Exception ex)
            {
                Log(ex);
                stream?.Close();
                client?.Close();
                client = null;
                CallDisconnection();
            }
        }
        #endregion

        #endregion
    }
}
