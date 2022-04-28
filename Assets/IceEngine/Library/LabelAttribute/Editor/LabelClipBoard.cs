using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LabelClipBoard
{
    static Dictionary<Type, object> clipBoard = new Dictionary<Type, object>();
    [MenuItem("CONTEXT/LabelClipBoard/C_Copy Data")]
    static void Copy(MenuCommand cmd)
    {
        var field = cmd.context.GetType().GetFields()[cmd.userData];
        var type = field.FieldType;
        var val = field.GetValue(cmd.context);
        if (clipBoard.ContainsKey(type)) clipBoard[type] = val;
        else clipBoard.Add(type, val);
    }
    //readonly static HashSet<Type> numberTypes = new HashSet<Type>()
    //{
    //    typeof(int),
    //    typeof(float),
    //    typeof(double),
    //};
    //readonly static HashSet<Type> vectorTypes = new HashSet<Type>()
    //{
    //    typeof(Vector2),
    //    typeof(Vector2Int),
    //    typeof(Vector3),
    //    typeof(Vector3Int),
    //    typeof(Vector4),
    //};
    [MenuItem("CONTEXT/LabelClipBoard/V_Paste Data", true)]
    static bool CanPaste(MenuCommand cmd)
    {
        var t = cmd.context.GetType().GetFields()[cmd.userData].FieldType;
        return clipBoard.ContainsKey(t);
    }
    [MenuItem("CONTEXT/LabelClipBoard/V_Paste Data")]
    static void Paste(MenuCommand cmd)
    {
        var field = cmd.context.GetType().GetFields()[cmd.userData];
        var type = field.FieldType;
        field.SetValue(cmd.context, clipBoard[type]);
        EditorUtility.SetDirty(cmd.context);
    }
    static void PasteFromTo(MenuCommand cmd, Type fromType, string toField)
    {
        var field = cmd.context.GetType().GetFields()[cmd.userData];
        var type = field.FieldType;
        var val = field.GetValue(cmd.context);
        type.GetField(toField).SetValue(val, clipBoard[fromType]);
        field.SetValue(cmd.context, val);
        EditorUtility.SetDirty(cmd.context);
    }
    [MenuItem("CONTEXT/LabelClipBoard/Paste From Float To X", true)]
    [MenuItem("CONTEXT/LabelClipBoard/Paste From Float To Y", true)]
    [MenuItem("CONTEXT/LabelClipBoard/Paste From Float To Z", true)]
    static bool CanPasteFromFloatToVector(MenuCommand cmd)
    {
        var t = cmd.context.GetType().GetFields()[cmd.userData].FieldType;
        return t == typeof(Vector3) && clipBoard.ContainsKey(typeof(float));
    }
    [MenuItem("CONTEXT/LabelClipBoard/Paste From Float To X")]
    static void PasteFromFloatToX(MenuCommand cmd) => PasteFromTo(cmd, typeof(float), "x");
    [MenuItem("CONTEXT/LabelClipBoard/Paste From Float To Y")]
    static void PasteFromFloatToY(MenuCommand cmd) => PasteFromTo(cmd, typeof(float), "y");
    [MenuItem("CONTEXT/LabelClipBoard/Paste From Float To Z")]
    static void PasteFromFloatToZ(MenuCommand cmd) => PasteFromTo(cmd, typeof(float), "z");

    public static MenuCommand CreateMenuCommand(SerializedProperty property)
    {
        var context = property.serializedObject.targetObject;
        var props = context.GetType().GetFields();
        var pName = property.name;
        int i = 0;
        while (i < props.Length && props[i].Name != pName) ++i;
        return new MenuCommand(context, i);
    }
    public static void PopupMenu(Rect position, SerializedProperty property)
    {
        if (Event.current.isMouse && Event.current.button == 1 && position.Contains(Event.current.mousePosition))
        {
            EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition, Vector2.zero), "CONTEXT/LabelClipBoard/", LabelClipBoard.CreateMenuCommand(property));
        }
    }
}