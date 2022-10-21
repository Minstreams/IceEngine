using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Events;

using IceEngine;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    // TODO: 优化
    [CustomPropertyDrawer(typeof(UnityEventBase), true)]
    internal class IceEventDrawer : UnityEventDrawer
    {
        static GUIStyle StlEventHeaderCounter => _stlEventHeaderCounter?.Check() ?? (_stlEventHeaderCounter = new GUIStyle("WinBtnInactiveMac") { padding = new RectOffset(5, 0, 0, 0), fontSize = 10, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = false, }.Initialize(stl => { stl.normal.textColor = new Color(1f, 1f, 1f); })); static GUIStyle _stlEventHeaderCounter;
        static GUIStyle StlEventHeaderBtn => "FloatFieldLinkButton";
        static GUIStyle StlEventHeaderEmpty => _stlEventHeaderEmpty?.Check() ?? (_stlEventHeaderEmpty = new GUIStyle("sv_label_0") { padding = new(16, 0, 0, 2), fontSize = 12, alignment = TextAnchor.MiddleLeft, fixedHeight = 0f, border = new(8, 8, 4, 4), }).Initialize(stl => { stl.hover.background = stl.normal.background; stl.hover.textColor = Color.white; }); static GUIStyle _stlEventHeaderEmpty;
        GUIStyle StlEventHeader => _stlEventHeader?.Check() ?? (_stlEventHeader = new GUIStyle($"sv_label_{IceGUIUtility.GetThemeColorHueIndex(IceGUIUtility.CurrentThemeColor, true)}") { padding = new(16, 0, 0, 2), fontSize = 12, alignment = TextAnchor.MiddleLeft, fixedHeight = 0f, border = new(8, 8, 4, 4), }).Initialize(stl => { stl.hover.background = stl.normal.background; stl.hover.textColor = Color.white; }); GUIStyle _stlEventHeader;
        GUIStyle HeaderStyle => callCount == 0 ? StlEventHeaderEmpty : StlEventHeader;

        GUIContent label;
        bool bFoldout;
        int callCount;

        protected override void DrawEventHeader(Rect headerRect)
        {
            var r = headerRect.MoveEdge(-22, 6, 0, 0);

            bFoldout = EditorGUI.Toggle(r, GUIContent.none, bFoldout, StlEventHeaderBtn);
            var labelText = label.text;
            if (labelText.StartsWith("On ")) labelText = labelText.Substring(3);

            var t = fieldInfo.FieldType;
            if (t.IsGenericType)
            {
                var args = t.GetGenericArguments();
                labelText += " (";
                for (int i = 0; i < args.Length; i++)
                {
                    System.Type a = args[i];
                    labelText += i > 0 ? $", {a.Name}" : a.Name;
                }
                labelText += ")";
            }

            StyleBox(r.MoveEdge(16), HeaderStyle, labelText, isHover: r.Contains(E.mousePosition));
            if (callCount > 0)
            {
                var rCounter = new Rect() { x = r.x + 17, y = r.y + 1, width = 16, height = 16 };
                StyleBox(rCounter, StlEventHeaderCounter, callCount.ToString());
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            callCount = property.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize;
            this.label = label;

            if (bFoldout) base.OnGUI(position, property, label);
            else DrawEventHeader(position.MoveEdge(6, -6, 1, -1));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (bFoldout) return base.GetPropertyHeight(property, label);
            return 20;
        }
    }
}
