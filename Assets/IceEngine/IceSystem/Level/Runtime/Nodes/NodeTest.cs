using IceEngine.Framework;
using System;
using Sys = Ice.Level;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("Ice/Level/Test")]
    [IcePacket(Hashcode = 15187)]
    public class NodeTest : IceprintNode
    {
        [IceprintPort]
        Action<string> onOut; 
        // Ports
        [IceprintPort] public void Test() { }
    }
}
