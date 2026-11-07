# JI2 Cheats

Reusable UPM package `com.ji2.cheats`, maintained independently from its consuming games.

- `Ji2.Cheats` (no Unity engine dependency): provider contracts, the guarded registry and the standard level commands.
- `Ji2.Cheats.Panel` (uGUI): a runtime cheat panel with a small DEV launcher for Editor/development builds.
- `Ji2.Cheats.Editor`: a searchable, scrollable window at `Tools > Development > Cheats`.

Games implement `ICheatProvider`: stable command IDs, authored display copy, categories, context, current availability and command execution. Every provider registered in `CheatRegistry.Shared` appears in both the runtime panel and the Editor window, so a game never writes a second button list. The package contains no game-specific commands, gameplay assets, currency, scene navigation or input recorder implementation.

Register a provider through `CheatRegistry.Shared.Register(provider)` and dispose its lease with its owning scene/scope. An Editor adapter can register a fallback with the same provider ID using `fallback: true`. A live provider temporarily supersedes that fallback; disposing the live lease restores it. Unknown or unavailable commands never execute. Command failures are returned to the caller and shown in the panel and the window.

## Standard level commands

Level-based games do not write win/lose/navigation cheats. Implement `ILevelCheatTarget` (current index, level count, whether a level is in progress, whether levels can change now, and `WinLevel`, `LoseLevel`, `RestartLevel`, `OpenLevel(index)`) and register the shared provider:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
lease = CheatRegistry.Shared.Register(new LevelCheats("my-game", "My Game", target));
#endif
```

Commands (stable IDs): `level.win`, `level.lose`, `level.restart`, `level.previous`, `level.next` (previous/next wrap around). Win, lose and restart are available only while a level is in progress; previous/next only when `CanChangeLevel` is true. Game-specific commands go in a second provider with its own ID.

## Runtime panel

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
CheatPanel.CloseOnExecute = true;           // optional: close before a command runs
CheatPanel.OpenChanged += open => input.Enabled = !open;
CheatPanel.Install();                       // DEV launcher at the left edge; Install(new Vector2(1, 0.5f)) for another anchor
#endif
```

The panel is a code-built Screen Space Overlay canvas (sorting order 32000, 1080×1920 reference, legacy built-in font, no game assets). It lists every registered provider grouped by category, shows each command's live state and disabled reason, and reports the last result. It needs the scene's `EventSystem`. `CheatPanel.Open/Close/Toggle` let a game add its own entrance (for example a hotkey). Pause gameplay input through `OpenChanged`; with `CloseOnExecute` the panel closes (and raises `OpenChanged(false)`) before the command runs, so a game's input restore cannot override what the command changes.

## Integration

Reference `Ji2.Cheats` (and `Ji2.Cheats.Panel` for the runtime panel) from the game's runtime assembly and `Ji2.Cheats.Editor` from its Editor adapter. Keep release-player feature gates in the game adapter and hide the runtime entrance outside Editor/development builds. The generic registry is not an authorization boundary.

Use an exact release tag/commit for UPM Git installation or an exact Git submodule commit. Never track a floating development branch in a shipping game. PushColorMerge uses `Packages/com.ji2.cheats` as a pinned submodule; its authored CheatCatalog and CheatService remain game-owned. Ring Twist uses `LevelCheats` and `CheatPanel` with a small adapter.

## Validation

```powershell
dotnet run --project Tests~/Cheats.Tests.csproj
```

These checks cover dispatch guards, duplicate registration, Editor fallback/live handoff, error reporting, teardown ownership and the level command contract. Unity compilation, native window/panel inspection and consuming-game integration tests must also pass before release.

Provenance: independently authored as a reusable command/Editor abstraction during the PushColorMerge integration on 27 October 2026; the level commands and runtime panel were added on 7 November 2026 to replace per-game copies (PushColorMerge, Snowball Sweep, Magnet Vehicle, Ring Twist). No publisher or other project's gameplay/assets are distributed here. See `MAINTENANCE.md` for release and consumer updates.
