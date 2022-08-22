using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class LabelEventDrawer : UnityEventDrawer
{
    bool initialized;
    bool activated;
    bool empty;

    static GUIStyle headerEmptyStyle = null;
    static GUIStyle HeaderEmptyStyle
    {
        get
        {
            if (headerEmptyStyle == null)
            {
                headerEmptyStyle = new GUIStyle("flow node 0");
                headerEmptyStyle.alignment = TextAnchor.MiddleLeft;
                headerEmptyStyle.padding = new RectOffset(8, 0, 0, 0);
                headerEmptyStyle.contentOffset = Vector2.zero;
            }
            return headerEmptyStyle;
        }
    }
    static GUIStyle headerNormalStyle = null;
    static GUIStyle HeaderNormalStyle
    {
        get
        {
            if (headerNormalStyle == null)
            {
                headerNormalStyle = new GUIStyle("flow node 5");
                headerNormalStyle.alignment = TextAnchor.MiddleLeft;
                headerNormalStyle.padding = new RectOffset(8, 0, 0, 0);
                headerNormalStyle.contentOffset = Vector2.zero;
            }
            return headerNormalStyle;
        }
    }
    GUIStyle HeaderStyle => empty ? HeaderEmptyStyle : HeaderNormalStyle;

    protected override void DrawEventHeader(Rect headerRect)
    {
        if (GUI.Button(new Rect(headerRect.x - 6, headerRect.y - 1, headerRect.width + 12, headerRect.height), GUIContent.none, HeaderStyle))
        {
            activated = false;
            HandleUtility.Repaint();
        }
        base.DrawEventHeader(headerRect);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.y += 1;
        position.height -= 1;
        if (!initialized)
        {
            base.OnGUI(position, property, label);
            initialized = true;
        }
        else if (activated) base.OnGUI(position, property, label);
        else
        {
            if (GUI.Button(new Rect(position.x, position.y + 1, 16, position.height), GUIContent.none, "ObjectFieldButton")
                || GUI.Button(new Rect(position.x + 16, position.y, position.width - 16, position.height), label.text, HeaderStyle))
            {
                activated = true;
                HandleUtility.Repaint();
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (activated) return base.GetPropertyHeight(property, label);
        else return HeaderStyle.CalcSize(label).y + 3;
    }

    protected override void SetupReorderableList(ReorderableList list)
    {
        base.SetupReorderableList(list);
        empty = list.count == 0;
    }
    protected override void OnAddEvent(ReorderableList list)
    {
        base.OnAddEvent(list);
        empty = false;
    }
    protected override void OnRemoveEvent(ReorderableList list)
    {
        base.OnRemoveEvent(list);
        if (list.count == 0)
        {
            empty = true;
        }
    }
}
