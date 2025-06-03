using UnityEngine;

namespace BeachHero
{
    public class TutorialController : MonoBehaviour
    {
        private bool isTutorialCompleted = false;
        private int tutorialLevelNumber = 0;

        public void ActivateTutorial(int levelnumber)
        {
          
        }

        public bool IsTutorialActive(int levelNumber)
        {
            return tutorialLevelNumber == levelNumber;
        }
    }
}
