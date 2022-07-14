using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.Blueprint;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    /// <summary>
    /// BP的Inspector面板
    /// </summary>
    [CustomEditor(typeof(IceBlueprintBehaviour), true)]
    internal class IceBlueprintDrawer : IceInspectorDrawer
    {
        IceBlueprintBehaviour Target => target as IceBlueprintBehaviour;
        public override void OnInspectorGUI()
        {
            if (Button("编辑Blueprint"))
            {
                IceBlueprintEditorWindow.Open(Target);
            }
            if (Button("Fan"))
            {
                Target.OnAfterDeserialize();
            }
            using (GROUP)
            {
                base.OnInspectorGUI();
            }
        }
    }
}