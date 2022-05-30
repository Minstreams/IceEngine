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
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        [SerializeField] internal IceDictionary<string, GUIStyle> _stringStyleMap = new IceDictionary<string, GUIStyle>();
        string curFilePath;
        public GUIStyle GetStyle(string key, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = null)
        {
            curFilePath = callerFilePath;
            if (_stringStyleMap.TryGetValue(key, out var stl))
            {
                return stl;
            }
            return IceGUIUtility.GetStyle(key);
        }

        [MenuItem("IceEditor/测试！")]
        static void OpenWindow() => GetWindow<TestWindow>();
        public override GUIContent TitleContent => new GUIContent("测试！");

        IceGUIDirection dir;
        Regex regRegion = new Regex("[\r\n]+([ \t]+)#region StyleMap([\r\n]+).*#endregion", RegexOptions.Multiline);
        Regex regClass = new Regex("[\n\r]+([ \t]*)(public |internal |private )?class [\\w]+ ?: ?IceEditorWindow[\r\n]*[ \t]*\\{[ \t]*([\r\n]+)", RegexOptions.Multiline);
        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var mainRect, out var subRect, "TTT", 64, dir))
            {
                using (Area(mainRect))
                {
                    if (Button("Save"))
                    {

                        static string GetStyleMapCodeContent(string retStr)
                        {
                            return $"{retStr}public void TestTTTT() {{ }}";
                        }

                        var content = File.ReadAllText(curFilePath);
                        if (regRegion.IsMatch(content))
                        {
                            content = regRegion.Replace(content, "$2$1#region StyleMap" + GetStyleMapCodeContent("$2$1") + "$2$1#endregion");
                        }
                        else if (regClass.IsMatch(content))
                        {
                            content = regClass.Replace(content, "$0$1    #region StyleMap" + GetStyleMapCodeContent("$3$1    ") + "$3$1    #endregion$3$3");
                        }
                        File.WriteAllText(curFilePath, content);
                    }
                }
                StyleBox(subRect, GetStyle("sgsdg"));
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
