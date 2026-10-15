# Project instructions

The user's C# formatting and member-ordering rules, including the Unity lifecycle callback exception, are maintained in the global instructions at `C:\Users\sirpr\.codex\AGENTS.md`. Read and follow those rules when working on this project.

Commit each completed and verified task separately, as authorized in the global instructions. Keep temporary captures and installation backups out of project commits.

## Architecture audit follow-up

Before changing the race architecture, read [the architecture audit, completed checklist, current responsibility boundaries, and regression results](docs/architecture/audit-2026-10-15.md). The original audit is a historical record; its findings were fixed and verified on 15 October 2026, ahead of the planned date. Preserve the responsibility boundaries, especially for `FinishLine`, `Mover`, and `Racer`, and run the applicable permanent checks in `Tests/Architecture` after architecture changes.
