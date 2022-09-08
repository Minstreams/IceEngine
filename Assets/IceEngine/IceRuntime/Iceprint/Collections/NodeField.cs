using System;
using IceEngine.Framework;

namespace IceEngine
{
    namespace Internal
    {
        public abstract class NodeField
        {
            public abstract void Initialize(IceprintNode node);
        }
    }
    /// <summary>
    /// 为Node内部提供引用字段
    /// </summary>
    [Serializable]
    public class NodeField<T> : Internal.NodeField where T : UnityEngine.Object
    {
        public T Value
        {
            get => _cache;
            set => node.graph.SetField(node.id, id, _cache = value);
        }

        [NonSerialized] IceprintNode node;
        [NonSerialized] T _cache;
        public int id = -1;
        public sealed override void Initialize(IceprintNode node)
        {
            this.node = node;
            _cache = node.graph.GetField<T>(node.id, ref id);
        }
    }
}
