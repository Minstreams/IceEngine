using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.Blueprint
{
    /// <summary>
    /// 蓝图组件基类
    /// </summary>
    public class IceBlueprintBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {
        [HideInInspector] public IceBlueprint blueprint;

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // 在这里更新初始节点
            //Debug.Log("IceBlueprintBehaviour.OnAfterDeserialize");
            IceBlueprintEntryNode entryNode;
            if (blueprint.nodeList.Count <= 0)
            {
                // 初次初始化
                blueprint.AddNode(entryNode = new IceBlueprintEntryNode());
            }
            else
            {
                // 更改
                entryNode = (IceBlueprintEntryNode)blueprint.nodeList[0];
                //blueprint.nodeList[0] = entryNode = new IceBlueprintEntryNode() { graph = blueprint, nodeId = 0 };
            }

            // 开始处理Ports
            var t = GetType();
            var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            entryNode.inports.Clear();

            foreach (var m in ms)
            {
                if (m.GetCustomAttribute<PortAttribute>() != null)
                {
                    entryNode.AddInport(m.Name);
                    entryNode._stringMap.Add(m.Name, "mark");
                }
            }
#endif
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // 不需要做什么
        }
    }
}