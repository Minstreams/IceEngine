using UnityEngine;

namespace IceEngine.Framework
{
    public class IceprintNodeComponent : MonoBehaviour
    {
        static readonly IceDictionary<int, IceprintNodeComponent> _nodeMap = new();
        public static IceprintNodeComponent FromInstanceId(int instanceId)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return (IceprintNodeComponent)UnityEditor.EditorUtility.InstanceIDToObject(instanceId);
#endif
            return _nodeMap.TryGetValue(instanceId, out var val) ? val : null;
        }

        protected virtual void Awake() => _nodeMap.Add(GetInstanceID(), this);
        protected virtual void OnDestroy() => _nodeMap.Remove(GetInstanceID());
    }
}
