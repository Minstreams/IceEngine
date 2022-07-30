using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Blueprint;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor
{
    /// <summary>
    /// 用于编辑IceBlueprint的窗口
    /// </summary>
    public class IceBlueprintEditorWindow : IceEditorWindow
    {
        public static IceBlueprintBehaviour Target { get; private set; }

        protected override string Title => $"正在编辑 - {Target?.gameObject.name ?? "null"}";

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshTarget();
        }
        void OnSelectionChange() => RefreshTarget();
        void RefreshTarget()
        {
            var gos = Selection.gameObjects;
            if (gos.Length == 1) Target = gos[0].GetComponent<IceBlueprintBehaviour>();
            else Target = null;
            RefreshTitleContent();
            Repaint();
        }

        public static void Open(IceBlueprintBehaviour target)
        {
            Target = target;

            GetWindow<IceBlueprintEditorWindow>().RefreshTitleContent();
        }
        protected override void OnWindowGUI(Rect position)
        {

        }
        protected override void OnExtraGUI(Rect position)
        {
            if (Target == null)
            {
                using (GROUP)
                {
                    Label("Target is Null");
                }
                return;
            }

            //GraphArea(this.position, position, Target.blueprint, stlBackGround: StlDock);
        }
    }
}
