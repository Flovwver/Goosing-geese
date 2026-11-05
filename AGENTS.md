# Project instructions

The user's C# formatting and member-ordering rules, including the Unity lifecycle callback exception, are maintained in the global instructions at `%USERPROFILE%\.codex\AGENTS.md`. Read and follow those rules when working on this project.

Commit each completed and verified task separately, as authorized in the global instructions. Keep temporary captures and installation backups out of project commits.

## Yandex Games branch boundary

- Develop all platform-neutral adapter contracts, gameplay integration through those contracts, and stubs or local implementations on `main`. The project must compile and run with those implementations independently of the Yandex Games SDK.
- Keep the Yandex Games SDK, its configuration, build templates, and SDK-specific adapter implementations and wiring exclusively on `codex/yandex-plugin`. Keep SDK references out of shared gameplay code.
- Land shared adapter changes on `main` first, then merge `main` into `codex/yandex-plugin`. Keep Yandex-specific commits out of `main`.

# Unity builds

Do not create player builds unless the user explicitly requests a build. Validate changes in the Unity Editor by default. This rule also applies to smoke-test and development builds.

## Architecture audit follow-up

Before changing race architecture, read [the historical audit and responsibility boundaries](Docs/architecture/audit-2026-10-15.md) and [the branch integration decisions](Docs/architecture/merge-2026-10-27.md). Preserve the boundaries: views observe models; FinishLine only detects crossings; Level owns preparation and spawn poses; RaceController owns the player simulation and results; Mover executes movement; grip is separate from overheating. Run applicable permanent checks from Tests/Architecture. The current road coloring is yellow straights and green bends; the older speed-profile rendering is historical.


## Current architecture and checks

Read [the completed October 29 follow-up](Docs/architecture/audit-follow-up-2026-10-29.md) for current namespaces, contracts and verification. Earlier source paths and the speed-profile UI in the October 15 audit are historical. Level consumes ILevelSession; GameSession owns campaign/profile workflow. Preserve stable script GUIDs and save DTO field names when renaming types or model properties.

Run `Tests/Architecture/Run-ArchitectureChecks.ps1` against the open Main scene for the complete Editor-only check. It restores the user profile and leaderboard automatically. Never overwrite an existing `Temp/ArchitecturePreferences.json`; restore it using PreferencesBackup.Restore first. Use `dotnet run --project Tools/CodeStyle -- --check` for the user's C# conventions (including Unity callback ordering). No player build without an explicit request.
