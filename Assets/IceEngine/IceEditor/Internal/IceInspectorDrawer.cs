using UnityEditor;
using UnityEngine;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    /// <summary>
    /// 重载编辑器默认的InspectorGUI行为
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    internal class IceInspectorDrawer : UnityEditor.Editor
    {
        IceGUIAutoPack pack;
        IceAttributesInfo info;
        void OnEnable()
        {
            pack = new IceGUIAutoPack(Repaint);
            info = IceAttributesInfo.GetInfo(serializedObject);
        }
        void OnDisable()
        {
            pack = null;
            info = null;
        }
        public override void OnInspectorGUI()
        {
            using (UsePack(pack)) IceGUIUtility.DrawSerializedObject(serializedObject, info);
        }
    }
}