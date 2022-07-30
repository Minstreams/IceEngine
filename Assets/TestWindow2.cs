using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

public class TestWindow2 : IceEditorWindow
{
    [MenuItem("测试/Test2")]
    public static void OpenWindow() => GetWindow<TestWindow2>();

    public override GUIContent TitleContent => new GUIContent("测试2");

    public enum Enum1
    {
        aaaaa = 0,
        bbbbb = 1,
        bbbbbasda = 2,
    }
    protected override void OnWindowGUI(Rect position)
    {
        using (HORIZONTAL)
        {
            if (IceButton("Null GameObject"))
            {
                GameObject obj = null;
                TestBinary(obj);
            }
            if (IceButton("string")) TestBinary("Hello, World!");
            if (IceButton("byte")) TestBinary((byte)14);
            if (IceButton("sbyte")) TestBinary((sbyte)-15);
            if (IceButton("bool")) TestBinary((bool)true);
            if (IceButton("char")) TestBinary('从');
            if (IceButton("short")) TestBinary((short)-964);
            if (IceButton("ushort")) TestBinary((ushort)1851);
            if (IceButton("int")) TestBinary((int)-151232);
            if (IceButton("uint")) TestBinary((uint)345893u);
            if (IceButton("long")) TestBinary((long)-145893594948994);
            if (IceButton("ulong")) TestBinary((ulong)345893594948994u);
            if (IceButton("float")) TestBinary((float)3.141592f);
            if (IceButton("double")) TestBinary((double)1.14159225619849465465465);
            if (IceButton("decimal")) TestBinary((decimal)1462457354234234);
            if (IceButton("Type")) TestBinary(typeof(Enum1));
            if (IceButton("Enum1")) TestBinary(Enum1.bbbbbasda);
            if (IceButton("byte[]")) TestBinary(new byte[] { 0, 1, 2, 3, 4, 5 });
            if (IceButton("Self")) TestBinary(this);
        }

        using (HORIZONTAL)
        {
            if (IceButton("Null GameObject"))
            {
                GameObject obj = null;
                TB(obj);
            }
            if (IceButton("string")) TB("Hello, World!");
            if (IceButton("byte")) TB((byte)14);
            if (IceButton("sbyte")) TB((sbyte)-15);
            if (IceButton("bool")) TB((bool)true);
            if (IceButton("char")) TB('从');
            if (IceButton("short")) TB((short)-964);
            if (IceButton("ushort")) TB((ushort)1851);
            if (IceButton("int")) TB((int)-151232);
            if (IceButton("uint")) TB((uint)345893u);
            if (IceButton("long")) TB((long)-145893594948994);
            if (IceButton("ulong")) TB((ulong)345893594948994u);
            if (IceButton("float")) TB((float)3.141592f);
            if (IceButton("double")) TB((double)1.14159225619849465465465);
            if (IceButton("decimal")) TB((decimal)1462457354234234);
            if (IceButton("Type")) TB(typeof(Enum1));
            if (IceButton("Enum1")) TB(Enum1.bbbbbasda);
            if (IceButton("byte[]")) TB(new byte[] { 0, 1, 2, 3, 4, 5 });
            if (IceButton("Self")) TB(this);
        }

        using (GROUP)
        {
            Label(GetString("Console"));
        }
        using (GROUP)
        {
            Label(GetString("Console2"));
        }
        using (GROUP)
        {
            Label(GetString("Console3"));
        }
        using (GROUP)
        {
            Label(GetString("Console4"));
        }
    }
    void TestBinary(object obj)
    {
        SetString("Console", IceBinaryUtility.ToBytes(obj, out var bytes));
        var res = IceBinaryUtility.FromBytesT(bytes);
        SetString("Console2", res == null ? "null" : $"[{res.GetType()}] {res}");
        SetString("Console3", JsonUtility.ToJson(obj, true));
        SetString("Console4", JsonUtility.ToJson(res, true));
    }

    #region 封装一层
    int? baseStack = null;
    void TB(object obj)
    {
        baseStack = null;
        SetString("Console", "");
        IceBinaryUtility.RegisterLogger(OnLog);
        var bytes = IceBinaryUtility.ToBytes(obj);
        var res = IceBinaryUtility.FromBytes(bytes);
        SetString("Console2", res == null ? "null" : $"[{res.GetType()}] {res}");
        SetString("Console3", JsonUtility.ToJson(obj, true));
        SetString("Console4", JsonUtility.ToJson(res, true));
    }
    void OnLog(string log)
    {
        string curLog = GetString("Console");

        string prefix = "";
        System.Diagnostics.StackTrace st = new();
        int fc = st.FrameCount;
        if (baseStack == null)
        {
            baseStack = fc;
            log = log.Replace("\n", "");
        }
        else
        {
            int indent = fc - baseStack.Value;
            if (indent > 0)
            {
                for (int i = 0; i < indent; ++i) prefix += "        ";
                log = log.Replace("\n", $"\n{prefix}");
            }
        }

        curLog += log;

        SetString("Console", curLog);
    }

    void TT()
    {
        var ass = AssetDatabase.FindAssets("t:Material", new string[] { AssetDatabase.GetAssetPath(Selection.activeObject) });
        foreach (var uid in ass)
        {
            var path = AssetDatabase.GUIDToAssetPath(uid);
            if (path.EndsWith("_pc.mat")) continue;
            var newPath = path.Replace(".mat", "_pc.mat");
            AssetDatabase.MoveAsset(path, newPath);
            Debug.Log($"{path}\n{newPath}");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }
    #endregion
}
