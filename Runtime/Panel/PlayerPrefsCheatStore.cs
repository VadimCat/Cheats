using UnityEngine;

namespace Ji2.Cheats.Panel
{
    /// <summary>PlayerPrefs persistence for shared cheat settings such as <see cref="CheatEntrance.Hidden"/>.</summary>
    public sealed class PlayerPrefsCheatStore : ICheatSettingsStore
    {
        public bool GetBool(string key, bool fallback) => PlayerPrefs.GetInt(key, fallback ? 1 : 0) != 0;
        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        // Installed in Edit Mode and before any scene loads, so the Editor window, the runtime panel and game
        // entrances read the same persisted value.
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Install()
        {
            if (!(CheatEntrance.Store is PlayerPrefsCheatStore)) CheatEntrance.Store = new PlayerPrefsCheatStore();
        }
    }
}
