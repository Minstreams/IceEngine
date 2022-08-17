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
            if (Button("编辑Blueprint"))
            {
                //IceprintEditorWindow.Open(Target);
            }
            using (GROUP)
            {
                base.OnGUI();
            }
        }
    }
}