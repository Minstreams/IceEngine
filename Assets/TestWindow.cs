using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor
{
    public class TestWindow : IceEditorWindow
    {
        [MenuItem("IceEditor/测试！")]
        static void OpenWindow() => GetWindow<TestWindow>();
        public override GUIContent TitleContent => new GUIContent("测试！");

        IceGUIDirection dir;
        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var mainRect, out var subRect, "TTT", 64, dir))
            {
                using (Area(mainRect))
                {
                    Button("Main");
                }
                using (Area(subRect))
                {
                    Button("Sub");
                }
            }
        }
        protected override void OnDebugGUI(Rect position)
        {
            EnumPopup(ref dir);
            base.OnDebugGUI(position);
        }
    }
}
