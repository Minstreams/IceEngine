using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    public class NodeMissing : IceprintNode, IPacketSerializationHashcode
    {
        public ushort Hashcode { get; set; }
        public override void Initialize()
        {
            for (int i = 0; i < connectionData.Count; ++i)
            {
                AddOutport($"Output{i}");
            }

            for (int i = 0; i < 16; ++i)
            {
                AddInport($"Input{i}");
            }
        }

        ushort IPacketSerializationHashcode.GetHashcode() => Hashcode;
    }
}
