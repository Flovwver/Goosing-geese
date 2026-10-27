# Project instructions

The user's C# formatting and member-ordering rules, including the Unity lifecycle callback exception, are maintained in the global instructions at `C:\Users\sirpr\.codex\AGENTS.md`. Read and follow those rules when working on this project.

Commit each completed and verified task separately, as authorized in the global instructions. Keep temporary captures and installation backups out of project commits.

# Unity builds

Do not create player builds unless the user explicitly requests a build. Validate changes in the Unity Editor by default. This rule also applies to smoke-test and development builds.

## Architecture audit follow-up

Before changing race architecture, read [the historical audit and responsibility boundaries](Docs/architecture/audit-2026-10-15.md) and [the branch integration decisions](Docs/architecture/merge-2026-10-27.md). Preserve the boundaries: views observe models; FinishLine only detects crossings; Level owns preparation and spawn poses; RaceController owns the player simulation and results; Mover executes movement; grip is separate from overheating. Run applicable permanent checks from Tests/Architecture. The current road coloring is yellow straights and green bends; the older speed-profile rendering is historical.

