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
            public virtual Vector2 GetSizeTitle(IceprintNodeComponent node) => new(128, 16);
            public virtual Vector2 GetSizeBody(IceprintNodeComponent node) => new(224, 32);
            public virtual void OnGUI_Title(IceprintNodeComponent node, Rect rect)
            {
                StyleBox(rect, StlLabel, GetDisplayName(node).Color(IceGUIUtility.CurrentThemeColor).Bold());
            }
            public virtual void OnGUI_Body(IceprintNodeComponent node, Rect rect)
            {
                using var _ = Area(rect);

                Space(8);
                Label(node.GetPath());
            }
            public virtual string GetDisplayName(IceprintNodeComponent node) => node.GetType().Name;
            #endregion
        }
    }

    public abstract class IceprintNodeComponentDrawer<NodeComp> : Internal.IceprintNodeComponentDrawer where NodeComp : IceprintNodeComponent
    {
        internal sealed override Type NodeType => typeof(NodeComp);
        public sealed override Vector2 GetSizeTitle(IceprintNodeComponent node) => GetSizeTitle(node as NodeComp);
        public virtual Vector2 GetSizeTitle(NodeComp node) => base.GetSizeTitle(node);
        public sealed override void OnGUI_Title(IceprintNodeComponent node, Rect rect) => OnGUI_Title(node as NodeComp, rect);
        public virtual void OnGUI_Title(NodeComp node, Rect rect) => base.OnGUI_Title(node, rect);
        public sealed override Vector2 GetSizeBody(IceprintNodeComponent node) => GetSizeBody(node as NodeComp);
        public virtual Vector2 GetSizeBody(NodeComp node) => base.GetSizeBody(node);
        public sealed override void OnGUI_Body(IceprintNodeComponent node, Rect rect) => OnGUI_Body(node as NodeComp, rect);
        public virtual void OnGUI_Body(NodeComp node, Rect rect) => base.OnGUI_Body(node, rect);
        public sealed override string GetDisplayName(IceprintNodeComponent node) => GetDisplayName(node as NodeComp);
        public virtual string GetDisplayName(NodeComp node) => typeof(NodeComp).Name;
    }
}
