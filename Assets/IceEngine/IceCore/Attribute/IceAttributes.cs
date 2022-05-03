using System;
using UnityEngine;

namespace IceEngine
{
    /// <summary>
    /// 代替标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelAttribute : Attribute
    {
        public readonly string label;
        public LabelAttribute(string label)
        {
            this.label = label;
        }
    }

    /// <summary>
    /// 运行时不可更改
    /// </summary>
    public class RuntimeConst : Attribute { }
}