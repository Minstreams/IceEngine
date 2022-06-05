using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor
{
    public class IceGUIStyleEditorWindow : IceEditorWindow
    {
        #region StyleMap
        GUIStyle StlSearchTextField => _stlSearchTextField?.Check() ?? (_stlSearchTextField = new GUIStyle("SearchTextField") { padding = new RectOffset(14, 3, 0, 0), fixedHeight = 17 }); GUIStyle _stlSearchTextField;
        #endregion


        #region ����
        [MenuItem("IceEngine/GUIStyle Box")]
        static void OpenWindow() => GetWindow<IceGUIStyleEditorWindow>();
        protected override string Title => "GUIStyle Box";
        #endregion

        #region StyleList
        public void DoFilterStyleList(string keyword)
        {
            LogImportant($"Filter!{keyword}");
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (SubArea(position, out var rMain, out var rSub, "StyleList Area", 256, IceGUIDirection.Left, "dragtab scroller next", 6, 0))
            {
                // StyleList
                using (Area(rSub))
                {
                    // ������
                    using (DOCK) using (HORIZONTAL) using (GUICHECK)
                    {
                        var keyword = TextFieldNoLabel("�����ؼ���", styleOverride: StlSearchTextField);

                        if (!GetBool("����ƥ��")) IceToggle("����ƥ��", false, "��", "����ƥ��");
                        IceToggle("���ִ�Сд", false, "Aa", "���ִ�Сд");
                        IceToggle("����ƥ��", false, ".*".Bold(), "ʹ��������ʽ");

                        if (GUIChanged) DoFilterStyleList(keyword);
                    }

                    using (ScrollInvisible("StyleList Scroll"))
                    {
                        for (int i = 0; i < 100; ++i)
                        {
                            var r = GetRect(GUILayout.ExpandWidth(true), GUILayout.Height(100));

                            // ����Ұ��Χ��
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
