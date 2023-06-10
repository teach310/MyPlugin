using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace PluginSampleEditor
{
    public class BuildPostprocessor : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 1; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            var buildTarget = report.summary.platform;
            if (buildTarget == BuildTarget.iOS)
            {
                ProcessForiOS(report);
            }

            Debug.Log("BuildPostprocessor.OnPostprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        }

        void ProcessForiOS(BuildReport report)
        {
            var buildOutputPath = report.summary.outputPath;
            UpdatePBXProject(buildOutputPath, project =>
            {
                DisableBitcode(project);
            });
        }

        void UpdatePBXProject(string buildOutputPath, Action<PBXProject> action)
        {
            var pbxProjectPath = PBXProject.GetPBXProjectPath(buildOutputPath);
            var project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);
            action(project);
            project.WriteToFile(pbxProjectPath);
        }

        // ENABLE_BITCODE は非推奨のパラメータのためNOにする
        void DisableBitcode(PBXProject project)
        {
            var mainTargetGuid = project.GetUnityMainTargetGuid();
            project.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");

            var unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            project.SetBuildProperty(unityFrameworkTargetGuid, "ENABLE_BITCODE", "NO");
        }
    }
}
