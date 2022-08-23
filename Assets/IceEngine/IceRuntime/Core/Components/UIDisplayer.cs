using System;
using UnityEngine;

namespace IceEngine.DebugUI
{
    /// <summary>
    /// A displayer to manage and show debug uis based on UIBase
    /// </summary>
    [AddComponentMenu("|UI/UIDisplayer")]
    public class UIDisplayer : MonoBehaviour
    {
        public UIPosition position;
        public bool horizontal;

        public event Action UIAction;

        void Awake()
        {
            var parents = GetComponentsInParent<UIDisplayer>();
            if (parents.Length > 1)
            {
                if (horizontal) parents[1].UIAction += CoreGUI_Horizontal;
                else parents[1].UIAction += CoreGUI_Vertical;
                enabled = false;
            }
        }

        const int padding = 4;

        [System.Diagnostics.Conditional("DEBUG")]
        void OnGUI()
        {
            bool isLeft = position == UIPosition.UpperLeft || position == UIPosition.LowerLeft;
            bool isUp = position == UIPosition.UpperLeft || position == UIPosition.UpperRight;
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            if (!isLeft) GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Height(Screen.height - padding));
            if (!isUp) GUILayout.FlexibleSpace();
            GUILayout.Space(padding);
            CoreGUI();
            if (isUp) GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            if (isLeft) GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        void CoreGUI()
        {
            if (horizontal) GUILayout.BeginHorizontal();
            else GUILayout.BeginVertical();
            UIAction?.Invoke();
            if (horizontal) GUILayout.EndHorizontal();
            else GUILayout.EndVertical();
        }
        void CoreGUI_Horizontal()
        {
            bool isLeft = position == UIPosition.UpperLeft || position == UIPosition.LowerLeft;
            GUILayout.BeginHorizontal();
            if (!isLeft) GUILayout.FlexibleSpace();
            UIAction?.Invoke();
            if (isLeft) GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        void CoreGUI_Vertical()
        {
            bool isUp = position == UIPosition.UpperLeft || position == UIPosition.UpperRight;
            GUILayout.BeginVertical();
            if (!isUp) GUILayout.FlexibleSpace();
            UIAction?.Invoke();
            if (isUp) GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
    }
    public enum UIPosition
    {
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight,
    }
}
