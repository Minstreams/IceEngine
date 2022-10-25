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
                ToggleLeft(ref bDeveloperMode, new GUIContent(bDeveloperMode ? "❂".Color(IceGUIUtility.CurrentThemeColor) : "❂", bDeveloperMode ? "开发者模式 开" : "开发者模式 关"), StlFooterBtn);
                if (GUIChanged) EditorPrefs.SetBool("DeveloperMode", bDeveloperMode);
            }
        }

        GUIStyle StlScriptTab => _stlScriptTab?.Check() ?? (_stlScriptTab = new GUIStyle("dragtab") { stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTab;
        GUIStyle StlSystemBox => _stlSystemBox?.Check() ?? (_stlSystemBox = new GUIStyle("window") { border = new RectOffset(11, 11, 11, 13), margin = new RectOffset(8, 8, 8, 8), padding = new RectOffset(0, 0, 0, 4), overflow = new RectOffset(9, 9, -12, 10), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlSystemBox;
        #endregion

        #region SubSystem Management
        List<IceSystemDrawer> SystemDrawers => IceSystemDrawer.Drawers;
        void OnIslandGUI()
        {
            Label("Island");
        }
        #endregion

        #region Code Generation
        public const string SubSystemFolder = "Assets/IceEngine/IceSystem";

        public static string GetDefaultThemeColorCode(Color color) => $"public override Color DefaultThemeColor => new({color.r}f, {color.g}f, {color.b}f);";
        static string GetSubSystemSettingCode(string name) =>
            $"using UnityEngine;\r\n" +
            $"\r\n" +
            $"namespace IceEngine.Internal\r\n" +
            $"{{\r\n" +
            $"    public class Setting{name} : Framework.IceSetting<Setting{name}>\r\n" +
            $"    {{\r\n" +
            $"        #region ThemeColor\r\n" +
            $"        {GetDefaultThemeColorCode(UnityEngine.Random.ColorHSV())}\r\n" +
            $"        #endregion\r\n" +
            $"    }}\r\n" +
            $"}}";
        static string GetSubSystemCode(string name) =>
            $"namespace Ice\r\n" +
            $"{{\r\n" +
            $"    public sealed class {name} : IceEngine.Framework.IceSystem<IceEngine.Internal.Setting{name}>\r\n" +
            $"    {{\r\n" +
            $"\r\n" +
            $"    }}\r\n" +
            $"}}";
        static string GetSubSystemDrawerCode(string name) =>
            $"using System;\r\n" +
            $"\r\n" +
            $"using IceEngine;\r\n" +
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

        static string AsmrefCode =>
            $"{{\n" +
            $"    \"reference\": \"IceEditor\"\n" +
            $"}}";
        static string GetNodeCode(string name, string subSystem) =>
            $"using IceEngine.Framework;\r\n" +
            $"using Sys = Ice.{subSystem};\r\n" +
            $"\r\n" +
            $"namespace IceEngine.IceprintNodes\r\n" +
            $"{{\r\n" +
            $"    [IceprintMenuItem(\"Ice/{subSystem}/{name}\")]\r\n" +
            $"    public class Node{name} : IceprintNode\r\n" +
            $"    {{\r\n" +
            $"        // Ports\r\n" +
            $"        [IceprintPort] public void Test() {{ }}\r\n" +
            $"    }}\r\n" +
            $"}}";
        static string GetNodeDrawerCode(string name) =>
            $"using UnityEngine;\r\n" +
            $"\r\n" +
            $"using IceEngine;\r\n" +
            $"using IceEngine.IceprintNodes;\r\n" +
            $"using static IceEditor.IceGUI;\r\n" +
            $"\r\n" +
            $"namespace IceEditor.Internal\r\n" +
            $"{{\r\n" +
            $"    internal class Node{name}Drawer : Framework.IceprintNodeDrawer<Node{name}>\r\n" +
            $"    {{\r\n" +
            $"\r\n" +
            $"    }}\r\n" +
            $"}}";

        public static void GenerateSubSystem(string name)
        {
            // Path calculation
            string path = $"{SubSystemFolder}/{name}";
            path.TryCreateFolder();

            // Runtime Code
            string runtimePath = $"{path}/Runtime";
            runtimePath.TryCreateFolder();
            File.WriteAllText($"{runtimePath}/Setting{name}.cs", GetSubSystemSettingCode(name), Encoding.UTF8);
            File.WriteAllText($"{runtimePath}/{name}.cs", GetSubSystemCode(name), Encoding.UTF8);

            // Editor Code
            string editorPath = $"{path}/Editor";
            editorPath.TryCreateFolder();
            File.WriteAllText($"{editorPath}/IceEditor.asmref", AsmrefCode, Encoding.UTF8);
            File.WriteAllText($"{editorPath}/{name}Drawer.cs", GetSubSystemDrawerCode(name), Encoding.UTF8);

            AssetDatabase.Refresh();
        }
        public static void DeleteSubSystem(string name)
        {
            var settingPath = $"Assets/Resources/Setting{name}.asset";
            // 删除Setting File
            AssetDatabase.DeleteAsset(settingPath);
            AssetDatabase.DeleteAsset($"{SubSystemFolder}/{name}");
            AssetDatabase.Refresh();
        }
        public static void GenerateNode(string name, string subSystem)
        {
            // Runtime Code
            string runtimePath = $"{SubSystemFolder}/{subSystem}/Runtime/Nodes";
            runtimePath.TryCreateFolder();
            File.WriteAllText($"{runtimePath}/Node{name}.cs", GetNodeCode(name, subSystem), Encoding.UTF8);

            // Editor Code
            string editorPath = $"{SubSystemFolder}/{subSystem}/Editor/Drawers";
            editorPath.TryCreateFolder();
            File.WriteAllText($"{editorPath}/Node{name}Drawer.cs", GetNodeDrawerCode(name), Encoding.UTF8);

            AssetDatabase.Refresh();
        }
        public static void GenerateNodeDrawer(string name, string subSystem)
        {
            // Editor Code
            string editorPath = $"{SubSystemFolder}/{subSystem}/Editor/Drawers";
            editorPath.TryCreateFolder();
            File.WriteAllText($"{editorPath}/Node{name}Drawer.cs", GetNodeDrawerCode(name), Encoding.UTF8);

            AssetDatabase.Refresh();
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (GROUP) using (SectionFolder("子系统"))
            {
                var sysName = GetString("系统名字");
                var selectedSysName = GetString("Selected SubSystem");
                IceSystemDrawer curDrawer = null;
                bool isSystem = false;
                if (sysName == "Island") isSystem = true;
                else
                {
                    foreach (var d in SystemDrawers)
                    {
                        var n = d.SystemName;
                        if (n == sysName) isSystem = true;
                        if (n == selectedSysName) curDrawer = d;
                    }
                }
                bool isSubSystem = isSystem && sysName != "Island";

                using (Vertical(StlSystemBox))
                {
                    using (HORIZONTAL)
                    {
                        if (ToggleButton("Island", curDrawer == null, StlScriptTab)) SetString("Selected SubSystem", null);
                        foreach (var d in SystemDrawers)
                        {
                            var n = d.SystemName;
                            if (ToggleButton(n, curDrawer == d, StlScriptTab)) SetString("Selected SubSystem", n);
                        }
                    }
                    using (Vertical(StlBackground))
                    {
                        if (curDrawer == null)
                        {
                            OnIslandGUI();
                        }
                        else
                        {
                            curDrawer.OnToolBoxGUI();
                        }
                    }
                }
                using (Horizontal(StlGroup)) using (LabelWidth(60))
                {
                    TextField("系统名字");
                    if (isSystem)
                    {
                        if (isSubSystem)
                        {
                            if (IceButton("删除".Color(Color.red)) && Dialog($"将删除{sysName}，无法撤销，确定？"))
                            {
                                DeleteSubSystem(sysName);
                            }
                        }
                    }
                    else
                    {
                        if (IceButton("生成"))
                        {
                            GenerateSubSystem(sysName);
                            DialogNoCancel($"{sysName}系统生成完成！");
                        }
                    }
                }

                if (isSubSystem)
                {
                    using (Horizontal(StlGroup)) using (LabelWidth(60))
                    {
                        var nodeName = TextField("节点名字");

                        var nodePath = $"{SubSystemFolder}/{sysName}/Runtime/Nodes/Node{nodeName}.cs";
                        var drawerPath = $"{SubSystemFolder}/{sysName}/Editor/Drawers/Node{nodeName}Drawer.cs";
                        bool bNodeExist = File.Exists(nodePath);
                        bool bDrawerExist = File.Exists(drawerPath);
                        if (bNodeExist)
                        {
                            if (!bDrawerExist && IceButton("生成Drawer"))
                            {
                                GenerateNodeDrawer(nodeName, sysName);
                            }
                            if (IceButton("删除".Color(Color.red)) && Dialog($"将删除{nodePath}，无法撤销，确定？"))
                            {
                                AssetDatabase.DeleteAsset(nodePath);
                                if (bDrawerExist) AssetDatabase.DeleteAsset(drawerPath);
                                AssetDatabase.Refresh();
                            }
                        }
                        else
                        {
                            if (IceButton("生成"))
                            {
                                GenerateNode(nodeName, sysName);
                                DialogNoCancel($"{sysName}/{nodeName}节点生成完成！");
                            }
                        }
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
                    if (Button("热指令", GUILayout.Height(32))) HotScriptBox.OpenWindow();
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
