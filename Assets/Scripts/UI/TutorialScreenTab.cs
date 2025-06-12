using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace BeachHero
{
    public class TutorialScreenTab : BaseScreenTab
    {
        [SerializeField] private RectTransform handObject;
        [SerializeField] private Image panelImage;
        [SerializeField] private Image handImage;
        [SerializeField] private float handMoveDuration = 1.4f;
        [SerializeField] private float handScaleDuration = 0.5f;
        [SerializeField] private float handScaleElasticity = 0.2f;
        [SerializeField] private float handScalePunch = 0.2f;
        [SerializeField] private float panelFadeDuration = 0.5f;

        private Camera cam;
        private Color handImageColor;
        private Color panelImageColor;

        private void OnEnable()
        {
            GameController.GetInstance.TutorialController.OnFTUEPlayerTouchAction += OnPlayerTouch;
            GameController.GetInstance.TutorialController.OnFTUEPathDrawnAction += OnPathDrawn;
        }
        private void OnDisable()
        {
            GameController.GetInstance.TutorialController.OnFTUEPlayerTouchAction -= OnPlayerTouch;
            GameController.GetInstance.TutorialController.OnFTUEPathDrawnAction -= OnPathDrawn;
        }
        private void OnPathDrawn()
        {
            handObject.DOKill();
            handObject.localScale = Vector3.one;
            //Fade panel and handImage
            handImage.DOFade(0, panelFadeDuration);
            panelImage.DOFade(0, panelFadeDuration).OnComplete(() =>
            {
                handImage.DOKill();
                panelImage.DOKill();
                handImage.color = handImageColor;
                panelImage.color = panelImageColor;
                Close();
            });
        }
        public override void Open()
        {
            base.Open();
            //Store alpha values
            handImageColor = handImage.color;
            panelImageColor = panelImage.color;
            cam = GameController.GetInstance.LevelController.Cam;
            Vector3 playerWorldPos = GameController.GetInstance.LevelController.PlayerTransform.position;
            Vector3 playerScreenPos = cam.WorldToScreenPoint(playerWorldPos);
            handObject.position = playerScreenPos;
            handObject.DOKill();
            handObject.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).SetLoops(-1);
        }

        private void OnPlayerTouch()
        {
            handObject.DOKill();
            Vector3 drowningCharacterPos = cam.WorldToScreenPoint(GameController.GetInstance.LevelController.DrownCharacter.position);
            handObject.DOMove(drowningCharacterPos, handMoveDuration).SetLoops(-1);
        }
    }
}
