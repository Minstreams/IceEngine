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
    IceprintGraph graph = new IceprintGraph();
    public IceGraphDrawer<IceprintNode> drawer;
    public byte[] data = null;
    public byte[] buffer = null;
    public byte[] diff = null;

    [MenuItem("测试/Graph")]
    static void OpenWindow() => GetWindow<TestGraphWindow>();

    protected override void OnEnable()
    {
        base.OnEnable();
        wantsMouseMove = true;
        drawer = new(graph, Repaint);
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
                        graph.AddNode<TestprintNode>();
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
                            data = graph.Serialize();
                        }
                    }
                    if (Button("Read!"))
                    {
                        graph.Deserialize(data);
                    }
                }
                using (HORIZONTAL)
                {
                    if (Button("Save Bytes[]"))
                    {
                        buffer = data;
                    }
                    if (Button("Get Diff"))
                    {
                        SetString("Console", "");
                        using (new IceBinaryUtility.LogScope(OnLog))
                        {
                            diff = IceBinaryUtility.GetDiff(data, buffer);
                        }
                    }
                    if (Button("Apply Diff"))
                    {
                        data = IceBinaryUtility.ApplyDiff(data, diff);
                    }
                    if (Button("Reverse Diff"))
                    {
                        data = IceBinaryUtility.ReverseDiff(data, diff);
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
