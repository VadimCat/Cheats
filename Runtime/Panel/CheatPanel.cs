using System;
using UnityEngine;

namespace Ji2.Cheats.Panel
{
    /// <summary>
    /// Runtime cheat panel shared by all consumers. It lists every provider in <see cref="CheatRegistry.Shared"/>,
    /// so a game adds commands only through its providers. Install it from Editor/development-only code.
    /// </summary>
    public static class CheatPanel
    {
        private static CheatPanelRoot _root;

        /// <summary>Raised with true when the panel opens and false when it closes. Games pause gameplay input here.</summary>
        public static event Action<bool> OpenChanged;

        /// <summary>Close the panel before a command runs (useful for win/lose/level switches).</summary>
        public static bool CloseOnExecute { get; set; }

        public static bool Installed => _root != null;
        public static bool IsOpen => _root != null && _root.IsOpen;

        /// <summary>Create the overlay and a small DEV launcher at a normalized screen anchor (default: left edge, upper middle).</summary>
        public static void Install(Vector2? launcherAnchor = null)
        {
            if (_root != null) return;
            var go = new GameObject("Ji2 Cheat Panel");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _root = go.AddComponent<CheatPanelRoot>();
            _root.Build(launcherAnchor ?? new Vector2(0f, 0.62f));
        }

        public static void Uninstall()
        {
            if (_root == null) return;
            if (_root.IsOpen) _root.SetOpen(false);
            UnityEngine.Object.Destroy(_root.gameObject);
            _root = null;
        }

        public static void Open() { Install(); _root.SetOpen(true); }
        public static void Close() { if (_root != null) _root.SetOpen(false); }
        public static void Toggle() { if (IsOpen) Close(); else Open(); }

        internal static void NotifyOpenChanged(bool open) => OpenChanged?.Invoke(open);
        internal static void Forget(CheatPanelRoot root) { if (_root == root) _root = null; }
    }
}
