using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        [MenuItem("IceEngine/热脚本")]
        static void OpenWindow() => GetWindow<HotScriptBox>();
        protected override string Title => "热脚本";

        GUIStyle StlAssemblyToggle => _stlAssemblyToggle?.Check() ?? (_stlAssemblyToggle = new GUIStyle("toggle") { richText = true, }); GUIStyle _stlAssemblyToggle;
        #endregion

        #region HotScript
        /// <summary>
        /// 实时运行的热脚本实例
        /// </summary>
        [Serializable]
        public class HotScript
        {
            public HashSet<string> assemlyLocationList = new();
        }
        #endregion

        #region Assemblies
        readonly Dictionary<string, Assembly> allAssemblies = new(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => new KeyValuePair<string, Assembly>(a.GetName().Name, a)));
        #endregion

        readonly static Dictionary<string, string> nameAssemblyMap = new();
        readonly static Dictionary<string, string> fullNameAssemblyMap = new();

        static void CollectAssemblies()
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                //nameA
            }
        }



        #region Search Field
        List<string> origin = new();
        List<(string displayName, string value)> filteredResult = new();

        readonly List<KeyValuePair<string, Assembly>> stlListFiltered = new();
        enum AssemblyDisplayMode
        {
            Name,
            FullName,
            Location,
        }
        AssemblyDisplayMode displayMode;
        void SearchAreaGUI()
        {
            // 搜索框
            using (BOX) SearchField(origin, ref filteredResult, extraElementsAction: () =>
            {
                EnumPopup(ref displayMode, GUILayout.Width(64));
            });

            // 在这显示
            using (ScrollInvisible("AssemblyList"))
            {
                // 显示样式列表
                foreach ((var displayName, var name) in stlListFiltered)
                {
                    _ToggleLeft(false, displayName);
                }
                //foreach (var stl in stlListFiltered)
                //{
                //    ToggleLeft(stl.Value.GetName().Name, defaultAssemblies.Contains(stl.Value.GetName().Name), stl.Key, StlAssemblyToggle);
                //}
            }
        }

        #endregion


        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var rMain, out var rSub, "MainArea"))
            {
                using (Area(rMain))
                {

                }
                using (Area(rSub))
                {
                    SearchAreaGUI();
                }
            }
        }
    }
}
