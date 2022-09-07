using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using System.IO;
using System.Text;
using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    public class IceToolBox : IceEditorWindow
    {
        #region 定制
        protected override string Title => "❄- Ice工具箱";
        [MenuItem("IceEngine/Ice工具箱 #F1", false, 0)]
        public static void OpenWindow() => GetWindow<IceToolBox>();

        [InitializeOnLoadMethod]
        static void OnLoadd()
        {
            bDeveloperMode = EditorPrefs.GetBool("DeveloperMode");
        }

        [ToolbarGUICallback(ToolbarGUIPosition.Right)]
        static void OnToolbarGUI()
        {
            if (IceButton("-❄-".Size(13), focusedWindow is IceToolBox, "Ice工具箱")) OpenWindow();
        }
        static bool bDeveloperMode;
        [AppStatusBarGUICallback]
        static void OnAppStatusGUI()
        {
            using (GUICHECK)
            {
                IceToggle("❂", ref bDeveloperMode, "开发者模式");
                if (GUIChanged) EditorPrefs.SetBool("DeveloperMode", bDeveloperMode);
            }
        }
        #endregion

        #region SubSystem Management
        public static List<Type> SubSystemList => Ice.Island.SubSystemList;

        void GUI_SubSystem()
        {
            foreach (var sub in SubSystemList)
            {
                Label(sub.Name);
            }
        }
        #endregion

        #region Code Generation
        public const string SubSystemFolder = "Assets/IceEngine/IceSystem";
        public static void GenerateSubSystem(string name)
        {
            // Path calculation
            string path = $"{SubSystemFolder}/{name}";
            IceEditorUtility.TryCreateDirectory(path);

            string resPath = $"{path}/Resources";
            IceEditorUtility.TryCreateDirectory(resPath);

            string runtimePath = $"{path}/Runtime";
            IceEditorUtility.TryCreateDirectory(runtimePath);

            string editorPath = $"{path}/Editor";
            IceEditorUtility.TryCreateDirectory(editorPath);

            // Code
            string settingPath = $"{runtimePath}/Setting{name}.cs";
            string settingCode =
                $"using IceEngine.Framework;\r\n" +
                $"\r\n" +
                $"namespace IceEngine.Internal\r\n" +
                $"{{\r\n" +
                $"    [IceSettingPath(\"IceEngine/IceSystem/{name}\")]\r\n" +
                $"    public class Setting{name} : IceSetting<Setting{name}>\r\n" +
                $"    {{\r\n" +
                $"\r\n" +
                $"    }}\r\n" +
                $"}}";
            File.WriteAllText(settingPath, settingCode, Encoding.UTF8);

            string systemPath = $"{runtimePath}/{name}.cs";
            string systemCode =
                $"using IceEngine.Framework;\r\n" +
                $"\r\n" +
                $"namespace Ice\r\n" +
                $"{{\r\n" +
                $"    public sealed class {name} : IceSystem<IceEngine.Internal.Setting{name}>\r\n" +
                $"    {{\r\n" +
                $"\r\n" +
                $"    }}\r\n" +
                $"}}";
            File.WriteAllText(systemPath, systemCode, Encoding.UTF8);

            string asmrefPath = $"{editorPath}/IceEditor.asmref";
            string asmrefCode =
                $"{{\n" +
                $"    \"reference\": \"IceEditor\"" +
                $"}}";
            File.WriteAllText(asmrefPath, asmrefCode, Encoding.UTF8);

            AssetDatabase.Refresh();
        }

        void GUI_CodeGeneration()
        {
            using var _ = LabelWidth(60);

            using (Horizontal(StlGroup))
            {
                var name = TextField("系统名字");
                if (IceButton("生成"))
                {
                    GenerateSubSystem(name);
                    DialogNoCancel($"{name}系统生成完成！");
                }
            }
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (GROUP) using (SectionFolder("子系统"))
            {
                GUI_SubSystem();
            }

            using (GROUP) using (SectionFolder("代码生成"))
            {
                GUI_CodeGeneration();
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
