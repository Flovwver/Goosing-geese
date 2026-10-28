# Follow-up to the strict architecture audit, 29 October 2026

This is an implementation progress record. The complete audit is NOT closed.

## Implemented and checked

- Disabled Racer stops movement and rejects simulation commands; OpponentDriver stops its racer.
- OpponentSpawner and WorldPresentation synchronize an existing path on enable and destroy owned objects.
- Zero off-road grace cannot lose while on the road; speedometer tolerates missing/short label arrays.
- Click volume is applied once.
- ProfileCommands handles settings/scenery/quest mutations outside views; volume persistence is deferred to screen transitions.
- Existing garage/record captions refresh after language changes; explicit boolean negation and selected names were corrected.
- GameSession owns campaign selection, rewards and persistence; Level no longer references RaceController. Existing scene/menu APIs remain adapters.
- Rewards/counters saturate instead of overflowing; route collections have cached read-only wrappers; campaign records are copied before returning.
- Level prepares opponent spawn poses; OpponentDriver chooses throttle only. OpponentSpawner retains the stage copy for retries of the current road.
- Mesh mirroring clones channels and submeshes, updates normals/tangents and checks destination availability.

Unity CLI beta.14, Unity 6000.3.6f1: editor compilation succeeded. All nine current architecture suites passed together with live profile and profile/leaderboard PlayerPrefs restored in finally. This included ten attempts and 100 seeds. No player build was created. Mesh mirroring compiled but still needs dedicated geometry regression coverage.

## Outstanding work from the audit

- Separate profile save DTO from its encapsulated runtime model; consolidate migrations and ownership normalization.
- Replace static persistence/domain entry points with explicit instance dependencies; stable leaderboard identity.
- Complete session composition and explicit scene dependency validation; replace broad scene searches.
- Capture the full opponent definition in the level snapshot, including behaviour/navigation and reload recovery.
- Define the transaction boundary for preparation and isolate observer failures after a successful road replacement.
- Split WorldPresentation responsibilities (scenery, racer appearance, compatible materials).
- Consolidate localization, simplify nested presentation expressions, cache HUD dependencies and update infrequent values on events.
- Give quests named identifiers and shared definitions; remove remaining style inconsistencies, including tests.
- Decide/remove historical unused speed-profile settings, APIs and components with asset-reference verification.
- Organize namespaces/folders without breaking scene GUIDs; integrate automated style checks.
- Move editor tooling into an editor assembly and test mesh-channel preservation.
- Commit a single reproducible architecture-suite runner with automatic profile restoration, add missing lifecycle/locale/overflow/campaign tests and rerun final checks.

Do not describe this audit as fully fixed until the outstanding list is resolved and verified.
