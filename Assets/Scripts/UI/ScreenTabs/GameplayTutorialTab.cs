using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Febucci.UI;

namespace BeachHero
{
    public class GameplayTutorialTab : BaseScreenTab
    {
        [SerializeField] private RectTransform handObject;
        [SerializeField] private TextAnimatorPlayer instructionText;
        [SerializeField] private Image panelImage;
        [SerializeField] private Image handImage;
        [SerializeField] private float handMoveDuration = 1.4f;
        [SerializeField] private float handScaleDuration = 0.5f;
        [SerializeField] private float handScaleElasticity = 0.2f;
        [SerializeField] private float handScalePunch = 0.2f;
        [SerializeField] private float panelFadeDuration = 0.5f;

        private Camera cam;
        private Color handImageColor;
        private FTUETutorialType currentFTUEType;


        public override void Open()
        {
            base.Open();
            if (GameController.GetInstance != null)
            {
                GameController.GetInstance.TutorialController.OnPathDrawnAction += OnPathDrawn;
            }
            currentFTUEType = GameController.GetInstance.TutorialController.CurrentFTUEType;
            cam = GameController.GetInstance.LevelController.Cam;
            handImageColor = handImage.color;
            OnHandTap();
            ShowInstructionText();
        }

        public override void Close()
        {
            base.Close();
            if (GameController.GetInstance != null)
            {
                GameController.GetInstance.TutorialController.OnPathDrawnAction -= OnPathDrawn;
            }
            handObject.DOKill();
            handImage.DOKill();
        }

        private void ShowInstructionText()
        {
            // Set instruction text based on the FTUE type
            if (currentFTUEType == FTUETutorialType.TapAndDrag)
            {
                instructionText.ShowText(StringUtils.TAP_AND_DRAG_TUTORIAL);
            }
            else if (currentFTUEType == FTUETutorialType.RescueAll)
            {
                instructionText.ShowText(StringUtils.RESCUE_ALL_TUTORIAL);
            }
        }

        private void HideInstructionText()
        {
            instructionText.StartDisappearingText();
        }

        private void OnPathDrawn()
        {
            HideInstructionText();
            //Fade handImage
            handImage.DOKill();
            handImage.DOFade(0, panelFadeDuration).OnComplete(() =>
            {
                handImage.color = handImageColor;
                Close();
            });
        }

        private void OnHandTap()
        {
            Vector3 playerWorldPos = GameController.GetInstance.LevelController.PlayerTransform.position;
            Vector3 playerScreenPos = cam.WorldToScreenPoint(playerWorldPos);
            handObject.localScale = Vector3.one; // Reset scale before applying punch
            handObject.position = playerScreenPos;
            handObject.DOKill();
            handObject.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).OnComplete(() =>
            {
                OnHandMove();
            });
        }

        private void OnHandMove()
        {
            handObject.DOKill();
            int characterIndex = currentFTUEType == FTUETutorialType.TapAndDrag ? 0 : 1;
            Vector3 drowningCharacterPos = cam.WorldToScreenPoint(GameController.GetInstance.LevelController.GetDrowningCharacter(characterIndex).position);
            handObject.DOMove(drowningCharacterPos, handMoveDuration).OnComplete(() =>
            {
                OnHandTap();
            });
        }

    }
}
