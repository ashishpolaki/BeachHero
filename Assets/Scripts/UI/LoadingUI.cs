using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeachHero
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private Image loadingBar;
        [SerializeField] private Camera renderCamera;
        [SerializeField] private GameObject panelBG;
        [SerializeField] private float loadingTime = 1;
        [SerializeField] private float referenceOrthoSize = 12f;

        public void EnableLoadingScreen(bool enable)
        {
            panelBG.SetActive(enable);
        }

        public async Task LoadSceneAsync(string sceneName)
        {
            renderCamera.orthographicSize = ScreenResolutionUtils.GetOrthographicSize(referenceOrthoSize);
            EnableLoadingScreen(true);
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;

            float barProgress = 0;
            while (barProgress <= loadingTime)
            {
                barProgress += Time.deltaTime;
                float progress = Mathf.Clamp01(barProgress / loadingTime);
                loadingBar.fillAmount = progress;
                await Task.Yield();
            }

            while (asyncOperation.progress < 0.9f)
            {
                await Task.Yield();
            }
            loadingBar.fillAmount = 1f; // Ensure the bar is full
            asyncOperation.allowSceneActivation = true;

            while (!asyncOperation.isDone)
                await Task.Yield();

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);
            EnableLoadingScreen(false);
        }
    }
}
