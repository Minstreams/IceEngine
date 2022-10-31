using System;
using UnityEngine;
using UnityEditor;

using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using Sys = Ice.Network;
using SysSetting = IceEngine.Internal.SettingNetwork;

namespace IceEditor.Internal
{
    internal sealed class NetworkDrawer : Framework.IceSystemDrawer<Sys, SysSetting>
    {
        string serverStatus = "";
        string clientStatus = "";
        public override void OnToolBoxGUI()
        {
            using (GROUP) using (SectionFolder("Config"))
            {
                using (GUICHECK)
                {
                    IntField("Client UDP Port", ref Setting.clientUDPPort);
                    IntField("Server UDP Port", ref Setting.serverUDPPort);
                    IntField("Server TCP Port", ref Setting.serverTCPPort);
                    TextField("Default Server IP", ref Setting.defaultServerIPString);
                    TextField("Default Server Domain", ref Setting.defaultServerDomain);
                    if (GUIChanged)
                    {
                        EditorUtility.SetDirty(Setting);
                    }
                }
            }
            using (Horizontal(StlGroup)) using (LabelWidth(80))
            {
                Sys.LatencyOverrideMS = _FloatField("延迟模拟(ms)", Sys.LatencyOverrideMS);
                Label(Sys.LatencySimulationEnabled ? "已开启".Color(Setting.themeColor) : "未开启".Color(Color.gray), StlIce);
            }
            using (GROUP) using (SectionFolder("Runtime")) using (Disable(!EditorApplication.isPlaying))
            {
                using (BOX)
                {
                    using (HORIZONTAL)
                    {
                        SectionHeader("Server");
                        Space();
                        Label(serverStatus);
                    }
                    using (Folder("Server"))
                    {
                        var ser = Sys.Server;
                        if (ser == null)
                        {
                            serverStatus = "none";
                            if (Button("Launch")) Sys.LaunchServer();
                        }
                        else
                        {
                            bool bTcpOn = ser.IsTcpOn;
                            if (bTcpOn)
                            {
                                int count = ser.ConnectionCount;
                                serverStatus = $"connections {count} - tcp on - running";
                                for (int i = 0; i < count; ++i)
                                {
                                    var conn = ser.GetConnection(i);
                                    using (GROUP) using (SectionFolder($"Conn {i}", labelOverride: $"Connection {conn.NetId}"))
                                    {
                                        if (Button("Disconnect")) conn.Destroy();
                                    }
                                }
                            }
                            else
                            {
                                serverStatus = $"tcp off - running";
                            }

                            _TextField("Local Address", Sys.LocalIPAddress.ToString());

                            if (bTcpOn)
                            {
                                if (Button("CloseTCP")) ser.CloseTCP();
                            }
                            else
                            {
                                if (Button("OpenTCP")) ser.OpenTCP();
                            }
                            if (Button("Shutdown")) Sys.ShutdownServer();
                        }
                    }
                }
                using (BOX)
                {
                    using (HORIZONTAL)
                    {
                        SectionHeader("Client");
                        Space();
                        Label(clientStatus);
                    }
                    using (Folder("Client"))
                    {
                        var cli = Sys.Client;
                        if (cli == null)
                        {
                            clientStatus = "none";
                            if (Button("Launch")) Sys.LaunchClient();
                        }
                        else
                        {
                            bool bConnected = cli.IsConnected;
                            bool bUdpOn = cli.IsUdpOn;
                            if (bConnected)
                            {
                                clientStatus = $"connected - udp {(bUdpOn ? "on" : "off")} - running";
                                _TextField("Server Address", cli.ServerIPAddress?.ToString());
                                if (Button("Disconnect")) cli.StopTCPConnecting();
                            }
                            else
                            {
                                clientStatus = $"disconncected - udp {(bUdpOn ? "on" : "off")} - running";
                                if (Button("ConnectToDefaultServerIP")) Sys.ClientConnectToDefaultServerIP();
                                if (Button("ConnectToDefaultServerDomain")) Sys.ClientConnectToDefaultServerDomain();
                                if (Button("Disconnect")) cli.StopTCPConnecting();
                            }

                            if (bUdpOn)
                            {
                                if (Button("CloseUDP")) cli.CloseUDP();
                            }
                            else
                            {
                                if (Button("OpenUDP")) cli.OpenUDP();
                            }
                            if (Button("Shutdown")) Sys.ShutdownClient();
                        }
                    }
                }
            }
        }
    }
}