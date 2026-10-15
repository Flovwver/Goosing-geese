# Architecture regression tests

These permanent integration tests run against the active Unity Main scene in Play Mode. They are outside `Assets` so the runtime build does not include test code. Each test restores modified runtime settings and event subscriptions before returning.

Use the Unity CLI `run_script` command, or the connected `run_script` tool, with the repository-relative source path and its static entry point. The tool compiles the test in memory against the loaded project assemblies; it does not require adding components or editing the scene.

| Source | Entry point | Coverage |
|---|---|---|
| `Tests/Architecture/RaceLifecycleTests.cs` | `RaceLifecycleTests.Main` | Disable during acceleration/pause, explicit preparation while disabled, repeat after result, complete navigation reset. |
| `Tests/Architecture/LevelReplacementTests.cs` | `LevelReplacementTests.Main` | Invalid settings/prefabs and failed candidate geometry preserve the current level and clean up partial objects. |
| `Tests/Architecture/LevelSnapshotTests.cs` | `LevelSnapshotTests.Main` | Immutable road/racer/navigation snapshots, independence from source edits, and restoration of a saved level definition. |
| `Tests/Architecture/GripAndOverheatTests.cs` | `GripAndOverheatTests.Main` | Independent grip thresholds and deterministic heating, cooling, pause and explosion behavior. |
| `Tests/Architecture/SpeedProfileTests.cs` | `SpeedProfileTests.Main` | Curvature/steering/braking recommendations, full route rendering, shared colors and visibility options. |
| `Tests/Architecture/RaceFlowTests.cs` | `RaceFlowTests.Main` | Ten attempts, finish/overheat, child collider detection, results, camera, pause, repeat/regeneration and 100 deterministic seeds. |

The speed profile is advisory. It derives angular limits from the existing body-turn and drift-alignment parameters, caps recommendations below the overheat zone, and propagates braking limits backwards using the coasting drag model. It does not introduce a new tire/road force simulation or guarantee recovery after the player has already exceeded the recommendation.

A successful test returns a `PASS` result. Assertion failures throw with a specific failure message. Run the scene normally before invoking a test; do not use these tests against an unsaved scene with unrelated gameplay objects.
