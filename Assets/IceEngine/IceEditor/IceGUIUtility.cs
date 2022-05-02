using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using IceEngine;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    public static class IceGUIUtility
    {
        #region General Implementation
        public static void DrawSerializedObject(SerializedObject so)
        {
            so.UpdateIfRequiredOrScript();
            SerializedProperty itr = so.GetIterator();
            itr.NextVisible(true);
            // m_Script 是 Monobehavior 隐藏字段，没必要显示在面板上
            do if (itr.propertyPath != "m_Script")
                {
                    // Property Field
                    EditorGUILayout.PropertyField(itr, true);
                }
            while (itr.NextVisible(false));
            so.ApplyModifiedProperties();
        }
        public static Color CurrentThemeColor => IceGUIAutoPack.CurrentPack?.ThemeColor ?? IcePreference.Config.themeColor;
        #endregion

        #region GUIStyle
        public static GUIStyle GetStyle(string key = null, Func<GUIStyle> itor = null) => IceGUIStyleBox.GetStyle(key, itor);
        internal static GUIStyle GetStlSectionHeader(Color themeColor) => new GUIStyle("AnimationEventTooltip")
        {
            padding = new RectOffset(1, 8, 2, 2),
            overflow = new RectOffset(24, 0, 0, 0),
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            imagePosition = ImagePosition.ImageLeft,
            contentOffset = new Vector2(0f, 0f),
            stretchWidth = false,
        }.Initialize(stl =>
        {
            stl.normal.textColor = new Color(0.8396226f, 0.8396226f, 0.8396226f);
            stl.hover.textColor = new Color(1, 1, 1);
            stl.hover.background = stl.normal.background;
            stl.onNormal.textColor = themeColor;
            stl.onNormal.background = stl.normal.background;
            stl.onHover.textColor = themeColor * 1.2f;
            stl.onHover.background = stl.normal.background;
        });
        internal static GUIStyle GetStlPrefix(Color themeColor) => new GUIStyle("PrefixLabel") { margin = new RectOffset(3, 3, 2, 2), padding = new RectOffset(1, 1, 0, 0), alignment = TextAnchor.MiddleLeft, richText = true, }.Initialize(stl => { stl.focused.textColor = stl.active.textColor = stl.onNormal.textColor = stl.onActive.textColor = themeColor; stl.onNormal.background = stl.active.background; });
        internal static GUIStyle GetStlSeparator(Color themeColor) => new GUIStyle($"flow node {GetThemeColorHueIndex(themeColor)}");
        internal static GUIStyle GetStlSeparatorOn(Color themeColor) => new GUIStyle($"flow node {GetThemeColorHueIndex(themeColor)} on");
        static int GetThemeColorHueIndex(Color themeColor)
        {
            Color.RGBToHSV(themeColor, out float h, out float s, out _);
            if (s < 0.3f) return 0;
            if (h < 0.06f) return 6;
            if (h < 0.13f) return 5;
            if (h < 0.19f) return 4;
            if (h < 0.46f) return 3;
            if (h < 0.52f) return 2;
            if (h < 0.84f) return 1;
            return 6;
        }
        #endregion

        #region Preference Setting
        [SettingsProvider] static SettingsProvider GetRuntimeSettingProvider() => GetSettingProvider("Preferences/IceEngine/0", "General", IcePreference.Config, IcePreference.CreateConfig);
        [SettingsProvider] static SettingsProvider GetSettingProvider() => GetSettingProvider("Preferences/IceEngine/1", "Editor", IceEditorConfig.Config, IceEditorConfig.CreateConfig);
        internal static SettingsProvider GetSettingProvider<ConfigType>(string path, string label, ConfigType config, Func<ConfigType> createConfigAction, SettingsScope scope = SettingsScope.User) where ConfigType : ScriptableObject
        {
            SerializedObject so = null;
            return new SettingsProvider(path, scope)
            {
                activateHandler = (filter, rootElement) =>
                {
                    if (config == null) config = createConfigAction?.Invoke();
                    if (config != null) so = new SerializedObject(config);
                },
                guiHandler = (filter) =>
                {
                    if (so == null)
                    {
                        LabelError("Serialized Object is not generated!");
                        return;
                    }
                    DrawSerializedObject(so);
                },
                label = label,
            };
        }
        #endregion
    }
}
