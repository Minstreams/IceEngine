using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

public class TestWindow2 : IceEditorWindow
{
    [MenuItem("IceEditor/Test2")]
    public static void OpenWindow() => GetWindow<TestWindow2>();

    public override GUIContent TitleContent => new GUIContent("测试2");

    protected override void OnWindowGUI(Rect position)
    {
        Label("This is TestWindow2!!");
    }
}
