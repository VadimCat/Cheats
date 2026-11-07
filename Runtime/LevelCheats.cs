using System;
using System.Collections.Generic;

namespace Ji2.Cheats
{
    /// <summary>What a level-based game exposes so the shared level commands can drive it.</summary>
    public interface ILevelCheatTarget
    {
        int LevelIndex { get; }
        int LevelCount { get; }
        /// <summary>A level is being played and can still be won, lost or restarted.</summary>
        bool LevelInProgress { get; }
        /// <summary>The game can switch levels now (for example, not on a loading or boot screen).</summary>
        bool CanChangeLevel { get; }
        void WinLevel();
        void LoseLevel();
        void RestartLevel();
        void OpenLevel(int index);
    }

    /// <summary>Standard level commands: win, lose, restart, previous and next level. Games supply only the target.</summary>
    public sealed class LevelCheats : ICheatProvider
    {
        public const string Win = "level.win";
        public const string Lose = "level.lose";
        public const string Restart = "level.restart";
        public const string Previous = "level.previous";
        public const string Next = "level.next";
        public const string Category = "Level";

        private static readonly CheatDefinition[] Definitions =
        {
            new CheatDefinition(Win, "Win level", "Finish the current level as won.", Category),
            new CheatDefinition(Lose, "Lose level", "Finish the current level as lost.", Category),
            new CheatDefinition(Restart, "Restart level", "Restart the current level from its start.", Category),
            new CheatDefinition(Previous, "Previous level", "Open the previous level (wraps around).", Category),
            new CheatDefinition(Next, "Next level", "Open the next level (wraps around).", Category),
        };

        private readonly ILevelCheatTarget _target;

        public LevelCheats(string id, string displayName, ILevelCheatTarget target)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A stable provider ID is required.", nameof(id));
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Context => "Level " + (_target.LevelIndex + 1) + " / " + _target.LevelCount + (_target.LevelInProgress ? " · playing" : string.Empty);
        public IReadOnlyList<CheatDefinition> Commands => Definitions;

        public CheatState GetState(string commandId)
        {
            switch (commandId)
            {
                case Win:
                case Lose:
                case Restart:
                    return _target.LevelInProgress ? CheatState.Ready() : CheatState.Disabled("No level in progress.");
                case Previous:
                case Next:
                    if (_target.LevelCount < 2) return CheatState.Disabled("Only one level.");
                    return _target.CanChangeLevel ? CheatState.Ready() : CheatState.Disabled("Levels cannot change now.");
                default:
                    return CheatState.Disabled("Unknown command.");
            }
        }

        public CheatResult Execute(string commandId)
        {
            var state = GetState(commandId);
            if (!state.Enabled) return CheatResult.Rejected(state.DisabledReason);
            int count = _target.LevelCount;
            switch (commandId)
            {
                case Win: _target.WinLevel(); return CheatResult.Completed("Level " + (_target.LevelIndex + 1) + " won.");
                case Lose: _target.LoseLevel(); return CheatResult.Completed("Level " + (_target.LevelIndex + 1) + " lost.");
                case Restart: _target.RestartLevel(); return CheatResult.Completed("Level " + (_target.LevelIndex + 1) + " restarted.");
                case Previous:
                {
                    int index = (_target.LevelIndex - 1 + count) % count;
                    _target.OpenLevel(index);
                    return CheatResult.Completed("Opened level " + (index + 1) + ".");
                }
                default:
                {
                    int index = (_target.LevelIndex + 1) % count;
                    _target.OpenLevel(index);
                    return CheatResult.Completed("Opened level " + (index + 1) + ".");
                }
            }
        }
    }
}
