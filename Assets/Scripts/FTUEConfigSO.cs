using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public struct FTUEEntry
    {
        public FTUETutorialType tutorialType;
        public int levelNumber;
    }
    [CreateAssetMenu(fileName = "FTUEConfigSO", menuName = "Scriptable Objects/FTUEConfigSO")]
    public class FTUEConfigSO : ScriptableObject
    {
        public FTUEEntry[] entries;
    }
}
