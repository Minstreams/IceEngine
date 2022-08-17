using UnityEngine;

using IceEngine;
using IceEditor.Framework;

namespace IceEditor.Internal
{
    public class EditorSettingIceprintBox : IceEditorSetting<EditorSettingIceprintBox>
    {
        public float gridSize = 32;
        public float minScale = 0.4f;
        public float maxScale = 4.0f;
        public Color themeColor;
        public Color gridColor;
        public bool mustContainOnSelect;
        public int maxUndoCount = 512;
        public bool allowRuntimeConst = true;
    }
}
