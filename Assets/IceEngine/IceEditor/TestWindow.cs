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

        protected override void OnDebugGUI(Rect position)
        {

        }
        protected override void OnWindowGUI(Rect position)
        {
            IntField("Play");
            IntField("Play2");
            Vector2Field("Play2");
            Vector3Field("Play2");
            Vector4Field("Play2");
            Vector2IntField("Play2");
            Vector3IntField("Play2");
        }
    }
}
