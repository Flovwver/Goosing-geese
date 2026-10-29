# Architecture regression checks

Open `Assets/Goosing geese/Scenes/Main.unity` in Unity 6000.3.6f1. From the repository root, run:

```powershell
& ./Tests/Architecture/Run-ArchitectureChecks.ps1
```

Prerequisites: Unity CLI beta.14 or newer connected to the Editor, Python 3, and .NET SDK 10 for the repository code-style tool. No player build is created.

The runner checks C# style, generates `Temp/ArchitectureSuite.cs`, backs up profile/leaderboard PlayerPrefs **before entering Play Mode**, executes all scenarios, restores the original preferences in `finally`, and restores the previous Play Mode state. The suite additionally restores the live profile. Unexpected Unity errors fail the suite; the deliberately injected `Expected audit observer failure` is the sole expected exception log.

If the process is forcibly interrupted after saving preferences, recover them before another run:

```powershell
unity command run_script Tests/Architecture/PreferencesBackup.cs PreferencesBackup.Restore
```

Never delete an existing `Temp/ArchitecturePreferences.json` instead of restoring it. The runner refuses to overwrite that backup.

| Entry point | Coverage |
|---|---|
| AuditLifecycleTests.Main | Disabled racer command rejection, zero off-road grace, partial/missing gauge labels. |
| GripAndOverheatTests.Main | Independent grip thresholds, explicit-time heat/cooling, paused step, one explosion and reset. |
| LevelReplacementTests.Main | Invalid settings/prefabs/geometry retain the existing road and clean up candidates. |
| LevelSnapshotTests.Main | Source-independent road/player/navigation snapshots; retry and restoration. |
| MergeIntegrationTests.Main | Grouped model colors, planar finish/runout, off-road rejection, vertical lock. |
| MeshMirrorTests.Main | Submesh winding, normal/tangent reflection, UV2/colors, untouched source mesh. |
| OpponentSnapshotTests.Main | Captured opponent physics/behaviour/pose, JSON reload snapshot, missed-path recovery, disabled driver. |
| PreparationBoundaryTests.Main | Pre-commit dependency validation; reported observer failure cannot invalidate replacement or skip later listeners. |
| PresentationContractsTests.Main | Immediate language refresh, model-driven scenery updates, re-enable synchronization. |
| ProfileArchitectureTests.Main | Quest rollover, single claim, invalid IDs, legacy/corrupt save migration and leaderboard isolation. |
| ProfileContractsTests.Main | DTO ownership, saturating currency, atomic spending, stable identity, defensive upgrade queries. |
| RaceFlowTests.Main | Ten attempts (five finishes/five overheats), child colliders, one result, input/pause/camera/retry/rebuild and 100 seeds. |
| RaceLifecycleTests.Main | Controller disable/re-enable, owned pause, preparation while disabled, full racer reset. |
| ReloadRecoveryTests.Main | First-frame recovery after managed state is lost; both participants, route and missing-script check. |
| ResultAndIdentityTests.Main | Reentrant result commits once; personal best survives renaming. |
| SceneWiringTests.Main | Serialized keyboard/pedal dependencies, enabled action, press/release delivery. |
| SpeedProfileTests.Main | Editor-only advisory geometry/drag diagnostics and current road-color continuity/visibility. |

To run selected scenarios through Unity MCP:

```powershell
python Tests/Architecture/build_suite.py OpponentSnapshotTests RaceLifecycleTests
```

Then call `run_script` with file `Temp/ArchitectureSuite.cs`, entry `ArchitectureSuite.Main` in Play Mode. For full preference protection use the PowerShell runner above. Never execute asynchronous test entries without awaiting them.

The advisory speed calculation lives under `Assets/Goosing geese/Editor` and has an Editor menu entry. Runtime road colors remain yellow straights and green bends; no speed profile is calculated by Level.

See [current responsibilities and audit resolution](../../Docs/architecture/audit-follow-up-2026-10-29.md). Historical behaviour is documented in the October 15 audit and October 27 integration record.
