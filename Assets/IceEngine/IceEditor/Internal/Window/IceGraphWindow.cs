using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
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
            //wantsMouseMove = true;
        }

        GUIStyle StlGraphNodeBody => _stlGraphNodeBody?.Check() ?? (_stlGraphNodeBody = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game).FindStyle("window"))); GUIStyle _stlGraphNodeBody;

        public IceGraph g = new IceGraph();
        public void GraphArea(Rect area, IceGraph g, GUIStyle stlBackGround = null)
        {
            using var viewport = ViewportGrid("GraphView", this.position, area, 32, gridColor: ThemeColor * 0.5f, styleBackground: stlBackGround);

            foreach (var node in g.nodeList)
            {
                var posRect = new Rect(node.position, Vector2.one * 128);

                // Draw
                {
                    //StlGraphNodeBody.fontSize = (int)(scale * 0.2f);
                    StyleBox(posRect, StlGraphNodeBody, "测试文字", isHover: null);

                    Handles.DrawLine(posRect.center, E.mousePosition);

                    // 添加节点按钮
                    {
                        var rAddOut = new Rect(posRect.x - 16, posRect.y, 16, 16);
                        if (GUI.Button(rAddOut, GUIContent.none, "OL Plus"))
                        {
                            //node.outputPorts
                        }
                    }
                }

                // Drag Control
                {
                    int idDragNode = GetControlID();
                    switch (E.type)
                    {
                        case EventType.MouseDown:
                            if (posRect.Contains(E.mousePosition) && GUIHotControl == 0)
                            {
                                GUIHotControl = idDragNode;
                                dragCache = node.position - E.mousePosition;
                                E.Use();
                            }
                            break;
                        case EventType.MouseUp:
                            if (GUIHotControl == idDragNode)
                            {
                                GUIHotControl = 0;
                                E.Use();
                            }
                            break;
                        case EventType.MouseDrag:
                            if (GUIHotControl == idDragNode)
                            {
                                node.position = dragCache + E.mousePosition;
                                E.Use();
                            }
                            break;
                    }
                }
            }
        }
        protected override void OnWindowGUI(Rect position)
        {
            using (DOCK)
            {
                if (IceButton("New Node"))
                {
                    g.nodeList.Add(new IceGraphNode());
                }
                Space();
                Label($"Node Count: {g.nodeList.Count}");
            }

        }

        //ref 变量
        float viewScale;
        Vector2 viewOffset;
        Vector2 dragOffset;

        protected override void OnExtraGUI(Rect position)
        {
            // 变量定义
            Rect workspace = position.ApplyBorder(-32);

            GraphArea(workspace, g, StlDock);
        }

    }
}
