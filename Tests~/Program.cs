using Ji2.Cheats;

int passed = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; }
void Throws<T>(Action action, string name) where T : Exception
{ try { action(); } catch (T) { passed++; return; } throw new Exception(name); }
var registry = new CheatRegistry();
var fallback = new Provider("game", false);
using var fallbackLease = registry.Register(fallback, fallback: true);
Check(registry.Find("game") == fallback, "Editor fallback is available before Play Mode");
var live = new Provider("game", true);
var liveLease = registry.Register(live);
Check(registry.Providers.Count == 1 && registry.Find("game") == live, "Live provider supersedes fallback without duplicate rows");
Throws<InvalidOperationException>(() => registry.Register(new Provider("game", true)), "Duplicate live context must fail");
Throws<ArgumentNullException>(() => registry.Register(null), "Null provider must fail");
Throws<ArgumentException>(() => new CheatDefinition("", "Title", ""), "Stable command ID required");
Throws<ArgumentException>(() => registry.Register(new Provider("bad", true) { Duplicate = true }), "Duplicate command IDs rejected");
Check(registry.Execute("missing", "grant").Success == false, "Unknown provider never executes");
Check(registry.Execute("game", "missing").Success == false && live.Executions == 0, "Unknown command never executes");
live.Enabled = false;
Check(!registry.Execute("game", "grant").Success && live.Executions == 0, "Disabled command never executes");
live.Enabled = true;
int changes = 0; registry.Changed += () => changes++;
Check(registry.Execute("game", "grant").Success && live.Executions == 1 && changes == 1, "Successful dispatch updates observers once");
live.ThrowOnExecute = true;
Check(!registry.Execute("game", "grant").Success, "Provider execution error reaches caller");
live.ThrowOnState = true;
Check(!registry.GetState("game", "grant").Enabled, "State query error disables command");
liveLease.Dispose();
Check(registry.Find("game") == fallback, "Scene teardown restores Editor fallback");
var replacement = new Provider("game", true);
using var replacementLease = registry.Register(replacement);
liveLease.Dispose();
Check(registry.Find("game") == replacement, "Old lease cannot unregister a replacement context");
replacementLease.Dispose(); fallbackLease.Dispose();
Check(registry.Providers.Count == 0, "All providers released cleanly");
var target = new LevelTarget();
var levels = new LevelCheats("levels", "Levels", target);
Throws<ArgumentException>(() => new LevelCheats("", "x", target), "Level provider requires an ID");
Throws<ArgumentNullException>(() => new LevelCheats("levels", "x", null), "Level provider requires a target");
using (var levelLease = registry.Register(levels))
{
    Check(!registry.Execute("levels", LevelCheats.Win).Success && target.Wins == 0, "Win needs a level in progress");
    target.Playing = true;
    Check(registry.Execute("levels", LevelCheats.Win).Success && target.Wins == 1, "Win reaches the game");
    Check(registry.Execute("levels", LevelCheats.Lose).Success && target.Losses == 1, "Lose reaches the game");
    Check(registry.Execute("levels", LevelCheats.Restart).Success && target.Restarts == 1, "Restart reaches the game");
    Check(!registry.Execute("levels", LevelCheats.Next).Success && target.Index == 0, "Navigation waits for CanChangeLevel");
    target.CanChange = true;
    Check(registry.Execute("levels", LevelCheats.Previous).Success && target.Index == 2, "Previous wraps to the last level");
    Check(registry.Execute("levels", LevelCheats.Next).Success && target.Index == 0, "Next wraps to the first level");
    target.Count = 1;
    Check(!registry.GetState("levels", LevelCheats.Next).Enabled, "A single level cannot switch");
}
Console.WriteLine($"PASS: {passed} reusable cheat registry and level command checks; no Unity loaded.");

sealed class Provider : ICheatProvider
{
    public Provider(string id, bool enabled) { Id = id; Enabled = enabled; }
    public string Id { get; }
    public string DisplayName => Id;
    public string Context => "test";
    public bool Enabled, Duplicate, ThrowOnState, ThrowOnExecute;
    public int Executions;
    public IReadOnlyList<CheatDefinition> Commands => Duplicate ? new[] { Command, Command } : new[] { Command };
    private static readonly CheatDefinition Command = new("grant", "Grant", "Grant test inventory", "Inventory");
    public CheatState GetState(string id) => ThrowOnState ? throw new InvalidOperationException("state failed") : Enabled ? CheatState.Ready() : CheatState.Disabled("not ready");
    public CheatResult Execute(string id)
    { if (ThrowOnExecute) throw new InvalidOperationException("execution failed"); Executions++; return CheatResult.Completed("done"); }
}

sealed class LevelTarget : ILevelCheatTarget
{
    public int Index, Count = 3, Wins, Losses, Restarts;
    public bool Playing, CanChange;
    public int LevelIndex => Index;
    public int LevelCount => Count;
    public bool LevelInProgress => Playing;
    public bool CanChangeLevel => CanChange;
    public void WinLevel() => Wins++;
    public void LoseLevel() => Losses++;
    public void RestartLevel() => Restarts++;
    public void OpenLevel(int index) => Index = index;
}
