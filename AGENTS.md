# Shared cheat package rules

- Maintain documentation in English.
- This repository is the canonical source for `com.ji2.cheats`; follow `MAINTENANCE.md` for isolated work, validation, releases and consumer updates.
- Preserve stable provider/command IDs and Unity meta GUIDs. Keep game logic and assets in consumers.
- Use the same command provider for runtime cheat UI and the generic Editor window. Never create a parallel command list in the window.
- Run standalone tests and affected Unity consumer checks before publishing. Do not create APK/AAB builds without an explicit request.
- Development tools belong in cheats or Editor windows, never player-facing settings. Consumers must gate runtime access to Editor/development builds.
