using System;
using IceEngine;

using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

using Sys = Ice.Network;
using SysSetting = IceEngine.Internal.SettingNetwork;

namespace IceEditor.Internal
{
    internal sealed class NetworkDrawer : Framework.IceSystemDrawer<Sys, SysSetting>
    {
        public override void OnToolBoxGUI()
        {

        }
    }
}