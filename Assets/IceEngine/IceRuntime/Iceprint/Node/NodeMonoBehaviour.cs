using System;
using UnityEngine;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("MonoBehaviour")]
    public class NodeMonoBehaviour : IceprintNode
    {
        #region Serialized Data
        public Type targetType;
        public int targetInstanceId;
        #endregion

        #region Cache
        [NonSerialized] public IceprintNodeComponent target;

        #endregion

        public override void InitializePorts()
        {
            if (targetType == null) return;

            if (target == null)
                target = IceprintNodeComponent.FromInstanceId(targetInstanceId);
            InitializePorts(targetType, target);
        }
    }
}
