using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IceEngine.Networking.Framework
{
    public abstract class ClientBase
    {
        #region Configuration

        protected abstract IPAddress LocalIPAddress { get; }
        public IPAddress ServerIPAddress { get; private set; }
        protected abstract int ServerTCPPort { get; }
        protected abstract int ClientUDPPort { get; }
        protected abstract int InitialBufferSize { get; }
        protected abstract byte MagicByte { get; }

        // Methods
        protected abstract void CallLog(string message);
        protected abstract void CallShutdownClient();

        // Events
        protected abstract void CallConnection();
        protected abstract void CallDisconnection();
        protected abstract void CallReceive(Pkt pkt);
        protected abstract void CallUDPReceive(Pkt pkt, IPEndPoint remote);
        #endregion

        #region Instance
        bool isDestroyed = false;

        public void Destroy()
        {
            if (isDestroyed)
            {
                Log("Disposed.");
                return;
            }
            isDestroyed = true;

            Log("Destroy");
            CloseUDP();
            StopTCPConnecting();
        }

        public ClientBase()
        {
            try
            {
                buffer = new byte[InitialBufferSize];
                bufferMagic = new byte[1] { MagicByte };
                Log("Client activated……");
            }
            catch (Exception ex)
            {
                Log(ex);
                CallShutdownClient();
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
        public void OpenUDP()
        {
            if (udpClient != null)
            {
                Log("UDP already opened!");
                return;
            }
            while (true)
            {
                try
                {
                    udpClient = new UdpClient(ClientUDPPort, AddressFamily.InterNetwork) { EnableBroadcast = true };
                    break;
                }
                catch (SocketException ex)
                {
                    Log(ex);
                    throw ex;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    CloseUDP();
                    throw ex;
                }
            }
            udpReceiveThread = new Thread(UDPReceiveThread);
            udpReceiveThread.Start();
        }
        public void CloseUDP()
        {
            if (udpClient is null)
            {
                Log("UDP already closed!");
                return;
            }
            udpReceiveThread?.Abort();
            udpClient?.Close();
            udpClient = null;
        }
        public void UDPSend(Pkt pkt, IPEndPoint remote)
        {
            if (udpClient
                is null)
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
        Thread udpReceiveThread;

        void UDPReceiveThread()
        {
            Log("Start receiving udp packet……:" + ClientUDPPort);
            while (true)
            {
                try
                {
                    IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, ClientUDPPort);
                    byte[] buffer = udpClient.Receive(ref remoteIP);
                    // Block --------------------------------
                    Pkt pkt = IceBinaryUtility.FromBytes(buffer) as Pkt;
                    Log($"UDPReceive{remoteIP}:{pkt}|{buffer.Hex()}");
                    CallUDPReceive(pkt, remoteIP);
                }
                catch (SocketException ex)
                {
                    Log(ex);
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
            while (true)
            {
                try
                {
                    client = new TcpClient(AddressFamily.InterNetwork);
                    break;
                }
                catch (SocketException ex)
                {
                    Log(ex);
                    if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        // 系统自动分配的port被占用，理论上不可能出现这种情况
                        throw ex;
                    }
                    else throw ex;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    client.Close();
                    client = null;
                    throw ex;
                }
            }
            ServerIPAddress = serverIPAddress;
            connectThread = new Thread(ConnectThread);
            connectThread.Start();
        }
        public void StopTCPConnecting()
        {
            if (IsConnected) CallDisconnection();
            else if (client is null)
            {
                Log("Connection already closed!");
                return;
            }
            connectThread?.Abort();
            receiveThread?.Abort();
            stream?.Close();
            client?.Close();
            client = null;
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
        Thread connectThread;
        NetworkStream stream;
        Thread receiveThread;
        byte[] buffer;
        readonly byte[] bufferMagic;

        void ConnectThread()
        {
            do
            {
                Log($"Connecting……|ip:{LocalIPAddress}:{Port}|remote:{ServerIPAddress}:{ServerTCPPort}");
                try
                {
                    client.Connect(new IPEndPoint(ServerIPAddress, ServerTCPPort));
                    // Block --------------------------------
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
                        client.Close();
                        client = null;
                        return;
                    }
                    Thread.Sleep(1000);
                    continue;
                }
                catch (ThreadAbortException)
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
            receiveThread = new Thread(ReceiveThread);
            receiveThread.Start();
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
                if (count <= 0) throw new Exception("Disconnected from server!");

                offset += count;
            }
        }
        void ReceiveThread()
        {
            try
            {
                ReadStream(4);
                NetId = buffer.ToInt32();
                CallConnection();

                while (true)
                {
                    // Magic Byte
                    do
                    {
                        int count = stream.Read(buffer, 0, 1);
                        // Block --------------------------------
                        if (count <= 0) throw new Exception("Disconnected from server!");
                    }
                    while (buffer[0] != MagicByte);

                    // Length
                    ReadStream(2);
                    ushort length = buffer.ToUInt16();

                    // Data
                    ReadStream(length);
                    Pkt pkt = IceBinaryUtility.FromBytes(buffer) as Pkt;
                    CallReceive(pkt);
                    Log($"Receive{client.Client.LocalEndPoint}:{pkt}|{buffer.Hex(0, length)}");
                }
            }
            catch (ThreadAbortException)
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
