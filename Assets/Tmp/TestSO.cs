﻿using System.Collections;
using UnityEngine;
using IceEngine;
using IceEngine.Internal;

public class TestSO : ScriptableObject
{
    [System.Serializable]
    public class TestCC { public int x; public int y; public Vector2 s; }
    [System.Serializable]
    public class TestC { public int x; public int y; public Vector2 s; public TestCC cc; }
    public IceEngine.IceDictionary<string, GameObject> comps = new IceEngine.IceDictionary<string, GameObject>();
    public enum EE
    {
        asdas,
        dq,
        qwee
    }
    public IceEnumMap<EE, string> map = new IceEnumMap<EE, string>();
    [Label("ASS")]
    public float a;


    public void SDASD()
    {

    }
}
