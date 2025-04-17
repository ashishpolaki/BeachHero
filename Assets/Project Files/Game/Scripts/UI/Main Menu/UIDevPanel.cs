using UnityEngine;

namespace Bokka.BeachRescue
{
    public class UIDevPanel : MonoBehaviour
    {
        public void FirstLevelButton()
        {
            GameController.FirstLevelDev();
        }

        public void PrevLevelButton()
        {
            GameController.PrevLevelDev();
        }

        public void NextLevelButton()
        {
            GameController.NextLevelDev();
        }

        public void HideButton()
        {
            gameObject.SetActive(false);
        }
    }
}