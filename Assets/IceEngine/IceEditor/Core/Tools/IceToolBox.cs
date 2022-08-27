using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    public class IceToolBox : IceEditorWindow
    {
        [MenuItem("IceEngine/Ice Tool Box #F1", priority = 0)]
        public static void OpenWindow() => GetWindow<IceToolBox>();

        #region SubSystem Management
        public static List<Type> SubSystemList => Ice.Island.SubSystemList;

        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (GROUP) using (SectionFolder("Sub Systems"))
            {
                foreach (var sub in SubSystemList)
                {
                    Label(sub.Name);
                }
            }

            using (GROUP)
            {
                Label(IceEditorUtility.GetSelectPath());
            }
        }
        private void Update()
        {
            Repaint();
        }
    }
}
