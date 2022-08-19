using System;
using System.Collections.Generic;
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
        public int paramsHash = 0;
        public List<Type> ParamsList
        {
            get => _paramsList; set
            {
                _paramsList = value;

                paramsHash = 0;
                foreach (var pt in _paramsList)
                {
                    paramsHash ^= pt.GetHashCode();
                }
            }
        }
        List<Type> _paramsList;

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

        #region 未来扩展点
        // 未来扩展到其他Graph可能会用到的特性，目前先不动
        public bool isMultiple => true;
        #endregion
    }
}