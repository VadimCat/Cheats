using System;
using System.Collections.Generic;

namespace Ji2.Cheats
{
    public sealed class CheatDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Category { get; }
        public CheatDefinition(string id, string title, string description, string category = "General")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A stable command ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("A command title is required.", nameof(title));
            Id = id; Title = title; Description = description ?? string.Empty; Category = category ?? "General";
        }
    }

    public readonly struct CheatState
    {
        public bool Enabled { get; }
        public string ActionLabel { get; }
        public string DisabledReason { get; }
        private CheatState(bool enabled, string label, string reason)
        { Enabled = enabled; ActionLabel = label; DisabledReason = reason; }
        public static CheatState Ready(string label = "RUN") => new CheatState(true, label, string.Empty);
        public static CheatState Disabled(string reason, string label = "UNAVAILABLE") => new CheatState(false, label, reason);
    }

    public readonly struct CheatResult
    {
        public bool Success { get; }
        public string Message { get; }
        private CheatResult(bool success, string message) { Success = success; Message = message; }
        public static CheatResult Completed(string message) => new CheatResult(true, message);
        public static CheatResult Rejected(string message) => new CheatResult(false, message);
    }

    /// <summary>Games own commands, context, availability and mutations. The package owns presentation and dispatch.</summary>
    public interface ICheatProvider
    {
        string Id { get; }
        string DisplayName { get; }
        string Context { get; }
        IReadOnlyList<CheatDefinition> Commands { get; }
        CheatState GetState(string commandId);
        CheatResult Execute(string commandId);
    }
}
