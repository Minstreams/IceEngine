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
        float labelWidth;
        void OnEnable()
        {
            pack = new IceGUIAutoPack(Repaint);
            labelWidth = EditorGUIUtility.labelWidth;
            foreach (var a in target.GetType().GetCustomAttributes(true))
            {
                if (a is ThemeColorAttribute tc)
                {
                    pack.ThemeColor = tc.Color;
                }
                else if (a is LabelWidthAttribute lw)
                {
                    labelWidth = lw.Width;
                }
            }
        }
        void OnDisable()
        {
            pack = null;
        }
        public sealed override void OnInspectorGUI()
        {
            using (UsePack(pack)) using (LabelWidth(labelWidth)) OnGUI();
        }
        protected virtual void OnGUI()
        {
            IceGUIUtility.DrawSerializedObject(serializedObject);
        }
    }
}