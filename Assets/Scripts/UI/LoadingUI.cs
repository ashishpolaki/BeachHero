using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeachHero
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private Image loadingFillImage;
        [SerializeField] private GameObject backgroundPanel;
        [SerializeField] private RectTransform tutorialCharacter;
        [SerializeField] private float minimumLoadingDuration = 1;
        [SerializeField] private Vector2 referenceCharacterSize = new Vector2(820, 820);

        private void SetActiveLoadingScreen(bool enable)
        {
            backgroundPanel.SetActive(enable);
        }

        private void UpdateTutorialCharacterSize()
        {
            Vector2 scaledSize = ScreenResolutionUtils.GetSizeDeltaFromOrthoReference(referenceCharacterSize.x, referenceCharacterSize.y);
            tutorialCharacter.sizeDelta = scaledSize;
        }

        public async Task LoadSceneAsync(string sceneName)
        {
            UpdateTutorialCharacterSize();
            SetActiveLoadingScreen(true);
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;

            float barProgress = 0;
            while (barProgress <= minimumLoadingDuration)
            {
                barProgress += Time.deltaTime;
                float progress = Mathf.Clamp01(barProgress / minimumLoadingDuration);
                loadingFillImage.fillAmount = progress;
                await Task.Yield();
            }

            while (asyncOperation.progress < 0.9f)
            {
                await Task.Yield();
            }
            loadingFillImage.fillAmount = 1f; // Ensure the bar is full
            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
                await Task.Yield();

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);
            SetActiveLoadingScreen(false);
        }
    }
}
