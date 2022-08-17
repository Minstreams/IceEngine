using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    namespace Internal
    {
        [HasPropertyDrawer]
        public class IceDictionary { }
    }

    /// <summary>
    /// 可序列化的Dictionary
    /// </summary>
    [System.Serializable]
    public class IceDictionary<TKey, TValue> : Internal.IceDictionary, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] List<TKey> m_Keys = new List<TKey>();
        [SerializeField] List<TValue> m_Values = new List<TValue>();

        bool Dirty { get; /*private */set; } = false;
        readonly Dictionary<TKey, TValue> m_Dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get => m_Dictionary[key]; set
            {
                if (m_Dictionary.ContainsKey(key))
                {
                    m_Dictionary[key] = value;
                    Dirty = true;
                }
                else
                {
                    m_Dictionary.Add(key, value);
                    m_Keys.Add(key);
                    m_Values.Add(value);
                }
            }
        }

        public List<TKey> Keys => m_Keys;
        public List<TValue> Values
        {
            get
            {
                if (Dirty)
                {
                    for (int i = 0; i < m_Keys.Count; ++i) m_Values[i] = m_Dictionary[m_Keys[i]];
                    Dirty = false;
                }
                return m_Values;
            }
        }
        public int Count => m_Dictionary.Count;

        public void Add(TKey key, TValue value)
        {
            this[key] = value;
        }
        public void Clear()
        {
            m_Dictionary.Clear();
            m_Keys.Clear();
            m_Values.Clear();
        }
        public bool ContainsKey(TKey key) => m_Dictionary.ContainsKey(key);
        public bool Remove(TKey key)
        {
            if (m_Dictionary.Remove(key))
            {
                int index = m_Keys.IndexOf(key);
                m_Keys.RemoveAt(index);
                m_Values.RemoveAt(index);
                return true;
            }
            return false;
        }
        public bool TryGetValue(TKey key, out TValue value) => m_Dictionary.TryGetValue(key, out value);

        #region 显式实现接口
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            int kc = m_Keys.Count;
            int vc = m_Values.Count;
            int count = Mathf.Min(kc, vc);

            // 去除多余数据
            if (kc != vc)
            {
                if (kc > vc) m_Keys.RemoveRange(count, kc - count);
                else m_Values.RemoveRange(count, vc - count);
            }

            // 构造m_dic
            m_Dictionary.Clear();
            for (int i = 0; i < count; ++i) if (!m_Dictionary.ContainsKey(m_Keys[i])) m_Dictionary.Add(m_Keys[i], m_Values[i]);

            Dirty = false;
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // 还原数据=》 List
            if (!Dirty) return;

            for (int i = 0; i < m_Keys.Count; ++i) m_Values[i] = m_Dictionary[m_Keys[i]];

            Dirty = false;
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => m_Dictionary.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => m_Dictionary.Values;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
            m_Keys.Add(item.Key);
            m_Values.Add(item.Value);
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => (m_Dictionary as IEnumerable<KeyValuePair<TKey, TValue>>).GetEnumerator();
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var collection = m_Dictionary as ICollection<KeyValuePair<TKey, TValue>>;
            if (collection.Remove(item))
            {
                int index = m_Keys.IndexOf(item.Key);
                m_Keys.RemoveAt(index);
                m_Values.RemoveAt(index);
                return true;
            }
            return false;
        }
        IEnumerator IEnumerable.GetEnumerator() => (m_Dictionary as IEnumerable).GetEnumerator();
        #endregion
    }
}