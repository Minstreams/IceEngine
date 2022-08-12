using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Graph;
using IceEditor;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using IceEditor.Framework;
using IceEngine.Blueprint;

public class TestGraphWindow : IceEditorWindow, ISerializationCallbackReceiver
{
    IceBlueprint graph = new IceBlueprint();
    public IceGraphDrawer drawer;
    public byte[] buffer = null;
    public byte[] diff = null;

    [MenuItem("测试/Graph")]
    static void OpenWindow() => GetWindow<TestGraphWindow>();

    protected override void OnEnable()
    {
        base.OnEnable();
        wantsMouseMove = true;
        drawer = new IceGraphDrawer(graph, Repaint);
    }
    protected override void OnWindowGUI(Rect position)
    {
        using (SubArea(position, out Rect main, out Rect sub, "AAAAA"))
        {
            using (Area(main))
            {
                using (DOCK)
                {
                    if (IceButton("InputNode"))
                    {
                        graph.AddNode<InputNode>();
                    }
                    if (IceButton("OutputNode"))
                    {
                        graph.AddNode<OutputNode>();
                    }
                    Space();
                    Label($"Node Count: {graph.nodeList.Count}");
                }

                drawer.OnGUI(stlBackGround: StlDock);

                using (HORIZONTAL)
                {
                    if (Button("Serialize!"))
                    {
                        SetString("Console", "");
                        using (new IceBinaryUtility.LogScope(OnLog))
                        {
                            graph.Serialize();
                        }
                    }
                    if (Button("Read!"))
                    {
                        graph.Deserialize();
                    }
                }
                using (HORIZONTAL)
                {
                    if (Button("Save Bytes[]"))
                    {
                        buffer = graph.data;
                    }
                    if (Button("Get Diff"))
                    {
                        SetString("Console", "");
                        using (new IceBinaryUtility.LogScope(OnLog))
                        {
                            diff = IceBinaryUtility.GetDiff(graph.data, buffer);
                        }
                    }
                    if (Button("Apply Diff"))
                    {
                        graph.data = IceBinaryUtility.ApplyDiff(graph.data, diff);
                    }
                    if (Button("Reverse Diff"))
                    {
                        graph.data = IceBinaryUtility.ReverseDiff(graph.data, diff);
                    }
                }

            }
            using (Area(sub))
            {
                using (ScrollInvisible("Console Area Scroll"))
                using (GROUP)
                {
                    Label(GetString("Console"));
                }
            }
        }
    }

    int? baseStack = null;
    void OnLog(string log)
    {
        string curLog = GetString("Console");

        string prefix = "";
        System.Diagnostics.StackTrace st = new();
        int fc = st.FrameCount;
        if (baseStack == null)
        {
            baseStack = fc;
            log = log.Replace("\n", "");
        }
        else
        {
            int indent = fc - baseStack.Value;
            if (indent > 0)
            {
                for (int i = 0; i < indent; ++i) prefix += "|       ".Color(GetColor(i));
                log = log.Replace("\n", $"\n{prefix}");
            }
        }

        Color GetColor(int indent)
        {
            return (indent % 3) switch
            {
                0 => Color.black,
                1 => Color.gray,
                2 => new Color(0.4f, 0.2f, 0.1f),
                _ => Color.red,
            };
        }

        curLog += log;

        SetString("Console", curLog);
    }

    protected override void OnExtraGUI(Rect position)
    {

    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        LogImportant("TestGraphWindow.OnAfterDeserialize");
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        LogImportant("TestGraphWindow.OnBeforeSerialize");
    }
}
