using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Test/asdasd")]
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
    public EnumMap<EE, string> map = new EnumMap<EE, string>();
}
