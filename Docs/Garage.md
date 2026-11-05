# Garage and model progression

The garage separates browsing from equipping. Locked models can be previewed, but cannot be equipped or upgraded. A purchase spends Wallet and equips the model; Points remains cumulative for the leaderboard. Goose colors share ownership and upgrades.

| Model | Price | Required completed level | Base speed | Base acceleration | Green cap |
|---|---:|---:|---:|---:|---:|
| Goose | Free | 0 | 100 | 100 | 80% |
| Car | 800 | 8 | 120 | 110 | 87% |
| Tractor | 1200 | 12 | 130 | 145 | 72% |
| Goat | 1800 | 18 | 145 | 125 | 92% |

Each model references its own base stats (goose retains PlayerRacerStats) and progression limits in RacerAppearanceCatalog. Turn rates on the faster models were tuned to keep their upgraded green-zone speeds compatible with the road curves.

## Upgrades

There are five levels each of acceleration, top speed, cooling, handling and heat resistance. Base prices are 100, 200, 300, 400 and 500, multiplied by 1 / 1.1 / 1.15 / 1.25 for goose / car / tractor / goat and rounded to whole points.

The required completed stages are 2, 5, 8, 11 and 14, or the model unlock stage if higher. A repeat victory does not bypass the sequential campaign gate.

Acceleration adds 10% of base per level; top speed 5%; cooling recovery 20%. Handling adds seven percentage points to the green boundary, capped by both the model's green limit and the current start of the red zone. Heat resistance advances the red start by four percentage points per level, subject to the model's limit. It releases room for further handling upgrades. Yellow may disappear when boundaries coincide; the speedometer merges the numbered tick.

RacerGarage owns purchase/selection/upgrade rules and profile changes. PreviewChanged drives WorldPresentation and GarageView. SelectionChanged drives RacerLoadout, which applies selected stats to a runtime copy. The models have no dependencies on views.

## Save migration

Existing campaign progress, point balances and upgrade levels are retained. The starting goose, the previously selected model and models with saved upgrades are granted ownership. New profiles start with only goose ownership. Unmodified source stats are retained; purchases never reduce cumulative leaderboard points.

## Verification

Checked ownership/stage/funds locks, preview without equipping, purchases charged once, model-specific costs and stats, shared goose colors, independent model upgrades, heat resistance releasing handling, personal caps, save/reload, rejected race-time actions, and unchanged base assets during runtime changes. The header and all five upgrade rows/back button fit the portrait layout. See Campaign-balance.md and Campaign-balance.csv for driving and economy checks.
