using System.Collections;
using UnityEngine;

namespace IceEngine.Internal
{
    /// <summary>
    /// IceIsland在场景中的组件实例
    /// </summary>
    internal class IceIslandComponent : MonoBehaviour
    {
        void Awake()
        {
            IceIsland.CallSubSystem("Awake");
        }
        void Start()
        {
            IceIsland.CallSubSystem("Start");
        }
    }
}