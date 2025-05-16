using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameLoseScreen : BaseScreen
    {
        [SerializeField] private Button retryButton;

        private void OnEnable()
        {
           retryButton.onClick.AddListener(OnRetryClick);
        }
        private void OnDisable()
        {
            retryButton.onClick.RemoveListener(OnRetryClick);
        }

        private void OnRetryClick()
        {
            Close();
            GameController.GetInstance.RetryLevel();
        }
    }
}
