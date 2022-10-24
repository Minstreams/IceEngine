using System;
using UnityEngine.SceneManagement;

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
            var count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                var s = SceneManager.GetSceneByBuildIndex(i);
                using (Horizontal(StlGroup))
                {
                    Label(s.path);
                    Space();
                    Label(i.ToString());
                }
            }
        }
    }
}