using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    public class IceToolBox : IceEditorWindow
    {
        #region 定制
        protected override string Title => "Ice工具箱";
        [MenuItem("IceEngine/Ice工具箱 #F1", false, 0)]
        public static void OpenWindow() => GetWindow<IceToolBox>();

        [ToolbarGUICallback(ToolbarGUIPosition.Right)]
        static void OnToolbarGUI()
        {
            if (IceButton("-◈-".Size(13), focusedWindow is IceToolBox, "Ice工具箱")) OpenWindow();
        }
        #endregion

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

            using (GROUP) using (SectionFolder("代码生成")) using (LabelWidth(60))
            {
                using (GROUP) using (SectionFolder("子系统"))
                {
                    TextField("系统名字");
                    if (Button("生成"))
                    {

                    }
                }
            }

            Space();

            using (GROUP)
            {
                Label("Selected Path:");
                Label(IceEditorUtility.GetSelectPath());
            }
        }
        private void Update()
        {
            Repaint();
        }
    }
}
