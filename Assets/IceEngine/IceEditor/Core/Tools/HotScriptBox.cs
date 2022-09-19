﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    public class HotScriptBox : IceEditorWindow
    {
        #region 定制
        readonly static HashSet<string> defaultAssemblyNames = new() {
            "System",
            "System.Core",
            "UnityEngine",
            "UnityEngine.CoreModule",
            "UnityEngine.IMGUIModule",
            "UnityEditor",
            "UnityEditor.CoreModule",
            "IceCore",
            "IceRuntime",
            "IceEditor",
        };

        [MenuItem("IceEngine/热脚本")]
        static void OpenWindow() => GetWindow<HotScriptBox>();
        protected override string Title => "热脚本";

        GUIStyle StlAssemblyToggle => _stlAssemblyToggle?.Check() ?? (_stlAssemblyToggle = new GUIStyle("toggle") { richText = true, }); [NonSerialized] GUIStyle _stlAssemblyToggle;
        GUIStyle StlScriptTab => _stlScriptTab?.Check() ?? (_stlScriptTab = new GUIStyle("dragtab") { stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTab;
        GUIStyle StlScriptTabOn => _stlScriptTabOn?.Check() ?? (_stlScriptTabOn = new GUIStyle("dragtab") { padding = new RectOffset(7, 24, 0, 0), stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTabOn;
        GUIStyle StlScriptTabOnFirst => _stlScriptTabOnFirst?.Check() ?? (_stlScriptTabOnFirst = new GUIStyle("dragtab first") { padding = new RectOffset(7, 24, 0, 0), stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTabOnFirst;
        GUIStyle StlScriptTabDock => _stlScriptTabDock?.Check() ?? (_stlScriptTabDock = new GUIStyle("dockarea") { padding = new RectOffset(0, 4, 3, 0), }); [NonSerialized] GUIStyle _stlScriptTabDock;

        protected override void OnThemeColorChange()
        {
            filteredResult = null;
        }
        protected override Color DefaultThemeColor => new Color(1, 0.7f, 0);
        #endregion

        #region Utility
        static EditorSettingHotScriptBox Setting => EditorSettingHotScriptBox.Setting;

        enum AssemblyDisplayMode
        {
            Name,
            FullName,
            Location,
        }
        AssemblyDisplayMode displayMode;
        readonly Dictionary<string, string> nameAssemblyMap = new();
        readonly Dictionary<string, string> fullNameAssemblyMap = new();
        readonly static HashSet<string> defaultAssemblyLocations = new();
        void CollectAssemblies()
        {
            nameAssemblyMap.Clear();
            fullNameAssemblyMap.Clear();
            defaultAssemblyLocations.Clear();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                var name = a.GetName().Name;
                var fullName = a.FullName;
                var location = a.Location;
                nameAssemblyMap.Add(name, location);
                fullNameAssemblyMap.Add(fullName, location);
            }
            foreach (var n in defaultAssemblyNames)
            {
                if (nameAssemblyMap.TryGetValue(n, out var location))
                {
                    defaultAssemblyLocations.Add(location);
                }
            }
        }

        readonly HashSet<string> curAssemlySet = new();
        [NonSerialized] HotScriptItem _curScript;
        HotScriptItem CurScript
        {
            get
            {
                if (_curScript == null)
                {
                    if (Setting.scripts.Count == 0)
                    {
                        Setting.scripts.Add(new HotScriptItem());
                        Setting.Save();
                    }
                    CurScript = Setting.scripts[0];
                }
                return _curScript;
            }
            set
            {
                _curScript = value;
                curAssemlySet.Clear();
                foreach (var a in defaultAssemblyLocations)
                {
                    curAssemlySet.Add(a);
                }

                if (value == null) return;
                foreach (var a in value.assemblies)
                {
                    curAssemlySet.Add(a);
                }
            }
        }

        [NonSerialized] List<(string displayName, string value)> filteredResult = null;

        public void CopyCurScript()
        {
            var json = EditorJsonUtility.ToJson(CurScript, true);
            Log("复制热脚本数据\n" + json);
            GUIUtility.systemCopyBuffer = json;
        }
        public void PasteCurScript(string json)
        {
            Log("粘贴热脚本数据\n" + json);
            EditorJsonUtility.FromJsonOverwrite(json, CurScript);
            Setting.Save();
        }
        #endregion

        #region Life Circle
        protected override void OnEnable()
        {
            base.OnEnable();
            CollectAssemblies();
        }
        #endregion

        #region Compile
        readonly static Regex usingReg = new("^\\s*using .+;\\s*\n", RegexOptions.Multiline);
        readonly Microsoft.CSharp.CSharpCodeProvider provider = new();
        public void Compile(string code, bool bGUI, string title = null)
        {
            const string NamespaceName = "DynamicCodeGenerate";
            const string ClassName = "DynamicClass";
            const string FuncName = "DynamicMethod";

            // 1. 预处理代码
            StringBuilder codeBuilder = new();
            codeBuilder.Append(
@"using System;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor;

");
            if (bGUI)
            {
                codeBuilder.Append(
@"using IceEditor.Framework;

using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

");
            }
            code = usingReg.Replace(code, m =>
            {
                codeBuilder.Append(m.Value);
                return string.Empty;
            });

            if (bGUI)
            {
                codeBuilder.AppendLine($"namespace {NamespaceName}{{\npublic class {ClassName} : IceEditorWindow {{\nprotected override void OnWindowGUI(Rect position)\n{{");
            }
            else
            {
                codeBuilder.AppendLine($"namespace {NamespaceName}{{\npublic class {ClassName}{{\npublic static void {FuncName}()\n{{");
            }
            codeBuilder.AppendLine(code);
            codeBuilder.AppendLine("}");
            if (bGUI)
            {
                codeBuilder.AppendLine($"public static void {FuncName}() => GetWindow<{ClassName}>();");
                codeBuilder.AppendLine("protected override Color DefaultThemeColor => new Color(0.8f, 0.2f, 0.5f);");
                if (!title.IsNullOrWhiteSpace()) codeBuilder.AppendLine($"protected override string Title => \"{title}\";");
            }
            codeBuilder.Append("}}");

            code = codeBuilder.ToString();

            LogDebug(code);

            // 2. 编译参数
            CompilerParameters objCompilerParameters = new CompilerParameters();
            foreach (var a in curAssemlySet)
            {
                objCompilerParameters.ReferencedAssemblies.Add(a);
            }
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;

            // 3. 编译结果处理
            CompilerResults cr = provider.CompileAssemblyFromSource(objCompilerParameters, code);

            if (cr.Errors.HasErrors)
            {
                string log = "编译错误：";

                foreach (CompilerError err in cr.Errors)
                {
                    log += err.ErrorText;
                }

                LogError(log);
            }
            else
            {
                // 通过反射，调用实例
                Assembly objAssembly = cr.CompiledAssembly;
                var m = objAssembly.GetType($"{NamespaceName}.{ClassName}").GetMethod(FuncName, BindingFlags.Public | BindingFlags.Static);
                m?.Invoke(null, null);
            }
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            // Dock
            using (Horizontal(StlScriptTabDock))
            {
                for (int i = 0; i < Setting.scripts.Count; ++i)
                {
                    var script = Setting.scripts[i];
                    using (HORIZONTAL)
                    {
                        var name = script.name.IsNullOrEmpty() ? i.ToString() : script.name;
                        bool selected = CurScript == script;
                        if (selected)
                        {
                            var stlOn = i == 0 ? StlScriptTabOnFirst : StlScriptTabOn;
                            StyleBox(GetRect(TempContent(name), stlOn), stlOn, name, on: true);
                            var rBtn = GetRect(1, 16);
                            if (GUI.Button(rBtn.MoveEdge(left: -15).Move(-4, 2), string.Empty, "WinBtnClose"))
                            {
                                Setting.scripts.RemoveAt(i);
                                --i;
                                if (i >= 0) CurScript = Setting.scripts[i];
                                else CurScript = null;
                                Setting.Save();
                            }
                        }
                        else
                        {
                            if (ToggleButton(name, false, StlScriptTab)) CurScript = script;
                        }
                    }
                }
                Space();
                if (Button(string.Empty, "OL Plus", GUILayout.ExpandWidth(false)))
                {
                    Setting.scripts.Add(CurScript = new HotScriptItem());
                    Setting.Save();
                }
            }

            var r = position.MoveEdge(top: 24);
            using (SubArea(r, out var rMain, out var rSub, "MainArea", 280, IceGUIDirection.Left, "dragtab scroller next"))
            {
                using (Area(rMain)) using (GUICHECK)
                {
                    var code = CurScript.code = GUILayout.TextArea(CurScript.code, "textarea", GUILayout.ExpandHeight(true));
                    if (GUIChanged)
                    {
                        Setting.Save();
                        if (code.StartsWith("{\n    \"name\": ") && code.EndsWith("\"\n}"))
                        {
                            PasteCurScript(code);
                        }
                    }
                }
                using (Area(rSub)) using (LabelWidth(32))
                {
                    using (GUICHECK)
                    {
                        TextField("名字", ref CurScript.name);
                        EnumPopup("类型", ref CurScript.type);

                        if (GUIChanged) Setting.Save();
                    }
                    Space(4);

                    // 搜索框
                    using (BOX)
                    {
                        using (HORIZONTAL)
                        {

                            Label("程序集");
                            SearchField(displayMode switch
                            {
                                AssemblyDisplayMode.Name => nameAssemblyMap.Keys,
                                AssemblyDisplayMode.FullName => fullNameAssemblyMap.Keys,
                                _ => nameAssemblyMap.Values,
                            }, ref filteredResult);

                            using (GUICHECK)
                            {
                                EnumPopup(ref displayMode, GUILayout.Width(64));
                                if (GUIChanged) filteredResult = null;
                            }
                        }


                        // 在这显示
                        using (ScrollInvisible("AssemblyList"))
                        {
                            // 显示样式列表
                            if (filteredResult != null)
                                foreach ((var displayName, var name) in filteredResult)
                                {
                                    string location = displayMode switch
                                    {
                                        AssemblyDisplayMode.Name => nameAssemblyMap[name],
                                        AssemblyDisplayMode.FullName => fullNameAssemblyMap[name],
                                        _ => name,
                                    };

                                    using (GUICHECK) using (Disable(defaultAssemblyLocations.Contains(location)))
                                    {
                                        var val = _ToggleLeft(curAssemlySet.Contains(location), displayName, StlAssemblyToggle);
                                        if (GUIChanged)
                                        {
                                            if (val)
                                            {
                                                curAssemlySet.Add(location);
                                                CurScript.assemblies.Add(location);
                                            }
                                            else
                                            {
                                                curAssemlySet.Remove(location);
                                                CurScript.assemblies.Remove(location);
                                            }
                                            Setting.Save();
                                        }
                                    }
                                }
                        }
                    }

                    using (HORIZONTAL)
                    {
                        if (Button("复制")) CopyCurScript();
                        if (Button("粘贴")) PasteCurScript(GUIUtility.systemCopyBuffer);
                    }
                    if (Button("运行"))
                    {
                        Compile(CurScript.code, CurScript.type == HotScriptType.GUI, CurScript.name);
                    }
                }
            }
        }
    }
}