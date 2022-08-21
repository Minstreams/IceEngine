using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Utility/Logger")]
    public class NodeLogger : IceprintNode
    {
        // Field
        public string message;

        // Ports
        [IceprintPort]
        public void Log()
        {
            Debug.Log(message);
        }
        [IceprintPort]
        public void Log(object obj)
        {
            Debug.Log(obj);
        }
        [IceprintPort]
        public void Log(A obj, string s)
        {
            Debug.Log(obj.n + s);
        }
    }

    [IceprintMenuItem("Test")]
    public class NodeTest : IceprintNode
    {
        [IceprintPort]
        public Action<B, string> onOut;
        [IceprintPort]
        public void GG()
        {
            onOut?.Invoke(new B() { n = "Asdadwadasdawdasd" }, "|AAA");
        }
    }

    public class A
    {
        public string n;
    }
    public class B : A
    {

    }
}
