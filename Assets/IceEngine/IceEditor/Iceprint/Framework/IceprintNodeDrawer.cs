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
            public virtual GUIStyle StlGraphNodeBackground => _stlGraphNodeBackground?.Check() ?? (_stlGraphNodeBackground = new GUIStyle("flow node 0") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlGraphNodeBackground;
            public virtual Vector2 GetSizeTitle(IceprintNode node) => new(128, 16);
            public virtual void OnGUI_Title(IceprintNode node, Rect rect) => StyleBox(rect.ApplyBorder(0, -1), StlHeader, GetDisplayName(node));
            public virtual Vector2 GetSizeBody(IceprintNode node) => new(128, 48);
            public virtual void OnGUI_Body(IceprintNode node, Rect rect) => StyleBox(rect, StlBackground, "╮(๑•́ ₃•̀๑)╭");
            public virtual string GetDisplayName(IceprintNode node) => GetNodeDisplayName(node.GetType().Name);
            public virtual void OnSelect(IceprintNode node) { }
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
        public sealed override void OnSelect(IceprintNode node) => OnSelect(node as Node);
        public virtual void OnSelect(Node node) { }
    }
}