using System;
using IceEngine.Framework;

namespace IceEngine.IceprintNodes
{
    [IceprintMenuItem("MonoBehaviour")]
    public class NodeMonoBehaviour : IceprintNode
    {
        #region Serialized Data
        public Type targetType;
        public NodeField<IceprintNodeComponent> target = new();
        #endregion

        public override void Initialize()
        {
            target.Initialize(this);
            if (target.Value != null)
            {
                if (target.Value.GetType() != targetType) target.Value = null;
            }
            InitializePorts();
        }

        public void InitializePorts()
        {
            if (targetType == null) return;
            InitializePorts(targetType, target.Value);
        }
    }
}