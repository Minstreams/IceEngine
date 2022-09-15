using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    namespace Internal
    {
        /// <summary>
        /// 与枚举挂钩的Map基类，用于反射
        /// </summary>
        [HasPropertyDrawer]
        public class IceEnumMap { }
    }

    /// <summary>
    /// 与枚举挂钩的Map，本质是List，通过Editor扩展实现与枚举挂钩
    /// 因此关联的枚举类型的数字映射必须为0到length-1
    /// </summary>
    /// <typeparam name="ET">Enum Type 关联的枚举类型</typeparam>
    /// <typeparam name="DT">Data Type 数据类型</typeparam>
    [System.Serializable]
    public class IceEnumMap<ET, DT> : Internal.IceEnumMap, IEnumerable<DT> where ET : System.Enum
    {
        /// <summary>
        /// 用于SerializedProperty获取ET的反射信息
        /// TODO:试试优化这一项
        /// </summary>
        [SerializeField] ET _;
        [System.NonSerialized] DT defaultValue = default;

        public IceEnumMap() { }
        public IceEnumMap(DT defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public List<DT> list = new(System.Enum.GetNames(typeof(ET)).Length);
        public DT this[ET key]
        {
            get => this.list[ToIndex(key)];
            set => this.list[ToIndex(key)] = value;
        }
        int ToIndex(ET key)
        {
            int i = (int)(object)key;
            while (list.Count <= i) list.Add(defaultValue);
            return i;
        }
        IEnumerator<DT> IEnumerable<DT>.GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    }
}