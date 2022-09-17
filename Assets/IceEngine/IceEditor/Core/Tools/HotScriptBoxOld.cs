using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    public class HotScriptBoxOld : IceEditorWindow
    {
        #region Configuration
        readonly static HashSet<string> defaultAssemblies = new() {
            "System",
            "System.Core",
            "UnityEngine",
            "UnityEngine.CoreModule",
            "UnityEditor",
            "UnityEditor.CoreModule",
            //"Game.Common",
            //"Game.Runtime",
            //"Game.Editor",
            //"Game.ClientLog",
        };
        #endregion

        #region 定制
        [MenuItem("程序/工具/热脚本")]
        static void OpenWindow() => GetWindow<HotScriptBoxOld>();
        protected override string Title => "热脚本";
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            DoFilterStyleList(null);
        }

        #region Assemblies
        Dictionary<string, Assembly> Assemblies
        {
            get
            {
                if (!_assemblies.Any())
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (!assembly.IsDynamic)
                        {
                            try
                            {
                                _assemblies.Add(assembly.GetName().Name, assembly);
                            }
                            catch
                            {
                                LogError($"添加引用失败！Location:{assembly.FullName}");
                            }
                        }
                    }
                }
                return _assemblies;
            }
        }
        Dictionary<string, Assembly> _assemblies = new();
        #endregion



        readonly List<KeyValuePair<string, Assembly>> stlListFiltered = new();


        enum DisplayMode
        {
            Name,
            FullName,
            Location,
        }
        [SerializeField] DisplayMode displayMode;

        GUIStyle StlSearchTextField => _stlSearchTextField?.Check() ?? (_stlSearchTextField = new GUIStyle("SearchTextField") { padding = new RectOffset(14, 3, 2, 1), fontSize = 12, fixedHeight = 0f, }); GUIStyle _stlSearchTextField;
        GUIStyle StlAssemblyToggle => _stlAssemblyToggle?.Check() ?? (_stlAssemblyToggle = new GUIStyle("toggle") { richText = true, }); GUIStyle _stlAssemblyToggle;


        #region Utils
        /// <summary>
        /// 筛选并生成列表
        /// </summary>
        public void DoFilterStyleList(string filterStr)
        {
            using var _ = PACK;

            /// <summary>
            /// 比较两个char的值，并指定是否区分大小写
            /// </summary>
            /// <param name="caseSensitive">是否区分大小写</param>
            static bool CompareChar(char self, char other, bool caseSensitive = true)
            {
                if (caseSensitive) return self == other;
                else return char.ToLower(self) == char.ToLower(other);
            }

            string GetName(Assembly val)
            {
                return displayMode switch
                {
                    DisplayMode.Name => val.GetName().Name,
                    DisplayMode.FullName => val.FullName,
                    DisplayMode.Location => val.Location,
                    _ => null,
                };
            }


            bool useRegex = GetBool("正则表达式");
            bool continuousMatching = GetBool("连续匹配");
            bool caseSensitive = GetBool("区分大小写");

            stlListFiltered.Clear();
            if (string.IsNullOrWhiteSpace(filterStr))
            {
                foreach (var stl in Assemblies)
                {
                    stlListFiltered.Add(new KeyValuePair<string, Assembly>(GetName(stl.Value), stl.Value));
                }
            }
            else
            {
                if (useRegex)
                {
                    // 正则表达式匹配
                    foreach (var stl in Assemblies)
                    {
                        try
                        {
                            var option = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                            if (Regex.IsMatch(GetName(stl.Value), filterStr, option))
                            {
                                string name = Regex.Replace(GetName(stl.Value), filterStr, "$0".Color(ThemeColorExp), option);
                                stlListFiltered.Add(new KeyValuePair<string, Assembly>(name, stl.Value));
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    // 判断是否包含filter关键字
                    if (continuousMatching)
                    {
                        foreach (var stl in Assemblies)
                        {
                            // 连续匹配
                            int index = GetName(stl.Value).IndexOf(filterStr, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                            if (index < 0) continue;

                            // 替换关键字为指定颜色
                            string name = GetName(stl.Value)
                                .Insert(index + filterStr.Length, "</color>")
                                .Insert(index, $"<color={ThemeColorExp}>");

                            stlListFiltered.Add(new KeyValuePair<string, Assembly>(name, stl.Value));
                        }
                    }
                    else
                    {
                        foreach (var stl in Assemblies)
                        {
                            int l = filterStr.Length;
                            {
                                // 离散匹配
                                int i = 0;
                                foreach (char c in GetName(stl.Value)) if (CompareChar(c, filterStr[i], caseSensitive) && ++i == l) break;
                                // 不包含则跳过
                                if (i < l) continue;
                            }


                            string name = string.Empty;
                            {
                                // 替换关键字为指定颜色
                                int i = 0;
                                foreach (char c in GetName(stl.Value))
                                {
                                    if (i < l && CompareChar(c, filterStr[i], caseSensitive))
                                    {
                                        name += c.ToString().Color(ThemeColorExp);
                                        ++i;
                                    }
                                    else name += c;
                                }
                            }
                            stlListFiltered.Add(new KeyValuePair<string, Assembly>(name, stl.Value));
                        }
                    }
                }
            }
        }
        public void Compile(string codeStr)
        {
            string code = codeStr;

            const string NamespaceName = "DynamicCodeGenerate";
            const string ClassName = "TestClass";
            const string FuncName = "Test";
            {
                // TODO:提取using
                //code.
                code =
                    "using System;\n" +
                    "using UnityEngine;\n" +
                    "using UnityEditor;\n" +
                    "using System.Collections.Generic;\n" +
                    $"namespace {NamespaceName}{'{'}\n" +
                    $"public class {ClassName}{'{'}\n" +
                    $"public static void {FuncName}(){'{'}\n" +
                    code +
                    "\n}}}";
            }

            // 3.CompilerParameters
            CompilerParameters objCompilerParameters = new CompilerParameters();
            foreach (var a in Assemblies)
            {
                if (GetBool(a.Key))
                {
                    objCompilerParameters.ReferencedAssemblies.Add(a.Value.Location);
                }
            }
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;

            // 4.CompilerResults
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
                //object objHelloWorld = objAssembly.CreateInstance($"{NamespaceName}.{ClassName}");
                //MethodInfo objMI = objHelloWorld.GetType().GetMethod(FuncName);
                //objMI.Invoke(objHelloWorld, null);
            }
        }
        #endregion

        protected override void OnDebugGUI(Rect position)
        {
            const string useRegexKey = "正则表达式";
            const string continuousMatchingKey = "连续匹配";
            const string caseSensitiveKey = "区分大小写";

            // 搜索框
            using (BOX) using (HORIZONTAL) using (GUICHECK)
            {
                //GUILayout.Label((GetBool(useRegexKey) ? $"<color={activeColorStr}>表达式</color>" : "关键字"), StlLabel, GUILayout.ExpandWidth(false));
                string filterStr = TextField("Filter Str", styleOverride: StlSearchTextField);
                if (!GetBool(useRegexKey))
                {
                    IceToggle(continuousMatchingKey, false, "连", "连续匹配");
                }
                IceToggle(caseSensitiveKey, false, "Aa", "区分大小写");
                IceToggle(useRegexKey, false, ".*", "使用正则表达式");
                EnumPopup(ref displayMode);

                if (GUIChanged) DoFilterStyleList(filterStr);
            }

            // 在这显示.
            using (ScrollInvisible("AssemblyList"))
            {
                // 显示样式列表
                foreach (var stl in stlListFiltered)
                {
                    ToggleLeft(stl.Value.GetName().Name, defaultAssemblies.Contains(stl.Value.GetName().Name), stl.Key, StlAssemblyToggle);
                }
            }
        }

        protected override void OnWindowGUI(Rect position)
        {
            codeStr = GUILayout.TextArea(codeStr, "textarea", GUILayout.ExpandHeight(true));
            if (Button("Run"))
            {
                Compile(codeStr);
            }
            //if (Button("Inject"))
            //{
            //    InjectTest();
            //}
            //if (Button("还原"))
            //{
            //    RestoreAllMethod();
            //}
        }

        CSharpCodeProvider provider = new CSharpCodeProvider();
        [SerializeField] string codeStr = null;
    }
}