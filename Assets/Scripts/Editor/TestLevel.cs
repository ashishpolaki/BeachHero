#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class TestLevel : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
            AsyncLazyInit();
        }

        private async void AsyncLazyInit()
        {
            // small delay so the first frame renders smoothly
            await Task.Yield();
            GameController.GetInstance.Init();
            AudioController.GetInstance.Init();
            CameraController.GetInstance.Init();
            AdController.GetInstance.Init();
            HapticsManager.GetInstance.Init();
            TutorialController.GetInstance.Init();
            ParticleController.GetInstance.Initialize();
            Febucci.UI.Core.TAnimBuilder.InitializeGlobalDatabase();

            // wait a little to avoid freezing all at once
            await Task.Delay(100);
            GameController.GetInstance.SpawnLevel();
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Close);
            CameraController.GetInstance.SetActiveCamera(GameCameraType.GameView);
            GameController.GetInstance.SetGameState(GameState.Playing);
            GameController.GetInstance.LevelController.InitializePlayerData(false);
            UIController.GetInstance.ScreenEvent(ScreenType.Gameplay, UIScreenEvent.Open);
        }
    }
}
#endif
