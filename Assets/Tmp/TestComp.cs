using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceEngine;
using System;
using IceEngine.Framework;
using UnityEngine.Events;

[ThemeColor(1, 0, 1)]
[LabelWidth(48)]
    [IceprintNode]
public class TestComp : MonoBehaviour
{
    [Group("Output")]
    [IceprintPort]
    public UnityEvent<int, string, bool> testEvent2;
    //[Header("testHeader")]
    [IceprintPort]
    [Label("标签te")]
    public UnityEvent<int> testEvent;

    [Group]
    public string testStr;
    public float a;
    [RuntimeConst]
    [Label("BBB")] public bool b;
    public int i;
    public Color c;
    public Vector2 v2;
    [RuntimeConst]
    public Vector3 v3;
    public Vector4 v4;

    public IceEngine.IceDictionary<string, GameObject> comps = new IceEngine.IceDictionary<string, GameObject>();
    public enum EE
    {
        asdas,
        dq,
        qwee
    }
    public IceEnumMap<EE, string> map = new IceEnumMap<EE, string>();

    [System.Serializable]
    public class ClassB
    {
        [Label("标签Bb")]
        public int bb;
        [Label("标签Bd")]
        public int bd;
    }
    [Label("标签Bs")]
    [RuntimeConst]
    public ClassB bs;

    [IceprintPort]
    public Action<string> onJJJ;

    [Button]
    [IceprintPort]
    public void DoSomething()
    {
        onJJJ?.Invoke("onJJJ");
        onEvt?.Invoke("onEvt");
    }

    [IceprintPort]
    public UnityEvent<string> onEvt;

    public void DebugLog(string msg)
    {
        Debug.Log(msg);
    }
}