# JI2 Cheats

Reusable UPM package `com.ji2.cheats`, maintained independently from its consuming games. The runtime assembly has no Unity engine dependency. The Editor assembly provides a searchable, scrollable window at `Tools > Development > Cheats`.

Games implement `ICheatProvider`: stable command IDs, authored display copy, categories, context, current availability and command execution. The same provider can feed a game's development-only cheat panel and the Editor window. The package contains no game commands, gameplay assets, currency, scene navigation or input recorder implementation.

Register a provider through `CheatRegistry.Shared.Register(provider)` and dispose its lease with its owning scene/scope. An Editor adapter can register a fallback with the same provider ID using `fallback: true`. A live provider temporarily supersedes that fallback; disposing the live lease restores it. Unknown or unavailable commands never execute. Command failures are returned to the caller and shown in the window.

## Integration

Reference `Ji2.Cheats` from the game's runtime assembly and `Ji2.Cheats.Editor` from its Editor adapter. Add a project menu alias if useful. Keep release-player feature gates in the game adapter and hide its runtime cheat entrance outside Editor/development builds. The generic registry is not an authorization boundary.

Use an exact release tag/commit for UPM Git installation or an exact Git submodule commit. Never track a floating development branch in a shipping game. PushColorMerge uses `Packages/com.ji2.cheats` as a pinned submodule; its authored CheatCatalog and CheatService remain game-owned.

## Validation

```powershell
dotnet run --project Tests~/Cheats.Tests.csproj
```

These checks cover dispatch guards, duplicate registration, Editor fallback/live handoff, error reporting and teardown ownership. Unity compilation, native Editor-window inspection and consuming-game integration tests must also pass before release.

Provenance: independently authored as a reusable command/Editor abstraction during the PushColorMerge integration on 27 October 2026. The prior game-local cheat implementation remains in PushColorMerge; no publisher or other project's gameplay/assets are distributed here. See `MAINTENANCE.md` for release and consumer updates.
