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
                target = (IceprintNodeComponent)UnityEditor.EditorUtility.InstanceIDToObject(targetInstanceId);
            InitializePorts(targetType, target);
        }

        protected override string GetDisplayName()
        {
            if (targetType == null) return base.GetDisplayName();
            return targetType.Name;
        }
    }
}
