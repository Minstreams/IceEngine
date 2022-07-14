using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IceEngine.Internal
{
    /// <summary>
    /// 序列化的图节点
    /// </summary>
    [Serializable]
    public class IceGraphNode
    {
        // 临时数据
        [NonSerialized] public IceGraph graph;
        [NonSerialized] public int nodeId;

        // 基本信息
        public List<IceGraphPort> outports = new();
        public List<IceGraphPort> inports = new();

        public void AddOutport(string name, Type valueType = null) => outports.Add(new IceGraphPort(name, valueType, this, outports.Count, true, false));
        public void AddOutport<T>(string name) => outports.Add(new IceGraphPort(name, typeof(T), this, outports.Count, true, false));
        public void AddInport(string name, Type valueType = null) => inports.Add(new IceGraphPort(name, valueType, this, inports.Count, false, false));
        public void AddInport<T>(string name) => inports.Add(new IceGraphPort(name, typeof(T), this, inports.Count, false, false));
        public void AddOutportMultiple(string name, Type valueType = null) => outports.Add(new IceGraphPort(name, valueType, this, outports.Count, true, true));
        public void AddOutportMultiple<T>(string name) => outports.Add(new IceGraphPort(name, typeof(T), this, outports.Count, true, true));
        public void AddInportMultiple(string name, Type valueType = null) => inports.Add(new IceGraphPort(name, valueType, this, inports.Count, false, true));
        public void AddInportMultiple<T>(string name) => inports.Add(new IceGraphPort(name, typeof(T), this, inports.Count, false, true));

        // 数据托管
        [SerializeField] IceDictionary<string, bool> _boolMap = new();
        [SerializeField] IceDictionary<string, int> _intMap = new();
        [SerializeField] IceDictionary<string, float> _floatMap = new();
        [SerializeField] public IceDictionary<string, string> _stringMap = new();

        #region GUI

        #region 基本字段
        public Vector2 position;
        public bool folded;
        #endregion

        #region GUI Property
        public Rect Area => new Rect(position, Size);
        public Vector2 Size => folded ? SizeFolded : SizeUnfolded;
        public Vector2 SizeUnfolded => new Vector2
        (
            Mathf.Max(SizeBody.x, SizeFolded.x),
            Mathf.Max(SizeBody.y + SizeFolded.y, inports.Count * IceGraphPort.PORT_SIZE, outports.Count * IceGraphPort.PORT_SIZE)
        );
        public Vector2 SizeFolded => SizeTitle;
        public virtual Vector2 SizeTitle => ((GUIStyle)"label").CalcSize(new GUIContent("测试标题"));
        public virtual Vector2 SizeBody => new Vector2(128, 64);
        #endregion

        #endregion
    }

}
