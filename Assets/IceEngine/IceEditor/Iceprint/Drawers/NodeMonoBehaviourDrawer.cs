using System;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Framework;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;
using System.Reflection;

namespace IceEditor.Internal
{
    public class NodeMonoBehaviourDrawer : Framework.IceprintNodeDrawer<NodeMonoBehaviour>
    {
        public override string GetDisplayName(NodeMonoBehaviour node)
        {
            if (node.target.Value != null) return node.target.Value.GetDisplayName();
            if (node.targetType != null) return $"{"Missing".Color(Color.red)} ({node.targetType.Name})";
            return "空组件";
        }
        public override Vector2 GetSizeTitle(NodeMonoBehaviour node)
        {
            if (node.target.Value != null) return node.target.Value.GetSizeTitle();
            return base.GetSizeTitle(node);
        }
        public override Vector2 GetSizeBody(NodeMonoBehaviour node)
        {
            if (node.target.Value != null) return node.target.Value.GetSizeBody();
            return new(192, 32);
        }
        public override void OnGUI_Title(NodeMonoBehaviour node, Rect rect)
        {
            var target = node.target.Value;
            if (target != null)
            {
                target.GetDrawer().OnGUI_Title(target, rect);
                return;
            }

            base.OnGUI_Title(node, rect);
        }

        public override void OnGUI_Body(NodeMonoBehaviour node, Rect rect)
        {
            var target = node.target.Value;
            if (target != null)
            {
                target.GetDrawer().OnGUI_Body(target, rect);
                return;
            }

            using (Area(rect)) using (GUICHECK)
            {
                MonoBehaviour val;
                Space(6);
                if (node.targetType == null)
                {
                    val = _ObjectField(target, true);
                }
                else
                {
                    val = (MonoBehaviour)EditorGUILayout.ObjectField(target, node.targetType, true);
                }

                if (GUIChanged && val != target)
                {
                    node.target.Value = val;
                    if (val != null)
                    {
                        Type t = val.GetType();

                        if (t != node.targetType || EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            node.targetType = t;
                            foreach (var ip in node.inports) ip.DisconnectAll();
                            foreach (var op in node.outports) op.DisconnectAll();
                            node.inports.Clear();
                            node.outports.Clear();
                            node.connectionData.Clear();
                            node.InitializePorts();
                        }
                    }
                }
            }
        }

        public override void OnSingleSelect(NodeMonoBehaviour node)
        {
            var target = node.target.Value;
            if (target != null)
            {
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }
        }
    }
}
