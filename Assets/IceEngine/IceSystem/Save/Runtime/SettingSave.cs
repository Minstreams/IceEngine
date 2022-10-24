using UnityEngine;

namespace IceEngine.Internal
{
    public class SettingSave : Framework.IceSetting<SettingSave>
    {
        #region ThemeColor
        public override Color DefaultThemeColor => new(0.4773f, 0.7075f, 0.4953f);
        #endregion
    }
}