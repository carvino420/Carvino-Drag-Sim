#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Carvino.Editor
{
    public static class WindowsBuild
    {
        private const string OutputPath = "P:/chatgpt projects/Carvino Drag Sim/Builds/Windows/Carvino Drag Sim.exe";

        [MenuItem("Carvino/Build Windows Development")]
        public static void BuildDevelopment()
        {
            CarvinoBuildValidation.ValidateOrThrow();

            string directory = Path.GetDirectoryName(OutputPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Carvino/Scenes/MainMenu.unity",
                    "Assets/Carvino/Scenes/Controls.unity",
                    "Assets/Carvino/Scenes/Settings.unity",
                    "Assets/Carvino/Scenes/Career.unity",
                    "Assets/Carvino/Scenes/Profile.unity",
                    "Assets/Carvino/Scenes/Garage.unity",
                    "Assets/Carvino/Scenes/Dyno.unity",
                    "Assets/Carvino/Scenes/RaceDay.unity",
                    "Assets/Carvino/Scenes/QuarterMilePrototype.unity"
                },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Carvino Windows build failed: " + report.summary.result);
        }

        [MenuItem("Carvino/Rebuild Prototype Scenes and Build Windows Development")]
        public static void RebuildScenesThenBuildDevelopment()
        {
            PrototypeSceneBuilder.Build();
            BuildDevelopment();
        }
    }
}
#endif
