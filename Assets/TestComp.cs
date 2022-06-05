using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceEngine;

[ThemeColor(1, 0, 1)]
public class TestComp : MonoBehaviour
{
    [System.Serializable]
    public class IntEvent : UnityEngine.Events.UnityEvent<int> { }
    public UnityEngine.Events.UnityEvent<int> testEvent2;
    [Header("testHeader")]
    [Label("标签te")]
    public IntEvent testEvent;

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
    public EnumMap<EE, string> map = new EnumMap<EE, string>();

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
    void Awake()
    {
        Debug.Log("AwakeTemp!");
    }

    [Button]
    public void DoSomething()
    {
        Debug.Log("Fff" + bs.bb);
    }
}