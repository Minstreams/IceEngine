using UnityEngine;

namespace IceEngine.Internal
{
    public class SettingLevel : Framework.IceSetting<SettingLevel>
    {
        #region ThemeColor
        public override Color DefaultThemeColor => new(1f, 0.6f, 0f);
        #endregion
    }
}