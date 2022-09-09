using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    [CustomEditor(typeof(Iceprint), true)]
    internal class IceprintDrawer : IceInspectorDrawer
    {
        Iceprint Target => target as Iceprint;
        protected override void OnGUI()
        {
            Label(Target.gameObject.name, StlHeader);
            Label($"Node Count: {Target.nodeList.Count}");
            Label($"Data Length: {Target.graphData?.Length}");

            if (Button("编辑Blueprint"))
            {
                IceprintBox.OpenPrint(Target);
            }
        }

        [HierarchyItemGUICallback]
        static void OnHierarchyGUI(Iceprint print, Rect selectionRect)
        {
            using var _a = AreaRaw(selectionRect);
            using var _h = HORIZONTAL;

            Space();

#pragma warning disable UNT0008 // Null propagation on Unity objects
            if (IceprintBox.Instance?.Graph == print)
            {
                Label("编辑中...".Color(IceprintBox.Setting.themeColor));
            }
            else
            {
                if (IceButton("编辑")) IceprintBox.OpenPrint(print);
            }
#pragma warning restore UNT0008 // Null propagation on Unity objects
        }

        // TODO: Draw nodes in preview area
    }
}