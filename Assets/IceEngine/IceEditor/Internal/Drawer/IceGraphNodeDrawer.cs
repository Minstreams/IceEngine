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
    /// ���ڻ���һ��Node
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
                            // ˫��
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
                                // ƽ�Ʋ���
                                var offset = Target.position - offsetCache;
                                if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                                {
                                    // ��x��ƽ��
                                    Target.position.y = offsetCache.y;
                                }
                                else
                                {
                                    // ��y��ƽ��
                                    Target.position.x = offsetCache.x;
                                }
                            }
                            if (E.control)
                            {
                                // ������������
                                Target.position = Target.position.Snap(gridSize);
                            }
                            E.Use();
                        }
                        break;
                    case EventType.Repaint:
                        if (GUIHotControl == idDragNode)
                        {
                            var holderRect = new Rect(nodeRect) { position = offsetCache };

                            // ��ԭʼλ�û�һ����Ӱ
                            using (GUIColor(Color.white * 0.6f)) StyleBox(holderRect, StlGraphNodeBackground);

                            // TODO: ���Ʋ���
                            if (E.alt)
                            {
                                // TODO:��ԭʼλ�û�һ����Ӱ
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
            using (AreaRaw(rect)) Label("�սڵ�");
        }
        protected virtual void OnGUI_Body(Rect rect)
        {
            using (AreaRaw(rect)) Label("����");
        }
        #endregion
    }
}
