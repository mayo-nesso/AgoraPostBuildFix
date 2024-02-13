using UnityEditor;
using UnityEngine;

namespace AgoraFix.Editor
{
    [CreateAssetMenu(fileName = "PostBuildConfiguration", menuName = "Agora/PostBuild Configuration", order = 10)]

    public class AgoraPostBuildConfiguration : ScriptableObject
    {
        private const string CONF_TOOLS_PATH = "CONF_TOOLS_PATH";
        private const string CONF_SIGN_NAME = "CONF_SIGN_NAME";
        
        [Tooltip("AppleSignatureName is part of the the Apple Developer Identity\n" +
                 "Is something like: 'my apple developer identity (xxxxx)'\n" +
                 "You can find it on the Keychain...")]
        [SerializeField] private string appleSignatureName = string.Empty;
        
        public static string AgoraToolsPath
        {
            get => PlayerPrefs.GetString(CONF_TOOLS_PATH);
            private set => PlayerPrefs.SetString(CONF_TOOLS_PATH, value);
        }
        
        public static string AppleSignatureName
        {
            get => PlayerPrefs.GetString(CONF_SIGN_NAME);
            private set => PlayerPrefs.SetString(CONF_SIGN_NAME, value);
        }
        
        public static void LookForSDK()
        {
            var path = EditorUtility.OpenFolderPanel("Find AgoraSDK Tools folder", "Assets", "");
            if ( string.IsNullOrEmpty(path))
            {
                return;
            }
            // TODO, if not code / sign sh files, then alert!
            
            // Make it relative to project...
            var relativePath = path.Replace(Application.dataPath, ""); 
            if (AgoraToolsPath != relativePath)
            {
                AgoraToolsPath = relativePath;
            }
        }

        public void OnValidate()
        {
            // Since AgoraToolsPath is handled by LookForSDK logic, we
            // have to only take care of saving appleSignatureName... 
            if (AppleSignatureName != appleSignatureName)
            {
                AppleSignatureName = appleSignatureName;
            }
        }
    }
    
    [CustomEditor(typeof(AgoraPostBuildConfiguration))]
    public class ConfigurationCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (AgoraPostBuildConfiguration)target;
            
            GUILayout.Label("Agora Tools Path:", GUILayout.Height(15));
            GUILayout.Label(AgoraPostBuildConfiguration.AgoraToolsPath, GUILayout.Height(20));
            
            if(GUILayout.Button("Setup Agora Tools Folder", GUILayout.Height(30)))
            {
                AgoraPostBuildConfiguration.LookForSDK();
            }
        }
    }
}