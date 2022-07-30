using System.Collections;
using UnityEngine;

namespace IceEngine.Internal
{
    /// <summary>
    /// Island在场景中的组件实例
    /// </summary>
    internal class IslandComponent : MonoBehaviour
    {
        void Awake()
        {
            Ice.Island.CallSubSystem("Awake");
        }
        void Start()
        {
            Ice.Island.CallSubSystem("Start");
        }
    }
}