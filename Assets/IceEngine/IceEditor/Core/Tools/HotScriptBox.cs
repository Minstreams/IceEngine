using System;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection.Emit;

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

        readonly static string baseUsingCode =
@"using System;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor;

";

        [MenuItem("IceEngine/C#热指令")]
        public static void OpenWindow() => GetWindow<HotScriptBox>();
        protected override string Title => "C#热指令";
        [AppStatusBarGUICallback]
        static void OnAppStatusGUI()
        {
            if (Button(new GUIContent("C#".Bold(), "C#热指令"), StlFooterBtn))
            {
                var box = GetWindow<HotScriptBox>();
                var code = GUIUtility.systemCopyBuffer;
                if (jsonReg.IsMatch(code) && box.Dialog("检测到已复制指令，要直接运行吗"))
                {
                    box.AddScript();
                    box.PasteCurScript(code);
                    box.Compile();
                }
            }
        }

        GUIStyle StlAssemblyToggle => _stlAssemblyToggle?.Check() ?? (_stlAssemblyToggle = new GUIStyle("toggle") { richText = true, }); [NonSerialized] GUIStyle _stlAssemblyToggle;
        GUIStyle StlScriptTab => _stlScriptTab?.Check() ?? (_stlScriptTab = new GUIStyle("dragtab") { stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTab;
        GUIStyle StlScriptTabOn => _stlScriptTabOn?.Check() ?? (_stlScriptTabOn = new GUIStyle("dragtab") { padding = new RectOffset(7, 24, 0, 0), stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTabOn;
        GUIStyle StlScriptTabOnFirst => _stlScriptTabOnFirst?.Check() ?? (_stlScriptTabOnFirst = new GUIStyle("dragtab first") { padding = new RectOffset(7, 24, 0, 0), stretchWidth = false, }); [NonSerialized] GUIStyle _stlScriptTabOnFirst;
        GUIStyle StlScriptTabDock => _stlScriptTabDock?.Check() ?? (_stlScriptTabDock = new GUIStyle("dockarea") { padding = new RectOffset(0, 4, 3, 0), }); [NonSerialized] GUIStyle _stlScriptTabDock;
        GUIStyle StlCodeBox => _stlCodeBox?.Check() ?? (_stlCodeBox = new GUIStyle("textfield") { stretchHeight = true, }); GUIStyle _stlCodeBox;

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
        readonly Dictionary<string, string> locationAssemblyMap = new();
        readonly Dictionary<string, string> fullNameAssemblyMap = new();
        readonly static HashSet<string> defaultAssemblyLocations = new();
        void CollectAssemblies()
        {
            nameAssemblyMap.Clear();
            locationAssemblyMap.Clear();
            fullNameAssemblyMap.Clear();
            defaultAssemblyLocations.Clear();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                var name = a.GetName().Name;
                var fullName = a.FullName;
                var location = a.Location;
                nameAssemblyMap.Add(name, location);
                locationAssemblyMap.Add(location, name);
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
        void ReloadCurAssemblies()
        {
            curAssemlySet.Clear();
            foreach (var a in defaultAssemblyLocations)
            {
                curAssemlySet.Add(a);
            }

            if (CurScript == null) return;
            foreach (var a in CurScript.assemblies)
            {
                if (nameAssemblyMap.TryGetValue(a, out var location))
                    curAssemlySet.Add(location);
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
                ReloadCurAssemblies();
            }
        }

        [NonSerialized] List<(string displayName, string value)> filteredResult = null;

        public void CopyCurScript()
        {
            var json = EditorJsonUtility.ToJson(CurScript, true);
            Log("复制热指令数据\n" + json);
            GUIUtility.systemCopyBuffer = json;
        }
        public void PasteCurScript(string json)
        {
            try
            {
                Log("粘贴热指令数据\n" + json);
                EditorJsonUtility.FromJsonOverwrite(json, CurScript);
                ReloadCurAssemblies();
                Setting.Save();
            }
            catch
            {
                LogError($"无效数据格式\n{json}");
            }
        }

        void AddScript()
        {
            Setting.scripts.Add(CurScript = new HotScriptItem());
            Setting.Save();
        }
        #endregion

        #region Life Circle
        protected override void OnEnable()
        {
            base.OnEnable();
            CollectAssemblies();

            mTest = GetType().GetMethod("Test", BindingFlags.Static | BindingFlags.NonPublic);
        }
        private void Update()
        {
            Repaint();
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
            var codeBuilder = new StringBuilder();
            codeBuilder.Append(baseUsingCode);
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
                string guiTitle = title.IsNullOrWhiteSpace() ? "临时窗口" : title;
                codeBuilder.AppendLine($"protected override string Title => \"{guiTitle}\";");
            }
            codeBuilder.Append("}}");

            code = codeBuilder.ToString();

            LogDebug(code);

            // 2. 编译参数
            var objCompilerParameters = new CompilerParameters();
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
                mLast = mCur;
                mCur = m;
            }
        }
        public void Compile() => Compile(CurScript.code, CurScript.type == HotScriptType.GUI, CurScript.name);
        #endregion

        #region ReplaceTest
        MethodInfo mCur;
        MethodInfo mLast;
        MethodInfo mTest;
        static void Test()
        {
            int a = 15;
            Debug.Log($"{a}");
        }
        void Replace(MethodInfo tar, MethodInfo inj)
        {
            //var il = inj.GetMethodBody().GetILAsByteArray();

            //// Pin the bytes in the garbage collection.
            //GCHandle h = GCHandle.Alloc((object)il, GCHandleType.Pinned);
            //IntPtr addr = h.AddrOfPinnedObject();
            //int size = il.Length;

            //// Swap the method.
            //MethodRental.SwapMethodBody(tar.DeclaringType, tar.MetadataToken, addr, size, MethodRental.JitOnDemand);
            //return;

            RuntimeHelpers.PrepareMethod(tar.MethodHandle);
            RuntimeHelpers.PrepareMethod(inj.MethodHandle);
            unsafe
            {
                byte* pTar = (byte*)tar.MethodHandle.Value.ToPointer();
                byte* pInj = (byte*)inj.MethodHandle.Value.ToPointer();

                long* ppTar = (long*)*(long*)(pTar + 8);
                long* ppInj = (long*)*(long*)(pInj + 8);

                void ReplaceInt(int offset)
                {
                    int* t1 = (int*)(pTar + offset);
                    int* t2 = (int*)(pInj + offset);
                    *t1 = *t2;
                }
                //void ReplaceLong(int offset)
                //{
                //    long* t1 = (long*)(ppTar + offset);
                //    long* t2 = (long*)(ppInj + offset);
                //    *t1 = *t2;
                //}

                //ReplaceInt(0);
                ReplaceInt(4);
                ReplaceInt(8);
                ReplaceInt(12);
                //ReplaceInt(16);
                //ReplaceInt(20);
                ////ReplaceInt(24);
                ////ReplaceInt(28);
                //ReplaceInt(40);
                //ReplaceInt(44);
                //ReplaceInt(48);
                //ReplaceInt(52);
                //ReplaceLong(0);
                //ReplaceLong(0);
            }
        }
        void GUI_Test()
        {
            if (Button("Test")) Test();
            if (Button("RunCur")) mCur?.Invoke(null, null);
            if (Button("RunLast")) mLast?.Invoke(null, null);
            if (Button("Replace test with mCur")) Replace(mTest, mCur);
            if (Button("Replace mLast with mCur")) Replace(mLast, mCur);
            if (Button("Reload")) EditorUtility.RequestScriptReload();
            if (Button("RR")) AppDomain.Unload(AppDomain.CurrentDomain);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Label("Debugger is attached.");
            }

            int count = IntField("Count");
            int highlight = IntField("Highlight");
            int searchRangeMin = IntField("SearchRangeMin");
            int searchRange = IntField("SearchRange");
            using (ScrollInvisible("Testt"))
            {
                unsafe
                {

                    void Field(byte* p)
                    {
                        string res = "";
                        for (int i = 0; i < count; ++i)
                        {
                            byte* pp = p + i;
                            byte b = *pp;
                            if ((i & 15) == 0) res += $"[{IceBinaryUtility.GetBytes((long)pp).Hex(false)}] ";
                            res += $"{b:X2} ".Color(b == highlight ? ThemeColor : b == 0 ? Color.black : Color.white);
                            if (((i + 1) & 3) == 0) res += "| ";
                            if (((i + 1) & 15) == 0) res += "\n";
                        }
                        using (GROUP)
                        {
                            Label(res);
                        }
                    }
                    void AllFields(MethodInfo m, bool search = false)
                    {
                        if (m == null) return;
                        byte* p = (byte*)m.MethodHandle.Value.ToPointer();
                        using (BOX)
                        {
                            Label(m.Name, StlSectionHeader);
                            Field(p);
                            var ppp = (byte*)*((long*)(p + 8));
                            Field(ppp);
                            //Field(ppp + (*(ushort*)(p + 4)));
                            //Field((byte*)*((long*)(p + 48)));

                            string res = "";
                            var bts = m.GetMethodBody().GetILAsByteArray();
                            foreach (var b in bts)
                            {
                                res += $"{b:X2} ";
                            }
                            Label(res);

                            if (search)
                            {
                                byte* pppp = (byte*)*(long*)*(long*)(ppp + 16);
                                int offset = 0;
                                for (int i = searchRangeMin; i < searchRange; ++i)
                                {
                                    if (*(pppp + i) == 42 && *(pppp + i - 1) == 10)
                                    {
                                        offset = i;
                                        break;
                                    }
                                }
                                using (HORIZONTAL)
                                {
                                    string attr;
                                    fixed (byte* b = bts)
                                    {
                                        attr = IceBinaryUtility.GetBytes((long)b).Hex(false);
                                    }
                                    Label($"{offset} | {IceBinaryUtility.GetBytes(offset).Hex(false)} | {attr}");
                                    if (offset > 0 && IceButton("Next")) SetInt("SearchRangeMin", offset + 1);
                                }
                                Field(pppp);
                                Field(pppp + offset - 16);
                            }
                        }
                    }

                    using (HORIZONTAL)
                    {
                        AllFields(mTest);
                        AllFields(mCur, true);
                        //AllFields(mLast);
                    }
                }
            }
        }
        #endregion


        readonly static Regex jsonReg = new("^\\{\\r?\\n\\s*\"name\":[\\w\\W]*\\}\\s*$", RegexOptions.Multiline);
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
                        var name = script.name.IsNullOrEmpty() ? $"指令{i}" : script.name;
                        bool selected = CurScript == script;
                        if (selected)
                        {
                            var stlOn = i == 0 ? StlScriptTabOnFirst : StlScriptTabOn;
                            StyleBox(GetRect(TempContent(name), stlOn), stlOn, name, on: true);
                            var rBtn = GetRect(1, 16);
                            if (GUI.Button(rBtn.MoveEdge(left: -15).Move(-4, 2), string.Empty, "WinBtnClose"))
                            {
                                Setting.scripts.RemoveAt(i);
                                int j = i;
                                if (j >= Setting.scripts.Count) --j;
                                if (j >= 0) CurScript = Setting.scripts[j];
                                else CurScript = null;
                                Setting.Save();
                                --i;
                            }
                        }
                        else
                        {
                            if (ToggleButton(name, false, StlScriptTab)) CurScript = script;
                        }
                    }
                }
                Space();
                if (Button(string.Empty, "OL Plus", GUILayout.ExpandWidth(false))) AddScript();
            }

            var r = position.MoveEdge(top: 24);
            using (SubArea(r, out var rMain, out var rSub, "MainArea", 280, IceGUIDirection.Left, "dragtab scroller prev"))
            {
                using (Area(rMain)) using (GUICHECK)
                {
                    var code = CurScript.code = GUILayout.TextArea(CurScript.code, StlCodeBox);
                    if (GUIChanged)
                    {
                        Setting.Save();
                        if (jsonReg.IsMatch(code))
                        {
                            PasteCurScript(code);
                        }
                    }

                    GUI_Test();
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
                                                CurScript.assemblies.Add(locationAssemblyMap[location]);
                                            }
                                            else
                                            {
                                                curAssemlySet.Remove(location);
                                                CurScript.assemblies.Remove(locationAssemblyMap[location]);
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
                    if (Button("运行")) Compile();
                }
            }
        }
    }
}