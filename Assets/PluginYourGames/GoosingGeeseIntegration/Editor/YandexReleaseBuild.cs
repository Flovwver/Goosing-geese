using System;
using System.Linq;
using GoosingGeese.Levels;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YG;

namespace GoosingGeese.Platforms.Editor
{
    public static class YandexReleaseBuild
    {
        private const string MainScene = "Assets/Goosing geese/Scenes/Main.unity";
        private const string YandexTemplate = "PROJECT:YandexGames";
        private const string AdapterType = "GoosingGeese.Platforms.YandexGamePlatform";

        private static readonly string[] RequiredDefines =
        {
            "PLUGIN_YG_2",
            "YandexGamesPlatform_yg",
            "Authorization_yg",
            "Leaderboards_yg",
            "Localization_yg"
        };

        public static void Build()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int outputArgument = Array.IndexOf(arguments, "-buildOutput");

            if (outputArgument < 0 || outputArgument + 1 >= arguments.Length)
                throw new BuildFailedException("Pass -buildOutput with the WebGL output directory.");

            ValidateProject();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MainScene },
                target = BuildTarget.WebGL,
                locationPathName = arguments[outputArgument + 1],
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Yandex WebGL build ended with {report.summary.result} ({report.summary.totalErrors} errors).");

            Debug.Log($"GOOSING_GEESE_WEBGL_OK: {report.summary.totalSize} bytes at {report.summary.outputPath}.");
        }

        private static void ValidateProject()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new BuildFailedException("Start Unity with the WebGL build target.");

            string[] defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WebGL).Split(';');
            string[] missing = RequiredDefines.Except(defines).ToArray();

            if (missing.Length > 0 || defines.Contains("GOOSING_GEESE_DISABLE_YANDEX"))
                throw new BuildFailedException($"Yandex SDK adapter is disabled. Missing WebGL defines: {string.Join(", ", missing)}.");

            if (typeof(GameSession).Assembly.GetType(AdapterType) == null)
                throw new BuildFailedException("The Yandex adapter was not compiled for the active target.");

            var settings = Resources.Load<InfoYG>(InfoYG.NAME_INFOYG_FILE);

            if (settings == null || settings.Basic.platform == null || settings.Basic.platform.nameFull != "YandexGamesPlatform")
                throw new BuildFailedException("PluginYG must have Yandex Games selected.");

            if (settings.Basic.autoGRA || PlayerSettings.WebGL.template != YandexTemplate)
                throw new BuildFailedException("Use the Yandex Games WebGL template and let the game's adapter report readiness.");
        }
    }
}
