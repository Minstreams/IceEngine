using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEditor;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System;

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

    [IcePacket]
    [Serializable]
    public struct Class1
    {
        public float f1;
        public string s2;
        public Class2 c3;
        public C4 c4;
        public Enum1 e1;
        [SerializeField] public DateTimeOffset dt;
    }
    [IcePacket]
    [Serializable]
    public struct C4
    {
        [SerializeField] float f3;
        public C4(float f3)
        {
            this.f3 = f3;
        }
    }
    [IcePacket]
    public class CC2
    {
        [SerializeField] float ff1;
        public int? iii;
    }
    [IcePacket(IsNullable = true)]
    [Serializable]
    public sealed class Class2 : CC2
    {
        public string ss2;
        public Type tt;
    }

    [IcePacket]
    public sealed class Class3
    {
        public Dictionary<string, string> d4;
        public string s1 = "asddwww";
        public List<int> iiiiiii2;
        public Class2[] c3;
    }

    protected override void OnWindowGUI(Rect position)
    {
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
            //if (IceButton("Self")) TB(this);
            if (IceButton("C1")) TB(new Class1()
            {
                f1 = 1.2323f,
                s2 = "asdddss",
                c3 = new Class2() { /*ff1 = 4123.23f,*/ ss2 = "WWQEE", tt = typeof(int) },
                c4 = new C4(1.44444f),
                dt = DateTimeOffset.Now
            });
            if (IceButton("C3")) TB(new Class3()
            {
                s1 = null,
                iiiiiii2 = new List<int> { 1567567, 3453453, 555454, 3245 },
                c3 = new Class2[]
                {
                    new Class2(){ss2 = "asdas"},
                    new Class2(){ss2 = "asdas2"},
                    new Class2(){ss2 = "asdas3"},
                },
                d4 = new Dictionary<string, string>()
                {
                    {"asd", "wewewe" }
                }
            });
            if (IceButton("self")) TB(this);
            Space();
            IceToggle("extraInfo");
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

    #region 封装一层
    string PrintJson(object obj)
    {
        string s1 = JsonUtility.ToJson(obj, false);
        string s2 = JsonUtility.ToJson(obj, true);
        return $"[{s1.Length}]\n{s2}";
    }
    void TB(object obj)
    {
        baseStack = null;
        SetString("Console", "");
        using (new IceBinaryUtility.LogScope(OnLog))
        {
            var bytes = IceBinaryUtility.ToBytes(obj, withExtraInfo: GetBool("extraInfo"));
            var res = IceBinaryUtility.FromBytes(bytes, withExtraInfo: GetBool("extraInfo"));
            SetString("Console2", res == null ? "null" : $"[{res.GetType()}] {res}");
            SetString("Console3", PrintJson(obj));
            SetString("Console4", PrintJson(res));
        }
    }
    int? baseStack = null;
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
    #endregion
}
