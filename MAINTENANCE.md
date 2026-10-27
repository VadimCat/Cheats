# Maintenance and consumer updates

Canonical repository: `https://github.com/VadimCat/Cheats`. Local UnityGameModules storage: `D:/Codex/JigglyAI/Cheats.git` (bare repository); implementation work uses isolated worktrees under `D:/Codex/JigglyAI/worktrees`.

1. Fetch the canonical main branch and create an isolated branch/worktree for each independent module change.
2. Implement generic registry/window behavior here. Keep game command IDs, configuration, providers, production-save boundaries and scene logic in the consuming game.
3. Run standalone registry checks and validate Unity compilation plus relevant integration checks in a task-owned consumer Editor. Inspect the actual window after layout changes.
4. Preserve existing Unity `.meta` GUIDs. Update `package.json` and `CHANGELOG.md` for every published release; version breaking contracts accordingly.
5. Review, commit and push the module, then publish an immutable `vX.Y.Z` tag. Do not move a published tag.
6. In the consumer's existing task worktree, fetch the module, check out the exact release commit, commit its submodule pointer/package lock and update project knowledge. Re-run affected checks. Do not edit the installed submodule copy as an alternative source.
7. Update the UnityGameModules catalog with the immutable source, version and verified consumer integration. Builds still require the user's explicit request.

The generic Editor window discovers the current provider definitions; adding a game cheat must not require a second hand-written button list. Regression coverage must preserve unavailable-command guards and fallback/live context teardown. The shared module and its consumer update are one reviewable delivery.
