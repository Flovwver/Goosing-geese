# Architecture regression tests

These integration scenarios run against Main.unity in Play Mode. They stay outside Assets and are not included in player builds. Invoke their static Main entry points through the Unity CLI run_script integration or compile a temporary test assembly against the current Unity project and invoke it on the editor main thread. Await the Task<string> returned by asynchronous tests.

Run against a disposable profile or snapshot/restore both the live PlayerProgress and the profile/leaderboard PlayerPrefs keys around the suite. Race-flow scenarios deliberately finish attempts and exercise reward persistence. Each scenario restores its runtime settings/subscriptions; the outer test runner must restore saved player data.

| Source / entry point | Coverage |
|---|---|
| RaceLifecycleTests.Main | Controller disable/re-enable during acceleration and pause; preparation while disabled; complete racer preparation and navigation reset. |
| LevelReplacementTests.Main | Invalid settings/prefabs/geometry preserve the current level and remove partial candidates. |
| LevelSnapshotTests.Main | Immutable road/racer/navigation snapshots, source independence, explicit loadout configuration changes and definition restoration. Campaign owns the stage piece count. |
| GripAndOverheatTests.Main | Independent grip thresholds, explicit-time heating/cooling/pause, one explosion and reset. |
| SpeedProfileTests.Main | Advisory curvature/steering/braking calculations; current road section coverage, seamless color transitions, retained rendering on retry and path visibility. |
| RaceFlowTests.Main | Ten attempts, finish/overheat, child colliders, one result, pause/camera/input, retry/rebuild and 100 deterministic seeds. Uses current menu actions through their model APIs. |
| MergeIntegrationTests.Main | Grouped goose colors survive animation; finish plus runout in one step at ground level and height 100; off-road rejection, one result, vertical lock. |

The braking-aware speed profile remains advisory and uses the existing movement model. The current road view follows the user's later visual rule: yellow straights and green turns with blends. The historical conditional speed-profile rendering and linear speedometer tests have been replaced with assertions for this current design.

See [integration decisions and actual verification](../../Docs/architecture/merge-2026-10-27.md). Do not create a player build without a separate user request.
