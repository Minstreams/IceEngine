using UnityEngine;

namespace IceEngine.Internal
{
    /// <summary>
    /// 全局系统配置
    /// </summary>
    [IceSettingPath("IceEngine/IceRuntime")]
    public class SettingGlobal : Framework.IceSetting<SettingGlobal>
    {
        #region ThemeColor
        public override Color DefaultThemeColor => new(0.5727127f, 0.7295899f, 0.735849f);
        #endregion
    }
}
