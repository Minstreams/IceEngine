using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// 冰屿
    /// </summary>
    public class IceIsland : MonoBehaviour
    {


        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string text, Object context) => Debug.Log(text, context);
    }
}
