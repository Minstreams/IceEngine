using UnityEditor;
using UnityEngine;
using IceEditor.Internal;

namespace IceEditor.Internal
{
    /// <summary>
    /// 重载编辑器默认的Inspector行为
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    internal class IceInspectorGUIDrawer : UnityEditor.Editor
    {
        void OnEnable()
        {
            // 处理Attributes!
            //var fs = target.GetType().GetFields();
            //foreach (var fi in fs)
            //{
            //    GUILayout.Label(fi.Name);
            //    foreach (var a in fi.GetCustomAttributes(true))
            //    {
            //        GUILayout.Label($"---\t{a}");
            //    }
            //}
        }
        public override void OnInspectorGUI()
        {
            {
                IceGUIUtility.DrawSerializedObject(serializedObject);
            }

            var fs = target.GetType().GetFields();
            foreach (var fi in fs)
            {
                GUILayout.Label(fi.Name);
                foreach (var a in fi.GetCustomAttributes(true))
                {
                    GUILayout.Label($"---\t{a}");
                }
            }
        }
    }
}