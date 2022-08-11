using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.Graph;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Framework
{
    namespace Internal
    {
        public class IceGraphNodeDrawer
        {
            #region Static Interface
            static readonly IceGraphNodeDrawer _defaultDrawer = new IceGraphNodeDrawer();
            static Dictionary<Type, IceGraphNodeDrawer> _nodeDrawerMap = null;
            static Dictionary<Type, IceGraphNodeDrawer> NodeDrawerMap
            {
                get
                {
                    if (_nodeDrawerMap == null)
                    {
                        _nodeDrawerMap = new();
                        var drawers = TypeCache.GetTypesDerivedFrom<IceGraphNodeDrawer>();
                        foreach (var dt in drawers)
                        {
                            if (dt.IsGenericType) continue;
                            var drawer = (Internal.IceGraphNodeDrawer)Activator.CreateInstance(dt);
                            if (!_nodeDrawerMap.TryAdd(drawer.NodeType, drawer)) throw new Exception($"Collecting drawer [{dt.FullName}] failed! [{drawer.NodeType}] already has a drawer [{_nodeDrawerMap[drawer.NodeType]}]");
                        }
                    }
                    return _nodeDrawerMap;
                }
            }
            public static IceGraphNodeDrawer GetDrawer(IceGraphNode node) => NodeDrawerMap.TryGetValue(node.GetType(), out var drawer) ? drawer : _defaultDrawer;
            #endregion

            #region Interface
            public void OnGUI(IceGraphNode node, float gridSize = 32)
            {
                // Rect Calculation
                var nodeRect = node.GetArea();
                var titleRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, node.GetSizeTitle().y);

                // DoubleClick To Fold
                {
                    if (E.type == EventType.MouseDown)
                    {
                        if (titleRect.Contains(E.mousePosition))
                        {
                            double t = EditorApplication.timeSinceStartup;
                            if (t - timeCache > 0.2)
                            {
                                timeCache = t;
                            }
                            else
                            {
                                timeCache = 0;
                                // 双击
                                node.folded = !node.folded;
                            }
                        }
                    }
                }

                // Drag Control
                {
                    int idDragNode = GetControlID();
                    switch (E.type)
                    {
                        case EventType.MouseDown:
                            if (titleRect.Contains(E.mousePosition) && GUIHotControl == 0 && E.button == 0)
                            {
                                GUIHotControl = idDragNode;
                                dragCache = node.position - E.mousePosition;
                                offsetCache = node.position;
                                E.Use();
                            }
                            break;
                        case EventType.MouseUp:
                            if (GUIHotControl == idDragNode && E.button == 0)
                            {
                                GUIHotControl = 0;
                                E.Use();
                            }
                            break;
                        case EventType.MouseDrag:
                            if (GUIHotControl == idDragNode)
                            {
                                node.position = dragCache + E.mousePosition;
                                if (E.shift)
                                {
                                    // 平移操作
                                    var offset = node.position - offsetCache;
                                    if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                                    {
                                        // 沿x轴平移
                                        node.position.y = offsetCache.y;
                                    }
                                    else
                                    {
                                        // 沿y轴平移
                                        node.position.x = offsetCache.x;
                                    }
                                }
                                if (E.control)
                                {
                                    // 网格吸附操作
                                    node.position = node.position.Snap(gridSize);
                                }
                                E.Use();
                            }
                            break;
                        case EventType.Repaint:
                            if (GUIHotControl == idDragNode)
                            {
                                var holderRect = new Rect(nodeRect) { position = offsetCache };

                                // 在原始位置画一个残影
                                using (GUIColor(Color.white * 0.6f)) StyleBox(holderRect, StlGraphNodeBackground);

                                // TODO: 复制操作
                                if (E.alt)
                                {
                                    // TODO:在原始位置画一个残影
                                }
                            }
                            break;
                    }
                }

                // Draw
                StyleBox(nodeRect, StlGraphNodeBackground);
                OnGUI_Title(node, titleRect);
                if (!node.folded)
                {
                    OnGUI_Body(node, new Rect(node.position, node.GetSizeBody()).Move(y: node.GetSizeFolded().y));
                }
            }

            #endregion

            internal virtual Type NodeType => null;

            #region Configuration
            protected virtual GUIStyle StlGraphNodeBackground => StlNode;
            public virtual Vector2 GetSizeBody(IceGraphNode node) => new(128, 64);
            public virtual Vector2 GetSizeTitle(IceGraphNode node) => ((GUIStyle)"label").CalcSize(new GUIContent("测试标题"));
            protected virtual void OnGUI_Title(IceGraphNode node, Rect rect)
            {
                using (AreaRaw(rect)) Label("空节点");
            }
            protected virtual void OnGUI_Body(IceGraphNode node, Rect rect)
            {
                using (AreaRaw(rect)) Label("……");
            }
            #endregion
        }
    }
    public abstract class IceGraphNodeDrawer<Node> : Internal.IceGraphNodeDrawer where Node : IceGraphNode
    {
        internal override Type NodeType => typeof(Node);
    }
}