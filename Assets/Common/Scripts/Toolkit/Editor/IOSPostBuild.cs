using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

namespace OctoberStudio
{
    public static class IOSPostBuild
    {
#if UNITY_IOS
        [PostProcessBuild]
        private static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS)
            return;

            var pbxPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(pbxPath);

            var mainTarget = project.GetUnityMainTargetGuid();
            var frameworkTarget = project.GetUnityFrameworkTargetGuid();

            project.AddFrameworkToProject(mainTarget, "CoreHaptics.framework", false);
            project.AddFrameworkToProject(frameworkTarget, "CoreHaptics.framework", false);

            project.AddFrameworkToProject(mainTarget, "AudioToolbox.framework", false);
            project.AddFrameworkToProject(frameworkTarget, "AudioToolbox.framework", false);

            File.WriteAllText(pbxPath, project.WriteToString());
        }
#endif
    }
}