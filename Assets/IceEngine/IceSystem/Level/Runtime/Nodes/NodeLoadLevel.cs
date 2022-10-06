using System;
using System.Net;
using UnityEngine;
using IceEngine.Framework;
using IceEngine.Networking.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Ice/Level/LoadLevel")]
    public class NodeLoadLevel : IceprintNode
    {
        // Ports
        [IceprintPort] public void LoadLevel(string sceneName) => Ice.Level.LoadLevel(sceneName);
    }
}