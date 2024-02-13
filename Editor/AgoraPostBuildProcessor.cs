using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AgoraFix.Editor
{
    public class AgoraPostBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private void FixScriptPaths(string prepSignPath, string signCodePath)
        {
            Debug.Log("FixPaths on prep_codesign.sh and signcode.sh");
            
            // From: `$PWD/$1` remove `$PWD/` resulting in `$1`  
            var psiPwd = new ProcessStartInfo
            {
                FileName = "sed",
                Arguments = $"-i '' \"s/\\$PWD\\/\\$1/\\$1/g\" {prepSignPath}" ,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var processPwd = Process.Start(psiPwd);
            processPwd.WaitForExit();
            Debug.Log(processPwd.StandardOutput.ReadToEnd());
            
            // From: `$APP/Contents/...` remove `Contents/` resulting in `$APP/...`  
            var psiContents = new ProcessStartInfo
            {
                FileName = "sed",
                Arguments = $"-i '' \"s/\\$APP\\/Contents/\\$APP/g\" {prepSignPath} {signCodePath}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var processContents = Process.Start(psiContents);
            processContents.WaitForExit();
            Debug.Log(processContents.StandardOutput.ReadToEnd());
        }

        private void PrepareReSigning(string prepSignPath, string exportedProjectPath)
        {
            var psiPrepareSigning = new ProcessStartInfo
            {
                FileName = prepSignPath,
                Arguments = exportedProjectPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psiPrepareSigning);
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();

            Debug.Log(output);
        }

        private void ReSignExported(string signCodePath, string signatureName, string exportedProjectPath)
        {
            var psiReSign = new ProcessStartInfo
            {
                FileName = signCodePath,
                Arguments = exportedProjectPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            psiReSign.EnvironmentVariables["SIGNATURE"] = signatureName;
            
            using var processReSign = Process.Start(psiReSign);
            processReSign.WaitForExit();
            var output = processReSign.StandardOutput.ReadToEnd();
            
            Debug.Log(output);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
#if PLATFORM_STANDALONE_OSX
            var agoraToolsPath = AgoraPostBuildConfiguration.AgoraToolsPath;
            var appleSignature = AgoraPostBuildConfiguration.AppleSignatureName;

            var prepSignPath = Path.Join(Application.dataPath, agoraToolsPath, "prep_codesign.sh");
            var signCodePath = Path.Join(Application.dataPath, agoraToolsPath, "signcode.sh");
            var exportedProjectPath = Path.Join(report.summary.outputPath, PlayerSettings.productName);
            
            Debug.Log($"PostBuildAgoraFix.OnPostprocessBuild for target {report.summary.platform} at path {exportedProjectPath}");

            if (string.IsNullOrEmpty(agoraToolsPath))
            {
                Debug.LogError("PostBuildAgoraFix `Agora Tools path` is not defined. \n" +
                               "Set values on `PostBuildConfiguration` scriptable object");
                return;
            }
            Debug.Log($"PostBuildAgoraFix.OnPostprocessBuild agoraToolsPath: {agoraToolsPath} ");
            
            if (string.IsNullOrEmpty(appleSignature))
            {
                Debug.LogError("PostBuildAgoraFix `Apple Signature Name` is not defined. \n" +
                               "Set values on `PostBuildConfiguration` scriptable object");
                return;
            }
            Debug.Log($"PostBuildAgoraFix.OnPostprocessBuild appleSignature: {appleSignature} ");

            FixScriptPaths(prepSignPath, signCodePath);
            PrepareReSigning(prepSignPath, exportedProjectPath);
            ReSignExported(signCodePath, appleSignature, exportedProjectPath);
#endif 
        }
    }

}
