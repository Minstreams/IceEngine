using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 与枚举挂钩的Map基类，用于反射
/// </summary>
public class EnumMap { }

/// <summary>
/// 与枚举挂钩的Map，本质是List，通过Editor扩展实现与枚举挂钩
/// 因此关联的枚举类型的数字映射必须为0到length-1
/// </summary>
/// <typeparam name="ET">Enum Type 关联的枚举类型</typeparam>
/// <typeparam name="DT">Data Type 数据类型</typeparam>
[System.Serializable]
public class EnumMap<ET, DT> : EnumMap, IEnumerable<DT> where ET : System.Enum
{
    [SerializeField] ET _;

    public List<DT> list = new List<DT>(System.Enum.GetNames(typeof(ET)).Length);
    public DT this[ET key]
    {
        get => this.list[(int)(object)key];
        set => this.list[(int)(object)key] = value;
    }
    IEnumerator<DT> IEnumerable<DT>.GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}
