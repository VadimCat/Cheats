using System;
using System.Collections.Generic;

namespace Ji2.Cheats
{
    /// <summary>Key-value storage for shared cheat settings. Unity consumers get a PlayerPrefs store from Ji2.Cheats.Panel.</summary>
    public interface ICheatSettingsStore
    {
        bool GetBool(string key, bool fallback);
        void SetBool(string key, bool value);
    }

    /// <summary>
    /// Built-in visibility switch for every game's cheat entrance (its DEV/CHEATS button). While <see cref="Hidden"/>
    /// is set, entrances draw nothing but keep receiving taps, so recordings and screenshots look like the shipped
    /// game while the cheats stay one tap away. Its command is always registered first in <see cref="CheatRegistry.Shared"/>.
    /// </summary>
    public static class CheatEntrance
    {
        public const string ProviderId = "Ji2.Cheats";
        public const string HideCommandId = "HideEntrance";
        public const string StorageKey = "Ji2.Cheats.EntranceHidden";

        private sealed class MemoryStore : ICheatSettingsStore
        {
            private readonly Dictionary<string, bool> _values = new Dictionary<string, bool>(StringComparer.Ordinal);
            public bool GetBool(string key, bool fallback) => _values.TryGetValue(key, out var value) ? value : fallback;
            public void SetBool(string key, bool value) => _values[key] = value;
        }

        private static ICheatSettingsStore _store = new MemoryStore();

        /// <summary>Raised when the entrance visibility (or its store) changes. Entrance views reapply their look here.</summary>
        public static event Action Changed;

        /// <summary>Persistence for the setting; in-memory until a consumer or Ji2.Cheats.Panel installs one.</summary>
        public static ICheatSettingsStore Store
        {
            get => _store;
            set { _store = value ?? new MemoryStore(); Changed?.Invoke(); }
        }

        public static bool Hidden
        {
            get => _store.GetBool(StorageKey, false);
            set
            {
                if (Hidden == value) return;
                _store.SetBool(StorageKey, value);
                Changed?.Invoke();
            }
        }

        public static string Toggle()
        {
            Hidden = !Hidden;
            return Hidden ? "Cheats button invisible; tap its place to open the cheats" : "Cheats button visible";
        }
    }

    /// <summary>The package's own provider: present in every consumer without game code.</summary>
    internal sealed class CheatEntranceProvider : ICheatProvider
    {
        private static readonly CheatDefinition[] Definitions =
        {
            new CheatDefinition(CheatEntrance.HideCommandId, "Invisible cheats button",
                "Hide the cheats button; tapping its place still opens the cheats", "Cheats")
        };
        public string Id => CheatEntrance.ProviderId;
        public string DisplayName => "Cheats";
        public string Context => CheatEntrance.Hidden ? "Cheats button invisible" : "Cheats button visible";
        public IReadOnlyList<CheatDefinition> Commands => Definitions;
        public CheatState GetState(string commandId) => commandId == CheatEntrance.HideCommandId
            ? CheatState.Ready(CheatEntrance.Hidden ? "ON" : "OFF")
            : CheatState.Disabled("Unknown command.");
        public CheatResult Execute(string commandId) => commandId == CheatEntrance.HideCommandId
            ? CheatResult.Completed(CheatEntrance.Toggle())
            : CheatResult.Rejected("Unknown command.");
    }
}
