using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor
{
    public class IceGUIStyleEditorWindow : IceEditorWindow
    {
        #region StyleMap
        GUIStyle StlSearchTextField => _stlSearchTextField?.Check() ?? (_stlSearchTextField = new GUIStyle("SearchTextField") { padding = new RectOffset(14, 3, 0, 0), fixedHeight = 17 }); GUIStyle _stlSearchTextField;
        #endregion


        #region 定制
        [MenuItem("测试/GUIStyle Box")]
        static void OpenWindow() => GetWindow<IceGUIStyleEditorWindow>();
        protected override string Title => "GUIStyle Box";
        #endregion

        #region StyleList
        public void DoFilterStyleList(string keyword)
        {
            Log($"Filter!{keyword}");
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var rMain, out var rSub, "StyleList Area", 256, IceGUIDirection.Left, "dragtab scroller next", 6, 0))
            {
                // StyleList
                using (Area(rSub))
                {
                    // 搜索框
                    using (DOCK) using (HORIZONTAL) using (GUICHECK)
                    {
                        var keyword = TextFieldNoLabel("搜索关键字", styleOverride: StlSearchTextField);

                        if (!GetBool("正则匹配")) IceToggle("连续匹配", false, "连", "连续匹配");
                        IceToggle("区分大小写", false, "Aa", "区分大小写");
                        IceToggle("正则匹配", false, ".*".Bold(), "使用正则表达式");

                        if (GUIChanged) DoFilterStyleList(keyword);
                    }

                    using (ScrollInvisible("StyleList Scroll"))
                    {
                        for (int i = 0; i < 100; ++i)
                        {
                            var r = GetRect(GUILayout.ExpandWidth(true), GUILayout.Height(100));

                            // 在视野范围内
                            var scrollY = GetVector2("StyleList Scroll").y;
                            if (r.yMax > scrollY && r.yMin < scrollY + rSub.height)
                            {
                                StyleBox(r, StlBox, $"{i} | {r.y}");

                            }
                        }
                    }
                }

                using (Area(rMain))
                {

                }
            }
        }
    }
}
