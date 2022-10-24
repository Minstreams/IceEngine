using System;

using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using Sys = Ice.Save;
using SysSetting = IceEngine.Internal.SettingSave;

namespace IceEditor.Internal
{
    internal sealed class SaveDrawer : Framework.IceSystemDrawer<Sys, SysSetting>
    {
        public override void OnToolBoxGUI()
        {
            using (HORIZONTAL)
            {
                using (GROUP)
                {
                    Label("Json", StlBoldLabel);
                    Label($"Format: {Sys.Json.Format.BodyName}");
                }
                using (GROUP)
                {
                    Label("Binary", StlBoldLabel);
                }
            }
        }
    }
}