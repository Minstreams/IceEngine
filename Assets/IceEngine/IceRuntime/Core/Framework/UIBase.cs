using UnityEngine;

namespace IceEngine.DebugUI
{
    /// <summary>
    /// Base class of a debug ui. Need a UI displayer to display
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        protected void SetStyle(ref GUIStyle target, string styleName)
        {
#if UNITY_EDITOR
            target = new GUIStyle(UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Game).GetStyle(styleName));
#endif
        }
        public GUIStyle boxStyle;
        protected virtual void Reset()
        {
            SetStyle(ref boxStyle, "box");
        }

        UIDisplayer displayer;
        protected virtual void OnEnable()
        {
            displayer = GetComponentInParent<UIDisplayer>();
            if (displayer != null) displayer.UIAction += UIAction;
            else this.enabled = false;
        }
        protected virtual void OnDisable()
        {
            if (displayer != null) displayer.UIAction -= UIAction;
        }
        public void UIAction()
        {
            GUILayout.BeginVertical(boxStyle, GUILayout.ExpandWidth(false));
            OnUI();
            GUILayout.EndVertical();
        }
        protected abstract void OnUI();


        // GUI Functions
        static GUIStyle DefaultLabelStyle => "label";
        /// <summary>
        /// Calculate the width of given content if rendered with default label style. Return the "width" layout option object.
        /// </summary>
        protected static GUILayoutOption AutoWidth(string label) => GUILayout.Width(DefaultLabelStyle.CalcSize(new GUIContent(label + "\t")).x);
        /// <summary>
        /// A text field where the user can edit a string
        /// </summary>
        protected static string StringField(string label, string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, AutoWidth(label));
            string res = GUILayout.TextField(text);
            GUILayout.EndHorizontal();
            return res;
        }
        /// <summary>
        /// A text field where the user can edit a integer
        /// </summary>
        protected static int IntField(string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, AutoWidth(label));
            int res;
            try
            {
                res = int.Parse(GUILayout.TextField(value.ToString()));
                GUILayout.EndHorizontal();
                return res;
            }
            catch
            {
                GUILayout.EndHorizontal();
                return value;
            };
        }
        /// <summary>
        /// A text field where the user can edit a float number
        /// </summary>
        protected static float FloatField(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, AutoWidth(label));
            float res;
            try
            {
                res = float.Parse(GUILayout.TextField(value.ToString()));
                GUILayout.EndHorizontal();
                return res;
            }
            catch
            {
                GUILayout.EndHorizontal();
                return value;
            };
        }
        /// <summary>
        /// Make a title of certain label
        /// </summary>
        protected void TitleLabel(string label)
        {
            GUILayout.Label(label, boxStyle, GUILayout.ExpandWidth(true));
        }
    }
}
