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

        // TODO: Draw nodes in preview area
    }
}