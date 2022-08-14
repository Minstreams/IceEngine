using UnityEngine;
using UnityEditor;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    [@CustomPropertyDrawer(typeof(IceEngine.Internal.IceEnumMap), true)]
    public class IceEnumMapDrawer : PropertyDrawer
    {
        bool folded = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (folded) return EditorGUIUtility.singleLineHeight + 27;

            var list = property.FindPropertyRelative("list");
            int count = list.arraySize;
            float height = EditorGUIUtility.singleLineHeight + 26;

            if (count <= 0) return EditorGUIUtility.singleLineHeight + 14 + height;

            for (int i = 0; i < count; ++i) height += EditorGUI.GetPropertyHeight(list.GetArrayElementAtIndex(i), true) + 14;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (folded)
            {
                float labelWidth = ((GUIStyle)"label").CalcSize(label).x + 64;
                GUI.Box(new Rect(position.x, position.y + 2, labelWidth, position.height - 4), GUIContent.none, "window");
                if (GUI.Button(new Rect(position.x, position.y + 9, labelWidth, EditorGUIUtility.singleLineHeight + 8), "Ⓔ - " + label.text + " - Ⓜ"/*Enum Map*/, "AnimationEventTooltip")) folded = false;
                GUI.Label(new Rect(position.x + labelWidth, position.y + 24, position.width - labelWidth, EditorGUIUtility.singleLineHeight + 8), "folded Enum Map", "ChannelStripSendReturnBar");
            }
            else
            {
                GUI.Box(new Rect(position.x, position.y + 2, position.width, position.height - 4), GUIContent.none, "window");
                if (GUI.Button(new Rect(position.x, position.y + 9, position.width - 14, EditorGUIUtility.singleLineHeight + 8), "Ⓔ - " + label.text, "AnimationEventTooltip")) folded = true;
                GUI.Label(new Rect(position.x + position.width - 30, position.y + 9, 30, EditorGUIUtility.singleLineHeight + 8), "Ⓜ", "InvisibleButton");

                Rect propRect = new Rect(position.x + 14, position.y + EditorGUIUtility.singleLineHeight + 27, position.width - 26, EditorGUIUtility.singleLineHeight);
                Rect outerRect = new Rect(propRect.x - 6, propRect.y - 4, propRect.width + 12, propRect.height + 8);
                Rect preRect = new Rect(outerRect.x - 6, outerRect.y + 4, 12, 12);

                var enumNames = property.FindPropertyRelative("_").enumDisplayNames;
                int count = enumNames.Length;
                var list = property.FindPropertyRelative("list");
                while (list.arraySize < count) list.InsertArrayElementAtIndex(list.arraySize);
                while (list.arraySize > count) list.DeleteArrayElementAtIndex(list.arraySize - 1);

                if (count == 0)
                {
                    GUI.Button(preRect, GUIContent.none, "Radio");
                    GUI.Box(outerRect, GUIContent.none, "window");
                    EditorGUI.LabelField(propRect, "Empty...");
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        var unitHeight = EditorGUI.GetPropertyHeight(list.GetArrayElementAtIndex(i), true);
                        propRect.height = unitHeight;
                        outerRect.height = propRect.height + 8;
                        GUI.Button(preRect, GUIContent.none, "Radio");
                        GUI.Box(outerRect, GUIContent.none, "window");
                        OnItemGUI(propRect, list.GetArrayElementAtIndex(i), TempContent(enumNames[i]), i);
                        preRect.y += unitHeight + 14;
                        outerRect.y += unitHeight + 14;
                        propRect.y += unitHeight + 14;
                    }
                }
            }

            EditorGUI.EndProperty();
        }
        public virtual void OnItemGUI(Rect propRect, SerializedProperty item, GUIContent label, int index)
        {
            EditorGUI.PropertyField(propRect, item, label, true);
        }
    }
}