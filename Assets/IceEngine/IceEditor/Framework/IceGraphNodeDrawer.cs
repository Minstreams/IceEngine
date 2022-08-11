﻿using System;
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
            #region Configuration
            internal virtual Type NodeType => null;
            public virtual GUIStyle StlGraphNodeBackground => _stlGraphNodeBackground?.Check() ?? (_stlGraphNodeBackground = new GUIStyle("flow node 0") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlGraphNodeBackground;
            public virtual GUIStyle StlGraphNodeBackgroundSelected => _stlGraphNodeBackgroundSelected?.Check() ?? (_stlGraphNodeBackgroundSelected = new GUIStyle("flow node 0 on") { margin = new RectOffset(6, 6, 4, 4), padding = new RectOffset(10, 10, 6, 6), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlGraphNodeBackgroundSelected;
            public virtual Vector2 GetSizeBody(IceGraphNode node) => new(128, 64);
            public virtual Vector2 GetSizeTitle(IceGraphNode node) => ((GUIStyle)"label").CalcSize(new GUIContent("测试标题"));
            public virtual void OnGUI_Title(IceGraphNode node, Rect rect)
            {
                using (AreaRaw(rect)) Label("空节点");
            }
            public virtual void OnGUI_Body(IceGraphNode node, Rect rect)
            {
                using (AreaRaw(rect)) Label("……");
            }
            #endregion
        }
    }
    public abstract class IceGraphNodeDrawer<Node> : Internal.IceGraphNodeDrawer where Node : IceGraphNode
    {
        internal sealed override Type NodeType => typeof(Node);
    }
}