using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Internal
{
    /// <summary>
    /// 用于绘制一个Node
    /// </summary>
    public class IceGraphNodeDrawer
    {
        #region Public
        public IceGraphNode Target { get; set; }
        public void OnGUI(float gridSize)
        {
            // Rect Calculation
            var nodeRect = Target.Area;
            var titleRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, Target.SizeTitle.y);

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
                            Target.folded = !Target.folded;
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
                            dragCache = Target.position - E.mousePosition;
                            offsetCache = Target.position;
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
                            Target.position = dragCache + E.mousePosition;
                            if (E.shift)
                            {
                                // 平移操作
                                var offset = Target.position - offsetCache;
                                if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                                {
                                    // 沿x轴平移
                                    Target.position.y = offsetCache.y;
                                }
                                else
                                {
                                    // 沿y轴平移
                                    Target.position.x = offsetCache.x;
                                }
                            }
                            if (E.control)
                            {
                                // 网格吸附操作
                                Target.position = Target.position.Snap(gridSize);
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
            OnGUI_Title(titleRect);
            if (!Target.folded)
            {
                OnGUI_Body(new Rect(Target.position, Target.SizeBody).Move(y: Target.SizeFolded.y));
            }
        }
        #endregion

        #region Virtual
        protected virtual GUIStyle StlGraphNodeBackground => StlNode;
        protected virtual void OnGUI_Title(Rect rect)
        {
            using (AreaRaw(rect)) Label("空节点");
        }
        protected virtual void OnGUI_Body(Rect rect)
        {
            using (AreaRaw(rect)) Label("……");
        }
        #endregion
    }
}
