# Architecture regression tests

These permanent integration tests run against the active Unity Main scene in Play Mode. They are outside `Assets` so the runtime build does not include test code. Each test restores modified runtime settings and event subscriptions before returning.

Use the Unity CLI `run_script` command, or the connected `run_script` tool, with the repository-relative source path and its static entry point. The tool compiles the test in memory against the loaded project assemblies; it does not require adding components or editing the scene.

| Source | Entry point | Coverage |
|---|---|---|
| `Tests/Architecture/RaceLifecycleTests.cs` | `RaceLifecycleTests.Main` | Disable during acceleration/pause, explicit preparation while disabled, repeat after result, complete navigation reset. |

A successful test returns a `PASS` result. Assertion failures throw with a specific failure message. Run the scene normally before invoking a test; do not use these tests against an unsaved scene with unrelated gameplay objects.
