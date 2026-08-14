using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeachHero
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private Slider loadingFillSlider;
        [SerializeField] private GameObject backgroundPanel;
        [SerializeField] private UiScreenTextStyler uiScreenTextStyler;
        [SerializeField] private float minimumLoadingDuration = 1;
        [SerializeField] private float loadDuration = 0.5f;

        private void SetActiveLoadingScreen(bool enable)
        {
            backgroundPanel.SetActive(enable);
        }

        //  [SerializeField] private Vector2 referenceCharacterSize = new Vector2(820, 820);
        //private void UpdateTutorialCharacterSize()
        //{
        //    Vector2 scaledSize = ScreenResolutionUtils.GetSizeDeltaFromOrthoReference(referenceCharacterSize.x, referenceCharacterSize.y);
        //    tutorialCharacter.sizeDelta = scaledSize;
        //}

        public async Task ShowLoadingScreen()
        {
            SetActiveLoadingScreen(true);
            float barProgress = 0;
            while (barProgress <= loadDuration)
            {
                barProgress += Time.deltaTime;
                float progress = Mathf.Clamp01(barProgress / loadDuration);
                loadingFillSlider.value = progress;
                await Task.Yield();
            }
             loadingFillSlider.value = 1; // Ensure the bar is full
        }

        public async Task LoadSceneAsync(string sceneName)
        {
            if (uiScreenTextStyler != null)
            {
                uiScreenTextStyler.ApplyStyle();
            }
            SetActiveLoadingScreen(true);
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;

            float barProgress = 0;
            while (barProgress <= minimumLoadingDuration)
            {
                barProgress += Time.deltaTime;
                float progress = Mathf.Clamp01(barProgress / minimumLoadingDuration);
                loadingFillSlider.value = progress;
                await Task.Yield();
            }

            while (asyncOperation.progress < 0.9f)
            {
                await Task.Yield();
            }
            loadingFillSlider.value = 1f; // Ensure the bar is full
            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
                await Task.Yield();

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);
        }

        public async Task DisableLoadingScreen(float milliSeconds = 1)
        {
            await Task.Delay((int)milliSeconds);
            SetActiveLoadingScreen(false);
        }

        public void EnableLoadingScreen(bool value)
        {
            SetActiveLoadingScreen(value);
        }
    }
}
