using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceEngine;

public class TestComp : MonoBehaviour
{
    [System.Serializable]
    public class IntEvent : UnityEngine.Events.UnityEvent<int> { }
    public UnityEngine.Events.UnityEvent<int> testEvent2;
    [Label] public IntEvent testEvent;

    [Label] public string testStr;
    [SerializeField] public float a;

    [System.Serializable]
    public class ClassB
    {
        public int bb;
    }
    public ClassB b;
    void Awake()
    {
        Debug.Log("AwakeTemp!");
    }
}