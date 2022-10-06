using System;
using IceEngine;

using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using Sys = Ice.Level;
using SysSetting = IceEngine.Internal.SettingLevel;

namespace IceEditor.Internal
{
    internal sealed class LevelDrawer : Framework.IceSystemDrawer<Sys, SysSetting>
    {
        public override void OnToolBoxGUI()
        {
            Label("Level");
        }
    }
}