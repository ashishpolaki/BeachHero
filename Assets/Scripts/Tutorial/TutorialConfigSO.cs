using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public struct TutorialEntry
    {
        public TutorialType tutorialType;
        public int levelNumber;
    }
    [CreateAssetMenu(fileName = "TutorialConfigSO", menuName = "Scriptable Objects/TutorialConfigSO")]
    public class TutorialConfigSO : ScriptableObject
    {
        public TutorialEntry[] entries;
    }
}
