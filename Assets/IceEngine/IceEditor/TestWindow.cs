using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
            using (CHANGECHECK)
            {
                EditorGUI.BeginChangeCheck();
                using (CHANGECHECK)
                {
                    Button("Anything1");
                    if (Changed) LogImportant("Check 1!");
                }
                using (CHANGECHECK)
                {
                    Button("Anything2");
                    if (Changed) LogImportant("Check 2!");
                }
                if (Changed) LogImportant("Check All!");
            }
        }
    }
}
