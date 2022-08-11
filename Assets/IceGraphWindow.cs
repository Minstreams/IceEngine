using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Graph;
using IceEngine.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System;
using IceEditor.Framework;

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
            drawer = new IceGraphDrawer(g);
        }


        public IceGraph g = new IceGraph();
        public IceGraphDrawer drawer;


        protected override void OnWindowGUI(Rect position)
        {
            using (DOCK)
            {
                if (IceButton("New Node"))
                {
                    var node = new IceGraphNode();
                    g.AddNode(node);
                }
                Space();
                Label($"Node Count: {g.nodeList.Count}");
            }

            using (DOCK)
            {
                if (IceButton("New Node"))
                {
                    var node = new IceGraphNode();
                    g.AddNode(node);
                }
                Space();
                Label($"Node Count: {g.nodeList.Count}");
            }


            using (HORIZONTAL)
            {
                using (VERTICAL)
                {
                    Label("BBBBBBBBBBBBBBBBB");
                }
                using (ScrollVertical("Sec"))
                {
                    Label("AAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCCC");
                    Label("AAAAAAAAAAAAAAA");
                    var r = GetRect(300, 300, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    drawer.OnGUI(r, stlBackGround: StlDock);
                    //if (E.type == EventType.MouseMove) Repaint();
                    Label("BBBBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCCC");
                    Label("AAAAAAA");
                    Label("BBBBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCCC");
                    Label("AAAAAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCCC");
                    Label("AAAAAAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCCC");
                    Label("AAAAAAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCCC");
                    Label("AAAAAAAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBBBBBB");
                    Label("CCCCCCAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCC");
                    Label("AAAAAAAAAAAAAAAA");
                    Label("BBBBBBBBBBBBBBBBB");
                    Label("CCCCCCCCCCCCCCCCC");
                }
            }
        }

        protected override void OnExtraGUI(Rect position)
        {
            //Rect workspace = position.ApplyBorder(-32).MoveEdge(top: 32);

            //drawer.GraphArea(this.position, workspace, stlBackGround: StlDock);
            //if (E.type == EventType.MouseMove) Repaint();
        }

    }
}