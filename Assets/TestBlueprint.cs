using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IceEngine;
using IceEngine.Blueprint;

[DisallowMultipleComponent]
public class TestBlueprint : IceBlueprintBehaviour
{
    public float tfloat;

    [Port]
    public void TestIn()
    {

    }

    [Port]
    public void TestIn2()
    {

    }
}
