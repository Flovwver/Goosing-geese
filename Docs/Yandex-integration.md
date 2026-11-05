# Yandex Games integration

The adapter registers automatically when PluginYG and its Yandex platform are enabled. It reports readiness once the main level is prepared, and starts or stops gameplay reporting as the race state changes. PluginYG's automatic Game Ready call is disabled so it cannot report before the level is ready.

- To disable the adapter and use the local fallback, define `GOOSING_GEESE_DISABLE_YANDEX` for the active build target. PluginYG remains installed.
- To remove the SDK and its adapter together, move `Assets/PluginYourGames` and its folder metadata outside `Assets` while Unity is closed. For a standalone Web build, select the default template and remove the unused Yandex template and SDK scripting defines in Player Settings.

The shared game, save data and local leaderboard do not require PluginYG.

The WebGL bridge uses the Yandex SDK instance initialized by the PluginYG template. It reads the SDK language and sends cumulative points to the online leaderboard for authorized players. Existing local records remain available while the online request is pending or if it fails.

Create a descending numeric leaderboard with technical name `goosing_geese_points` in the Yandex Games Developer Console. If the game already uses another technical name, change `LeaderboardName` in `YandexGamePlatform.cs` to match it.
