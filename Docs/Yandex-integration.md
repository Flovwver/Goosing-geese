# Yandex Games integration

The adapter registers automatically when PluginYG and its Yandex platform are enabled. It reports readiness once the main level is prepared, and starts or stops gameplay reporting as the race state changes. PluginYG's automatic Game Ready call is disabled so it cannot report before the level is ready.

- To disable the adapter and use the local fallback, define `GOOSING_GEESE_DISABLE_YANDEX` for the active build target. PluginYG remains installed.
- To remove the SDK and its adapter together, move `Assets/PluginYourGames` and its folder metadata outside `Assets` while Unity is closed. For a standalone Web build, select the default template and remove the unused Yandex template and SDK scripting defines in Player Settings.

The shared game, save data and local leaderboard do not require PluginYG.

Authorization, InterstitialAdv, Leaderboards, Localization and RewardedAdv are installed from the [official PluginYG source](https://github.com/JustPlay-Max/Unity-PluginYG-2/tree/0928f7c) for core version 2.0092. This provides the module code and build scripts when the package server is unreachable. The upstream source does not ship Unity metadata for its sample scenes and prefabs, so those samples are excluded. The game continues to use its existing SDK adapter.

The adapter uses the PluginYG Localization module to detect the platform language and handle explicit language changes. It uses Leaderboards to read the online table and submit a higher cumulative score for the account identified by Authorization. Local records remain available while the online request is pending or if it fails.

Create a descending numeric leaderboard with technical name `goosing_geese_points` in the Yandex Games Developer Console. If the game already uses another technical name, change `LeaderboardName` in `YandexGamePlatform.cs` to match it.

For authorized players the template uses the Player object from the Authorization module to load the cloud profile before Unity starts, with a three-second timeout. The game chooses the newer snapshot between the cloud and the account-specific local copy. Guest and standalone profiles keep their original local save and are separate from account profiles. If the cloud read fails, the game retains local saves and does not write to the cloud until a later successful startup.

To verify synchronization on Yandex, play with an authorized test account, make progress, and reopen the same draft on a second browser with that account. In the SDK debug panel, check for `getData` on startup and `setData` after a save.
