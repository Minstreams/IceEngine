using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System;

namespace IceEditor
{
    public class IceGraphWindow : IceEditorWindow
    {
        #region 定制
        [MenuItem("测试/测试图")]
        static void OpenWindow() => GetWindow<IceGraphWindow>();
        protected override string Title => "测试图窗口";
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseMove = true;
        }


        public IceGraph g = new IceGraph();


        protected override void OnWindowGUI(Rect position)
        {
            using (DOCK)
            {
                if (IceButton("New Node"))
                {
                    var node = new IceGraphNode();
                    node.AddOutport<float>("testOut1");
                    node.AddOutport<int>("testOut2");
                    node.AddInportMultiple<float>("V");
                    node.AddInportMultiple<int>("F");
                    g.AddNode(node);
                }
                Space();
                Label($"Node Count: {g.nodeList.Count}");
            }
        }

        protected override void OnExtraGUI(Rect position)
        {
            // 变量定义
            Rect workspace = position.MoveEdge(top: 20).ApplyBorder(-32);

            GraphArea(this.position, workspace, g, stlBackGround: StlDock);
            if (E.type == EventType.MouseMove) Repaint();
        }

    }
}