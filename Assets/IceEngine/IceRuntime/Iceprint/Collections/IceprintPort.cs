using System;

using IceEngine.Framework;

namespace IceEngine.Internal
{
    public abstract class IceprintPort
    {
        #region Cache
        [NonSerialized] public IceprintNode node;
        [NonSerialized] public int id;
        #endregion

        #region Serialized Data
        // Runtime
        public Type valueType;
        public bool isMultiple => true;

        // Editor
        public string name;
        #endregion

        #region Interface
        public abstract bool IsOutport { get; }
        public abstract bool IsConnected { get; }
        public abstract void ConnectTo(IceprintPort other);
        public abstract void DisconnectFrom(IceprintPort other);
        public abstract void DisconnectAll();
        #endregion
    }
}