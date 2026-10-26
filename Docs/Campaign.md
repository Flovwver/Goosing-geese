# Campaign and progression

Campaign.asset contains 30 fixed seeds and geometry-validated road layouts. The first three stages are easy. Later five-stage groups shuffle two easy, two medium and one hard stage deterministically; stage 30 is a hard finale. Length and baseline opponent speed rise by group with easier recovery stages between harder races. Each stage stores its own opponent speed, acceleration, target cruising/corner speed and rewards.

Rewards are 60/100/160 points for easy/medium/hard wins, plus a one-time 40-point first-clear bonus. Replay selection is separate from HighestCompletedLevel: repeats pay ordinary rewards without unlocking new levels. Losses pay nothing. Existing saves migrate their previous next-level number into selected level and completed count. Campaign selection is opened from the level button on the start screen; locked stages remain visible but disabled.

Checked first-clear/repeat/loss accounting, unlock boundaries, save reload, out-of-range requests, blocked selection while racing, final-stage behavior, and matching preset layouts/seeds after restart at levels 1, 6, 15 and 30. All 30 layouts were checked against road geometry and finish runout. Tests restored original profile and leaderboard preferences. Editor compilation and Play/Stop produced no errors; no player build was made.
