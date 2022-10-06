using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using System.IO;
using System.Text;
using IceEngine;
using IceEditor.Framework;
using IceEditor.Framework.Internal;
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
        static void OnLoad()
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

        GUIStyle StlScriptTab => _stlScriptTab?.Check() ?? (_stlScriptTab = new GUIStyle("dragtab") { stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTab;
        GUIStyle StlSystemBox => _stlSystemBox?.Check() ?? (_stlSystemBox = new GUIStyle("window") { border = new RectOffset(11, 11, 11, 13), margin = new RectOffset(8, 8, 8, 8), padding = new RectOffset(0, 0, 0, 4), overflow = new RectOffset(9, 9, -12, 10), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlSystemBox;
        #endregion

        #region SubSystem Management
        List<IceSystemDrawer> SystemDrawers => IceSystemDrawer.Drawers;
        IceSystemDrawer CurDrawer { get; set; }
        void OnIslandGUI()
        {
            Label("Island");
        }
        #endregion

        #region Code Generation
        public const string SubSystemFolder = "Assets/IceEngine/IceSystem";
        public static void GenerateSubSystem(string name)
        {
            // Path calculation
            string path = $"{SubSystemFolder}/{name}";
            path.TryCreateFolder();

            string resPath = $"{path}/Resources";
            resPath.TryCreateFolder();

            string runtimePath = $"{path}/Runtime";
            runtimePath.TryCreateFolder();

            string editorPath = $"{path}/Editor";
            editorPath.TryCreateFolder();

            // Code
            string settingPath = $"{runtimePath}/Setting{name}.cs";
            string settingCode =
                $"namespace IceEngine.Internal\r\n" +
                $"{{\r\n" +
                $"    public class Setting{name} : Framework.IceSetting<Setting{name}>\r\n" +
                $"    {{\r\n" +
                $"\r\n" +
                $"    }}\r\n" +
                $"}}";
            File.WriteAllText(settingPath, settingCode, Encoding.UTF8);

            string systemPath = $"{runtimePath}/{name}.cs";
            string systemCode =
                $"using IceEngine;\r\n" +
                $"\r\n" +
                $"namespace Ice\r\n" +
                $"{{\r\n" +
                $"    public sealed class {name} : IceEngine.Framework.IceSystem<IceEngine.Internal.Setting{name}>\r\n" +
                $"    {{\r\n" +
                $"\r\n" +
                $"    }}\r\n" +
                $"}}";
            File.WriteAllText(systemPath, systemCode, Encoding.UTF8);

            string drawerPath = $"{editorPath}/{name}Drawer.cs";
            string drawerCode =
                $"using System;\r\n" +
                $"using IceEngine;\r\n" +
                $"\r\n" +
                $"using static IceEditor.IceGUI;\r\n" +
                $"using static IceEditor.IceGUIAuto;\r\n" +
                $"using Sys = Ice.{name};\r\n" +
                $"using SysSetting = IceEngine.Internal.Setting{name};\r\n" +
                $"\r\n" +
                $"namespace IceEditor.Internal\r\n" +
                $"{{\r\n" +
                $"    internal sealed class {name}Drawer : Framework.IceSystemDrawer<Sys, SysSetting>\r\n" +
                $"    {{\r\n" +
                $"        public override void OnToolBoxGUI()\r\n" +
                $"        {{\r\n" +
                $"            Label(\"{name}\");\r\n" +
                $"        }}\r\n" +
                $"    }}\r\n" +
                $"}}";
            File.WriteAllText(drawerPath, drawerCode, Encoding.UTF8);

            string asmrefPath = $"{editorPath}/IceEditor.asmref";
            string asmrefCode =
                $"{{\n" +
                $"    \"reference\": \"IceEditor\"" +
                $"}}";
            File.WriteAllText(asmrefPath, asmrefCode, Encoding.UTF8);

            AssetDatabase.Refresh();
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (GROUP) using (SectionFolder("子系统"))
            {
                using (Vertical(StlSystemBox))
                {
                    using (HORIZONTAL)
                    {
                        if (ToggleButton("Island", CurDrawer == null, StlScriptTab)) CurDrawer = null;
                        foreach (var d in SystemDrawers)
                        {
                            if (ToggleButton(d.SystemName, CurDrawer == d, StlScriptTab)) CurDrawer = d;
                        }
                    }
                    using (Vertical(StlBackground))
                    {
                        if (CurDrawer == null)
                        {
                            OnIslandGUI();
                        }
                        else
                        {
                            CurDrawer.OnToolBoxGUI();
                        }
                    }
                }
                using (Horizontal(StlGroup)) using (LabelWidth(60))
                {
                    var name = TextField("系统名字");
                    if (IceButton("生成"))
                    {
                        GenerateSubSystem(name);
                        DialogNoCancel($"{name}系统生成完成！");
                    }
                }
            }

            using (GROUP) using (SectionFolder("工具箱"))
            {
                Header("编辑器");
                using (HORIZONTAL)
                {
                    if (Button("Iceprint Box", GUILayout.Height(32))) IceprintBox.OpenWindow();
                    if (Button("Style Box", GUILayout.Height(32))) IceGUIStyleBox.OpenWindow();
                }
                Space(8);
                Header("其他");
                using (HORIZONTAL)
                {
                    if (Button("热脚本", GUILayout.Height(32))) HotScriptBox.OpenWindow();
                    if (Button("贴图裁剪UV重排", GUILayout.Height(32))) ModelUVTextureEditorWindow.OpenWindow();
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
