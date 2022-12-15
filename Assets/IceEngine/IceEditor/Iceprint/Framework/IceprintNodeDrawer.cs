using System;
using System.Text.RegularExpressions;

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
        public class IceprintNodeDrawer
        {
            readonly static Regex upperAlphaRegex = new("(?<!^)[A-Z]");
            internal static string GetNodeDisplayName(string name)
            {
                if (name.StartsWith("Node")) name = name.Substring(4);
                return upperAlphaRegex.Replace(name, " $0");
            }

            #region Configuration
            internal virtual Type NodeType => null;
            public virtual GUIStyle StlGraphNodeBackground => _stlGraphNodeBackground?.Check() ?? (_stlGraphNodeBackground = new GUIStyle("AnimationEventTooltip") { overflow = new RectOffset(2, 2, 2, 2), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlGraphNodeBackground;
            public virtual GUIStyle StlGraphNodeTitle => _stlGraphNodeTitle?.Check() ?? (_stlGraphNodeTitle = new GUIStyle("CN CountBadge") { margin = new RectOffset(4, 4, 4, 4), padding = new RectOffset(6, 6, 2, 2), fontSize = 16, fontStyle = FontStyle.Bold, fixedHeight = 0f, stretchWidth = false, }.Initialize(stl => { stl.normal.textColor = new Color(0f, 0f, 0f); stl.hover.textColor = IceGUIUtility.CurrentThemeColor; })); GUIStyle _stlGraphNodeTitle;
            public virtual Vector2 GetSizeTitle(IceprintNode node) => StlGraphNodeTitle.CalcSize(TempContent(GetDisplayName(node))) + new Vector2(StlGraphNodeTitle.margin.horizontal, StlGraphNodeTitle.margin.vertical);
            public virtual void OnGUI_Title(IceprintNode node, Rect rect) => StyleBox(rect.Resize(GetSizeTitle(node)), StlGraphNodeTitle, GetDisplayName(node), true, node.GetArea().Contains(E.mousePosition));
            public virtual Vector2 GetSizeBody(IceprintNode node) => new(128, 32);
            public virtual void OnGUI_Body(IceprintNode node, Rect rect) { }
            public virtual string GetDisplayName(IceprintNode node) => GetNodeDisplayName(node.GetType().Name);
            public virtual void OnSingleSelect(IceprintNode node) { }
            #endregion
        }
    }
    public abstract class IceprintNodeDrawer<Node> : Internal.IceprintNodeDrawer where Node : IceprintNode
    {
        internal sealed override Type NodeType => typeof(Node);
        public sealed override Vector2 GetSizeTitle(IceprintNode node) => GetSizeTitle(node as Node);
        public virtual Vector2 GetSizeTitle(Node node) => base.GetSizeTitle(node);
        public sealed override void OnGUI_Title(IceprintNode node, Rect rect) => OnGUI_Title(node as Node, rect);
        public virtual void OnGUI_Title(Node node, Rect rect) => base.OnGUI_Title(node, rect);
        public sealed override Vector2 GetSizeBody(IceprintNode node) => GetSizeBody(node as Node);
        public virtual Vector2 GetSizeBody(Node node) => base.GetSizeBody(node);
        public sealed override void OnGUI_Body(IceprintNode node, Rect rect) => OnGUI_Body(node as Node, rect);
        public virtual void OnGUI_Body(Node node, Rect rect) => base.OnGUI_Body(node, rect);
        public sealed override string GetDisplayName(IceprintNode node) => GetDisplayName(node as Node);
        public virtual string GetDisplayName(Node node) => GetNodeDisplayName(typeof(Node).Name);
        public sealed override void OnSingleSelect(IceprintNode node) => OnSingleSelect(node as Node);
        public virtual void OnSingleSelect(Node node) { }
    }
}