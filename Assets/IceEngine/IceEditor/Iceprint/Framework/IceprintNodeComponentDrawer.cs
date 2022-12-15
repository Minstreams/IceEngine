using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Framework
{
    namespace Internal
    {
        public class IceprintNodeComponentDrawer
        {
            #region Configuration
            internal virtual Type NodeType => null;
            public virtual GUIStyle StlGraphNodeTitle => _stlGraphNodeTitle?.Check() ?? (_stlGraphNodeTitle = new GUIStyle("CN CountBadge") { margin = new RectOffset(4, 4, 4, 4), padding = new RectOffset(6, 6, 2, 2), fontSize = 16, fontStyle = FontStyle.Bold, fixedHeight = 0f, stretchWidth = false, }.Initialize(stl => { stl.normal.textColor = new Color(0f, 0f, 0f); stl.hover.textColor = IceGUIUtility.CurrentThemeColor; })); GUIStyle _stlGraphNodeTitle;
            public virtual Vector2 GetSizeTitle(MonoBehaviour node) => StlGraphNodeTitle.CalcSize(TempContent(GetDisplayName(node))) + new Vector2(StlGraphNodeTitle.margin.horizontal, StlGraphNodeTitle.margin.vertical);
            public virtual Vector2 GetSizeBody(MonoBehaviour node) => new(192, 32);
            public virtual void OnGUI_Title(MonoBehaviour node, Rect rect) => StyleBox(rect.Resize(GetSizeTitle(node)), StlGraphNodeTitle, GetDisplayName(node), true, rect.MoveEdge(bottom: GetSizeBody(node).y).Contains(E.mousePosition));
            public virtual void OnGUI_Body(MonoBehaviour node, Rect rect)
            {
                StyleBox(rect, StlLabelNonWrap, node.GetPath(), true);
            }
            public virtual string GetDisplayName(MonoBehaviour node) => node.ToString();
            #endregion
        }
    }

    public abstract class IceprintNodeComponentDrawer<NodeComp> : Internal.IceprintNodeComponentDrawer where NodeComp : MonoBehaviour
    {
        internal sealed override Type NodeType => typeof(NodeComp);
        public sealed override Vector2 GetSizeTitle(MonoBehaviour node) => GetSizeTitle(node as NodeComp);
        public virtual Vector2 GetSizeTitle(NodeComp node) => base.GetSizeTitle(node);
        public sealed override void OnGUI_Title(MonoBehaviour node, Rect rect) => OnGUI_Title(node as NodeComp, rect);
        public virtual void OnGUI_Title(NodeComp node, Rect rect) => base.OnGUI_Title(node, rect);
        public sealed override Vector2 GetSizeBody(MonoBehaviour node) => GetSizeBody(node as NodeComp);
        public virtual Vector2 GetSizeBody(NodeComp node) => base.GetSizeBody(node);
        public sealed override void OnGUI_Body(MonoBehaviour node, Rect rect) => OnGUI_Body(node as NodeComp, rect);
        public virtual void OnGUI_Body(NodeComp node, Rect rect) => base.OnGUI_Body(node, rect);
        public sealed override string GetDisplayName(MonoBehaviour node) => GetDisplayName(node as NodeComp);
        public virtual string GetDisplayName(NodeComp node) => typeof(NodeComp).Name;
    }
}
