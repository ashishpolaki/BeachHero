using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BeachHero
{
    public class LevelDatabaseCleanerPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        private string ASSETPATH = "Assets/ScriptableObjects/Levels/LevelsDatabase.asset";

        public void OnPreprocessBuild(BuildReport report)
        {
            var levelDatabase = AssetDatabase.LoadAssetAtPath<LevelDatabaseSO>(ASSETPATH);
            if (levelDatabase != null)
            {
                DebugUtils.Log("Clearing LevelDatabase before build...");
                levelDatabase.ClearLevelsData();
                EditorUtility.SetDirty(levelDatabase);
                AssetDatabase.SaveAssets();
            }
            else
            {
                DebugUtils.LogWarning("LevelDatabase asset not found at: " + ASSETPATH);
            }
        }
    }
}
