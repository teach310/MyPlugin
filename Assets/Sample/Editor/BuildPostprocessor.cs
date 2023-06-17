using System;
using System.Collections.Generic;
using System.IO;
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
            UpdateInfoPlist(buildOutputPath, InfoPlistData());
            UpdatePBXProject(buildOutputPath, project =>
            {
                AddEntitlementsFile(buildOutputPath, project);
                DisableBitcode(project);
            });
        }

        void UpdateInfoPlist(string buildOutputPath, Dictionary<string, string> plistData)
        {
            var plistPath = Path.Combine(buildOutputPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            var rootDict = plist.root;
            foreach (var kvp in plistData)
            {
                rootDict.SetString(kvp.Key, kvp.Value);
            }
            plist.WriteToFile(plistPath);
        }

        Dictionary<string, string> InfoPlistData()
        {
            return new Dictionary<string, string>()
            {
                { "NSHealthShareUsageDescription", "read HealthKit Data" }
            };
        }

        void UpdatePBXProject(string buildOutputPath, Action<PBXProject> action)
        {
            var pbxProjectPath = PBXProject.GetPBXProjectPath(buildOutputPath);
            var project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);
            action(project);
            project.WriteToFile(pbxProjectPath);
        }

        // PBXProjectクラスによってentitlementsに「com.apple.developer.healthkit.access」を追加することができなかったため、手動で作ったものをコピーする
        void AddEntitlementsFile(string buildOutputPath, PBXProject pbxProject)
        {
            string baseEntitlementsFilePath = "BuildSettings/Unity-iPhone.entitlements";
            string entitlementsFileRelativePath = "Unity-iPhone/Unity-iPhone.entitlements";
            var entitlementsFilePath = Path.Combine(buildOutputPath, entitlementsFileRelativePath);
            File.Copy(baseEntitlementsFilePath, entitlementsFilePath);
            // Xcodeプロジェクトが読み込むentitlementsのパスを指定する
            // 競合させないために PlayerSettings > Other Settings > Configuration > Automatically add capabilities はオフにする
            pbxProject.AddBuildProperty(pbxProject.GetUnityMainTargetGuid(), "CODE_SIGN_ENTITLEMENTS", entitlementsFileRelativePath);

            // Xcodeプロジェクトを開いたときにNavigation Areaにentitlementsファイルが表示されるようにする
            string entitlementsFileProjectPath = Path.GetFileName(entitlementsFileRelativePath);
            pbxProject.AddFile(entitlementsFileRelativePath, entitlementsFileProjectPath, PBXSourceTree.Source);
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
