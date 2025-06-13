using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameWinScreen : BaseScreen
    {
        [SerializeField] private Button nextLevelButton;

        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(OnNextLevel);
        }
        private void OnDisable()
        {
            nextLevelButton.onClick.RemoveListener(OnNextLevel);
        }
        private void OnNextLevel()
        {
            Close();
            GameController.GetInstance.RetryLevel();
        }
    }
}
