# Yandex Games integration

The adapter registers automatically when PluginYG and its Yandex platform are enabled. It reports readiness once the main level is prepared, and starts or stops gameplay reporting as the race state changes. PluginYG's automatic Game Ready call is disabled so it cannot report before the level is ready.

- To disable the adapter and use the local fallback, define `GOOSING_GEESE_DISABLE_YANDEX` for the active build target. PluginYG remains installed.
- To remove the SDK and its adapter together, move `Assets/PluginYourGames` and its folder metadata outside `Assets` while Unity is closed. For a standalone Web build, select the default template and remove the unused Yandex template and SDK scripting defines in Player Settings.

The shared game, save data and local leaderboard do not require PluginYG.
