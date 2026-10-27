using System;
using System.Collections.Generic;
using System.Linq;

namespace Ji2.Cheats
{
    /// <summary>Main-thread registry. A scene provider temporarily supersedes its Editor fallback.</summary>
    public sealed class CheatRegistry
    {
        private sealed class Slot { public ICheatProvider Primary, Fallback; }
        private sealed class Lease : IDisposable
        {
            private CheatRegistry _owner;
            private readonly ICheatProvider _provider;
            private readonly bool _fallback;
            public Lease(CheatRegistry owner, ICheatProvider provider, bool fallback)
            { _owner = owner; _provider = provider; _fallback = fallback; }
            public void Dispose()
            { var owner = _owner; _owner = null; owner?.Remove(_provider, _fallback); }
        }
        private readonly Dictionary<string, Slot> _slots = new Dictionary<string, Slot>(StringComparer.Ordinal);
        public static CheatRegistry Shared { get; } = new CheatRegistry();
        public event Action Changed;
        public IReadOnlyList<ICheatProvider> Providers => _slots.Values.Select(s => s.Primary ?? s.Fallback)
            .OrderBy(p => p.Id, StringComparer.Ordinal).ToArray();
        public ICheatProvider Find(string id) => _slots.TryGetValue(id, out var slot) ? slot.Primary ?? slot.Fallback : null;

        public IDisposable Register(ICheatProvider provider, bool fallback = false)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (string.IsNullOrWhiteSpace(provider.Id)) throw new ArgumentException("Provider ID is required.", nameof(provider));
            if (provider.Commands == null) throw new ArgumentException("Provider commands are required.", nameof(provider));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var command in provider.Commands)
                if (command == null || !ids.Add(command.Id)) throw new ArgumentException("Null or duplicate command in " + provider.Id);
            if (!_slots.TryGetValue(provider.Id, out var slot)) _slots.Add(provider.Id, slot = new Slot());
            if ((fallback ? slot.Fallback : slot.Primary) != null)
                throw new InvalidOperationException("Provider already registered in this slot: " + provider.Id);
            if (fallback) slot.Fallback = provider; else slot.Primary = provider;
            Changed?.Invoke();
            return new Lease(this, provider, fallback);
        }

        private void Remove(ICheatProvider provider, bool fallback)
        {
            if (!_slots.TryGetValue(provider.Id, out var slot)) return;
            if (fallback && ReferenceEquals(slot.Fallback, provider)) slot.Fallback = null;
            if (!fallback && ReferenceEquals(slot.Primary, provider)) slot.Primary = null;
            if (slot.Primary == null && slot.Fallback == null) _slots.Remove(provider.Id);
            Changed?.Invoke();
        }

        public CheatState GetState(string providerId, string commandId)
        {
            var provider = Find(providerId);
            if (provider == null) return CheatState.Disabled("No active context for this provider.");
            if (!provider.Commands.Any(c => c.Id == commandId)) return CheatState.Disabled("Command is no longer registered.");
            try { return provider.GetState(commandId); }
            catch (Exception error) { return CheatState.Disabled(error.Message); }
        }

        public CheatResult Execute(string providerId, string commandId)
        {
            var state = GetState(providerId, commandId);
            if (!state.Enabled) return CheatResult.Rejected(state.DisabledReason);
            CheatResult result;
            try { result = Find(providerId).Execute(commandId); }
            catch (Exception error) { result = CheatResult.Rejected(error.Message); }
            Changed?.Invoke();
            return result;
        }
    }
}
