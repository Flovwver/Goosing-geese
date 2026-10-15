# Project instructions

The user's C# formatting and member-ordering rules, including the Unity lifecycle callback exception, are maintained in the global instructions at `C:\Users\sirpr\.codex\AGENTS.md`. Read and follow those rules when working on this project.

Commit each completed and verified task separately, as authorized in the global instructions. Keep temporary captures and installation backups out of project commits.

## Architecture audit follow-up

Before changing the race architecture or fixing the audit findings, read [the full architecture audit and pending checklist](docs/architecture/audit-2026-10-15.md). The findings are pending work scheduled in Todoist for 28 October 2026; do not treat the recorded recommendations as completed fixes. Preserve the responsibility boundaries described there, especially for `FinishLine`, `Mover`, and `Racer`.
