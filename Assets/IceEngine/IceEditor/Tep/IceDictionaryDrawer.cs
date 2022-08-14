using UnityEngine;
using UnityEditor;

namespace IceEditor.Internal
{
    [CustomPropertyDrawer(typeof(IceEngine.Internal.IceDictionary), true)]
    public class IceDictionaryDrawer : PropertyDrawer
    {
        bool folded = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (folded) return EditorGUIUtility.singleLineHeight + 27;

            var keys = property.FindPropertyRelative("m_Keys");
            var vals = property.FindPropertyRelative("m_Values");
            int count = keys.arraySize;
            float height = EditorGUIUtility.singleLineHeight + 26;

            if (count <= 0) return EditorGUIUtility.singleLineHeight + 14 + height;

            for (int i = 0; i < count; ++i)
            {
                float keyHeight = 14;
                var key = keys.GetArrayElementAtIndex(i);
                if (key.hasVisibleChildren)
                {
                    var end = key.GetEndProperty();
                    key.NextVisible(true);
                    while (key.propertyPath != end.propertyPath)
                    {
                        keyHeight += EditorGUI.GetPropertyHeight(key, true);
                        key.NextVisible(false);
                    }
                }
                else keyHeight += EditorGUIUtility.singleLineHeight;

                float valHeight = 14;
                var val = vals.GetArrayElementAtIndex(i);
                if (val.hasVisibleChildren)
                {
                    var end = val.GetEndProperty();
                    val.NextVisible(true);
                    while (val.propertyPath != end.propertyPath)
                    {
                        valHeight += EditorGUI.GetPropertyHeight(val, true);
                        val.NextVisible(false);
                    }
                }
                else valHeight += EditorGUIUtility.singleLineHeight;

                height += Mathf.Max(keyHeight, valHeight);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (folded)
            {
                float labelWidth = ((GUIStyle)"label").CalcSize(label).x + 64;
                GUI.Box(new Rect(position.x, position.y + 2, labelWidth, position.height - 4), GUIContent.none, "window");
                if (GUI.Button(new Rect(position.x, position.y + 9, labelWidth, EditorGUIUtility.singleLineHeight + 8), "Ⓢ - " + label.text + " - Ⓓ"/*Enum Map*/, "AnimationEventTooltip")) folded = false;
                GUI.Label(new Rect(position.x + labelWidth, position.y + 24, position.width - labelWidth, EditorGUIUtility.singleLineHeight + 8), "folded Dictionary", "ChannelStripSendReturnBar");
            }
            else
            {
                var keys = property.FindPropertyRelative("m_Keys");
                var vals = property.FindPropertyRelative("m_Values");
                int count = keys.arraySize;

                GUI.Box(new Rect(position.x, position.y + 2, position.width, position.height - 4), GUIContent.none, "window");
                GUI.Label(new Rect(position.x, position.y + 9, position.width - 12, EditorGUIUtility.singleLineHeight + 8), "Ⓢ - " + label.text, "AnimationEventTooltip");
                if (GUI.Button(new Rect(position.x, position.y + 9, position.width - 32, EditorGUIUtility.singleLineHeight + 8), "", "InvisibleButton")) folded = true;
                if (GUI.Button(new Rect(position.xMax - 24, position.y + 14, 16, 16), GUIContent.none, "OL Plus"))
                {
                    keys.InsertArrayElementAtIndex(count);
                    vals.InsertArrayElementAtIndex(count);
                    ++count;
                    property.serializedObject.ApplyModifiedProperties();
                }

                Rect propRect = new Rect(position.x + 14, position.y + EditorGUIUtility.singleLineHeight + 27, position.width - 50, EditorGUIUtility.singleLineHeight);
                Rect outerRect = new Rect(propRect.x - 6, propRect.y - 4, propRect.width + 14, propRect.height + 8);
                Rect preRect = new Rect(outerRect.x - 6, outerRect.y + 4, 12, 12);
                Rect delRect = new Rect(outerRect.xMax + 4, outerRect.y + 4, 16, 16);

                //EditorGUI.BeginChangeCheck();
                //while (list.arraySize < count) list.InsertArrayElementAtIndex(list.arraySize);
                //while (list.arraySize > count) list.DeleteArrayElementAtIndex(list.arraySize - 1);
                //if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

                if (count == 0)
                {
                    GUI.Button(preRect, GUIContent.none, "Radio");
                    GUI.Box(outerRect, GUIContent.none, "window");
                    EditorGUI.LabelField(propRect, "Empty...");
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    for (int i = 0; i < count; ++i)
                    {
                        float keyHeight = 0;
                        var key = keys.GetArrayElementAtIndex(i);
                        if (key.hasVisibleChildren)
                        {
                            var end = key.GetEndProperty();
                            key.NextVisible(true);
                            while (key.propertyPath != end.propertyPath)
                            {
                                keyHeight += EditorGUI.GetPropertyHeight(key, true);
                                key.NextVisible(false);
                            }
                        }
                        else keyHeight += EditorGUIUtility.singleLineHeight;

                        float valHeight = 0;
                        var val = vals.GetArrayElementAtIndex(i);
                        if (val.hasVisibleChildren)
                        {
                            var end = val.GetEndProperty();
                            val.NextVisible(true);
                            while (val.propertyPath != end.propertyPath)
                            {
                                valHeight += EditorGUI.GetPropertyHeight(val, true);
                                val.NextVisible(false);
                            }
                        }
                        else valHeight += EditorGUIUtility.singleLineHeight;

                        var unitHeight = Mathf.Max(keyHeight, valHeight);
                        propRect.height = unitHeight;
                        outerRect.height = propRect.height + 8;
                        GUI.Button(preRect, GUIContent.none, "Radio");
                        if (GUI.Button(delRect, GUIContent.none, "OL Minus"))
                        {
                            keys.DeleteArrayElementAtIndex(i);
                            vals.DeleteArrayElementAtIndex(i);
                            --count;
                            --i;
                            continue;
                        }
                        GUI.Box(outerRect, GUIContent.none, "window");
                        OnItemGUI(propRect, keys.GetArrayElementAtIndex(i), vals.GetArrayElementAtIndex(i), i);
                        preRect.y += unitHeight + 14;
                        delRect.y += unitHeight + 14;
                        outerRect.y += unitHeight + 14;
                        propRect.y += unitHeight + 14;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            EditorGUI.EndProperty();
        }
        public virtual void OnItemGUI(Rect propRect, SerializedProperty key, SerializedProperty val, int index)
        {
            var lw = EditorGUIUtility.labelWidth;
            var keyRect = new Rect(propRect.x, propRect.y, lw, propRect.height);
            if (key.hasVisibleChildren)
            {
                var elRect = new Rect(keyRect) { height = EditorGUIUtility.singleLineHeight };
                EditorGUIUtility.labelWidth = 32;
                var end = key.GetEndProperty();
                key.NextVisible(true);
                while (key.propertyPath != end.propertyPath)
                {
                    EditorGUI.PropertyField(elRect, key, true);
                    elRect.y += EditorGUIUtility.singleLineHeight;
                    key.NextVisible(false);
                }
            }
            else
            {
                EditorGUIUtility.labelWidth = 1;
                EditorGUI.PropertyField(keyRect, key, true);
            }
            var valRect = new Rect(propRect.x + lw, propRect.y, propRect.width - lw, propRect.height);
            if (val.hasVisibleChildren)
            {
                var elRect = new Rect(valRect) { height = EditorGUIUtility.singleLineHeight };
                EditorGUIUtility.labelWidth = 32;
                var end = val.GetEndProperty();
                val.NextVisible(true);
                while (val.propertyPath != end.propertyPath)
                {
                    EditorGUI.PropertyField(elRect, val, true);
                    elRect.y += EditorGUIUtility.singleLineHeight;
                    val.NextVisible(false);
                }
            }
            else
            {
                EditorGUIUtility.labelWidth = 1;
                EditorGUI.PropertyField(valRect, val, true);
            }
            EditorGUIUtility.labelWidth -= lw;
        }
    }
}