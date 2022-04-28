using UnityEditor;
using UnityEngine;

namespace IceEngine.Assets.IceEngine.Library.LabelAttribute.Editor
{
    /// <summary>
    /// 重载编辑器默认的Inspector行为
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class OverrideDrawer : UnityEditor.Editor
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
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                // 第一个 property 是 m_Script，直接跳过
                while (iterator.NextVisible(false))
                {
                    // Property Field
                    EditorGUILayout.PropertyField(iterator, true);
                }
                serializedObject.ApplyModifiedProperties();
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