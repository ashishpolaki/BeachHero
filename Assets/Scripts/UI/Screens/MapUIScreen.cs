using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MapUIScreen : BaseScreen
    {
        [SerializeField] private Button mapExitBtn;
        [SerializeField] private ParticleSystem confettiParticleSystem;
        [SerializeField] private float confettiDelay = 1f;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            var particle = confettiParticleSystem.main;
            particle.startDelay = confettiDelay;
            CameraController.GetInstance.SetActiveCamera(GameCameraType.Map);
            MapController.GetInstance.InitializeMapVisuals();

            //RegisterEvents
            mapExitBtn.ButtonRegister(MapExitToHome);
            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsEnabled += () => SetMapButtonsVisibility(true);
            }
        }

        public override void Close()
        {
            base.Close();
           // SetMapButtonsVisibility(false);
            mapExitBtn.ButtonDeRegister(MapExitToHome);
            if (MapController.GetInstance != null)
            {
                MapController.GetInstance.OnMapButtonsEnabled -= () => SetMapButtonsVisibility(false);
            }
        }

        private void SetMapButtonsVisibility(bool _val)
        {
            mapExitBtn.gameObject.SetActive(_val);
        }

        private void MapExitToHome()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.NotStarted);
        }
    }
}
