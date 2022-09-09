using UnityEditor;
using UnityEngine;

using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    /// <summary>
    /// 重载编辑器默认的InspectorGUI行为
    /// </summary>
    [CustomEditor(typeof(Object), true), CanEditMultipleObjects]
    internal class IceInspectorDrawer : Editor
    {
        IceGUIAutoPack pack;
        void OnEnable()
        {
            pack = new IceGUIAutoPack(Repaint);
        }
        void OnDisable()
        {
            pack = null;
        }
        public sealed override void OnInspectorGUI()
        {
            using (UsePack(pack)) OnGUI();
        }
        protected virtual void OnGUI()
        {
            IceGUIUtility.DrawSerializedObject(serializedObject);
        }
    }
}