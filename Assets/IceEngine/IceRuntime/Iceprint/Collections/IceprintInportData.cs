using System;

namespace IceEngine.Internal
{
    [IcePacket]
    public sealed class IceprintInportData
    {
        #region Cache
        [NonSerialized] public IceprintInport port;
        [NonSerialized] public object action;
        #endregion

        public int nodeId;
        public int portId;
    }
}
