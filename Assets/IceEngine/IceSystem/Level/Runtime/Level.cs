using UnityEngine.SceneManagement;

namespace Ice
{
    public sealed class Level : IceEngine.Framework.IceSystem<IceEngine.Internal.SettingLevel>
    {
        public static void LoadLevel(string sceneName) => SceneManager.LoadScene(sceneName);
    }
}