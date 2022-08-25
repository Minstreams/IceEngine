using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System.IO;
using System.Text.RegularExpressions;
using IceEditor.Framework;

namespace IceEditor
{
    public class TestWindow : IceEditorWindow
    {
        #region StyleMap
        public void TestTTTT344() { }
        #endregion

        [MenuItem("测试/测试！")]
        static void OpenWindow() => GetWindow<TestWindow>();

        protected override string Title => "测试";

        IceGUIDirection dir;
        protected override void OnEnable()
        {
            Pack.SetBool("开发者模式", EditorPrefs.GetBool("DeveloperMode"));
            base.OnEnable();
        }
        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var mainRect, out var subRect, "TTT", 64, dir))
            {
                using (Area(mainRect))
                {
                    var input = TextField("Input", "public void TestTTTT2() { }");
                    if (Button("Save"))
                    {
                        WriteToRegion("StyleMap", input);
                    }
                }
                //StyleBox(subRect, GetStyle("sgsdg"));
                using (Area(subRect))
                {
                    if (Button("Sub")) EditorWindow.GetWindow<TestWindow2>();

                    if (Button("TestString1"))
                    {
                        LogImportant("asad".IsNullOrEmpty() ? "true" : "false");
                    }
                    if (Button("TestString2"))
                    {
                        LogImportant("".IsNullOrEmpty() ? "true" : "false");
                    }
                    if (Button("TestString3"))
                    {
                        LogImportant(((string)null).IsNullOrEmpty() ? "true" : "false");
                    }
                    using (GUICHECK)
                    {
                        bool m = IceToggle("开发者模式");
                        if (GUIChanged)
                            EditorPrefs.SetBool("DeveloperMode", m);
                    }
                }
            }
        }
        protected override void OnDebugGUI(Rect position)
        {
            EnumPopup(ref dir);
            base.OnDebugGUI(position);
        }

        void WriteToRegion(string regionName, string content, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = null)
        {
            IceEditorUtility.WriteToFileRegion(callerFilePath, regionName, content.Split('\n'));
        }
    }
}
