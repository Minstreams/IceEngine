using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System.IO;
using System.Text.RegularExpressions;

namespace IceEditor
{
    public class TestWindow : IceEditorWindow
    {
        #region StyleMap
        public void TestTTTT344() { }
        #endregion

        [MenuItem("IceEditor/测试！")]
        static void OpenWindow() => GetWindow<TestWindow>();

        protected override string Title => "测试";

        IceGUIDirection dir;
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
