# Campaign balance verification

The first 30 stages have fixed seeds and geometry-validated layouts in Campaign.asset. Difficulty quotas are shuffled within five-stage groups after the three easy introduction races. Stage 30 is a hard milestone; procedural stages continue afterward.

## Rewards and economy

Starting repeat rewards are 60 / 100 / 160 for easy / medium / hard. Each group raises them by 25% of those base amounts. First-clear bonus is 50 points, paid once. The 30 first clears pay 5865 points in total, excluding quests. Starting upgrade prices rise by level and model; model purchase prices are 800 / 1200 / 1800.

A checked purchase route, without quest rewards:
- Before 6: goose top speed 1 and handling 1, cost 200.
- Before 9: buy car, cost 800.
- Before 12: car acceleration 1, top speed 2, handling 2, heat resistance 1, total additional cost 880. Two repeats of an available hard stage cover the shortage.
- Before 19: car top speed 3 and handling 3, additional cost 660.
- Before 25: buy goat and reach top speed 2, handling 2, heat resistance 1, total cost 2675. Three available hard-stage repeats cover the shortage.
- Before 29: goat top speed 3 and handling 3, additional cost 750.

This route spends 5965 and finishes with 1140 available points after those five repeats. Other upgrade choices and claimed quests change the required repeat count.

## Driving checks

Ran the actual Rigidbody movement at 0.02-second steps, including finish triggers and off-road checks. The test pedal policy anticipates turns by 80 metres, targets 95% of the green boundary in corners and 96% of the red-start speed on straights. This is an automated policy, not a guarantee for every human input pattern.

All 30 stages passed with the purchase route above. Player maximum distance from the path was about 2.67 m; opponent about 5.00 m, including its 4 m lane offset. The opponent did not explode. Tractor checks also passed stage 15 and remained on the road while losing stage 30; key intro/upgrade/final cases were rechecked with the previous 0.5 coasting damping without modifying the user asset. Full results and exact tested upgrade levels are in Campaign-balance.csv.

Negative controls: the unmodified goose lost stage 6; the fully upgraded goose lost stage 30; the fully upgraded goat won stage 30 without leaving the road. Faster model turn rates and opponent speed-scaled turning/grip were corrected after an initial run exposed corner cutting at high speeds.

Checked stage 31 unlock after clearing 30, unchanged procedural route on retry, difficulty quotas beyond 30, no duplicate win payout, and locked-level rejection. Test profile and leaderboard preferences were restored. The real save retained level 27, 3025 total points and 325 wallet points, with its selected model and upgrades preserved.

No player build was created. Editor compilation, Play/Stop and final console checks reported no errors.
