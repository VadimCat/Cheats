# Changelog

## 1.2.0 — 2026-11-15

- Add the built-in `CheatEntrance` switch and its "Invisible cheats button" command (`Ji2.Cheats` / `HideEntrance`). It is always registered first in `CheatRegistry.Shared`, so every consumer's Editor window and runtime panel show it without game code.
- `ICheatSettingsStore` persists shared settings; `Ji2.Cheats.Panel` installs `PlayerPrefsCheatStore` in Edit Mode and before the first scene loads.
- The runtime panel's DEV launcher becomes fully transparent while the entrance is hidden but still opens the panel on tap.

## 1.1.0 — 2026-11-07

- Add `LevelCheats` and `ILevelCheatTarget`: shared win, lose, restart, previous and next level commands with stable IDs.
- Add the `Ji2.Cheats.Panel` runtime panel: code-built uGUI overlay with a DEV launcher, live command states, disabled reasons, result feedback, `OpenChanged` and `CloseOnExecute`.

## 1.0.0 — 2026-10-27

- Introduce stable cheat/provider contracts and guarded dispatch.
- Add owned live/fallback registration and deterministic scene teardown.
- Add a searchable Editor window with live state, disabled reasons and execution feedback.
- Establish standalone checks and a release/consumer update workflow.
