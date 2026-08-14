using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class LoginUIScreen : BaseScreen
    {
        [Header("Buttons")]
        [SerializeField] private UIButton gpgsSignInButton;
        [SerializeField] private UIButton guestLoginButton;

        [Header("Loading Spinner")]
        [SerializeField] private SimpleSpinner simpleSpinner;

        #region BaseScreen Overrides
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetLoadingState(false);
            AddListeners();
        }

        public override void Close()
        {
            base.Close();
            if (simpleSpinner != null)
            {
                simpleSpinner.StopSpinning();
                simpleSpinner.gameObject.SetActive(false);
            }
            RemoveListeners();
        }
        #endregion

        #region Listeners
        private void AddListeners()
        {
            if (gpgsSignInButton != null)
                gpgsSignInButton.OnButtonReleased += OnGPGSSignInClicked;
            if (guestLoginButton != null)
                guestLoginButton.OnButtonReleased += OnGuestLoginClicked;
        }

        private void RemoveListeners()
        {
            if (gpgsSignInButton != null) gpgsSignInButton.OnButtonReleased -= OnGPGSSignInClicked;
            if (guestLoginButton != null) guestLoginButton.OnButtonReleased -= OnGuestLoginClicked;
        }

        #endregion

        private void OnGPGSSignInClicked()
        {
            if (!NetworkController.IsInternetAvailable)
            {
                return;
            }
            SetLoadingState(true);
            PlayGamesController.GetInstance.SignIn(success =>
            {
                SetLoadingState(false);
                if (success)
                {
                    LoadMainMenuAsync();
                    Close();
                }
            });
        }

        private void OnGuestLoginClicked()
        {
            SaveSystem.SaveInt(StringUtils.AUTH_LOGIN_TYPE, 2); // 2 = Guest
            LoadMainMenuAsync();
            Close();
        }

        private async void LoadMainMenuAsync()
        {
            await UIController.GetInstance.LoadingUI.LoadSceneAsync(StringUtils.GAME_SCENE);
            GameController.GetInstance.SpawnLevel();
            await SceneLoader.GetInstance.UnloadScene(StringUtils.INIT_SCENE);
            AdController.GetInstance.Init();
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
        }

        private void SetLoadingState(bool isLoading)
        {
            if (simpleSpinner != null)
            {
                if (isLoading)
                {
                    simpleSpinner.StartSpinning();
                }
                else
                {
                    simpleSpinner.StopSpinning();
                }
            }
        }
    }
}
