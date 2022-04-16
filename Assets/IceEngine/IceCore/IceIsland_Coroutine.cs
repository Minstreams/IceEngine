using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine
{
    using IceCoroutine = LinkedListNode<Coroutine>;

    public static partial class IceIsland
    {
        static readonly Dictionary<System.Type, LinkedList<Coroutine>> _routineMap = new Dictionary<System.Type, LinkedList<Coroutine>>();

        /// <summary>
        /// 开始协程
        /// </summary>
        /// <param name="routine">协程</param>
        /// <param name="key">作为索引的类型</param>
        public static IceCoroutine StartCoroutine(IEnumerator routine, System.Type key)
        {
            static IEnumerator SubCoroutine(IEnumerator inCoroutine, IceCoroutine node)
            {
                yield return inCoroutine;
                node.List.Remove(node);
            }

            if (!_routineMap.TryGetValue(key, out var linkedList))
            {
                _routineMap[key] = linkedList = new LinkedList<Coroutine>();
            }
            var node = new IceCoroutine(null);
            node.Value = Instance.StartCoroutine(SubCoroutine(routine, node));
            linkedList.AddLast(node);

            return node;
        }
        /// <summary>
        /// 停止一个类型的所有协程
        /// </summary>
        /// <param name="key">作为索引的类型</param>
        public static void StopAllCoroutines(System.Type key)
        {
            if (!_routineMap.TryGetValue(key, out var linkedList)) return;

            foreach (Coroutine c in linkedList)
            {
                Instance.StopCoroutine(c);
            }

            linkedList.Clear();
        }
        /// <summary>
        /// 停止一个StartCoroutine开始的协程
        /// </summary>
        /// <param name="node">要停止的协程对象</param>
        public static void StopCoroutine(IceCoroutine node)
        {
            if (node == null || node.List == null) return;
            Instance.StopCoroutine(node.Value);
            node.List.Remove(node);
        }
    }
}
