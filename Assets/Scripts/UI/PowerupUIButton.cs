using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeachHero
{
    public class PowerupUIButton : UIButton
    {
        [SerializeField] private PowerupType powerUpType;
        [SerializeField] private Button addMoreButton;
        [SerializeField] private Image iconImg;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject lockObj;
        [SerializeField] private Sprite buttonBgSprite;

        [Header("Tutorial References")]
        [SerializeField] private HandPointAnimation buttonAnimationData;

        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;

        private PowerupController powerupController;
        private bool isSelected = false;
        private int balance = 0;
        private bool isUnlocked = false;
        private bool isTutorialActive = false;

        public void Init(PowerupType _powerupType)
        {
            powerUpType = _powerupType;
            powerupController = GameController.GetInstance.PowerupController;
            if (powerupController.IsUnlockLevelForPowerup(powerUpType, GameController.GetInstance.CurrentLevelIndex + 1))
            {
                powerupController.UnlockPowerup(powerUpType);
                isTutorialActive = true;
                TryShowPowerupTutorials();
            }
            isUnlocked = powerupController.IsPowerupUnlocked(powerUpType);
            if (isUnlocked)
            {
                iconImg.gameObject.SetActive(true);
                counterText.gameObject.SetActive(true);
                balance = powerupController.GetPowerupBalance(powerUpType);
                SetCountText(balance);
                OnButtonReleased += OnPowerupButtonClicked;
            }
            else
            {
                lockObj.SetActive(true);
            }
        }

        public void TryShowPowerupTutorials()
        {
            var tc = TutorialController.GetInstance;
            tc.TutorialHand.PlayAnimation(buttonAnimationData, this.transform);
            tc.HighlightButton(this.transform, this.GetComponent<RectTransform>().sizeDelta, buttonBgSprite, false,
               () =>
               {
                   tc.EnsureTutorialCanvas(this.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER, IntUtils.TUTORIAL_CANVAS_LAYER);
               });
            tc.TutorialCharacter.PlayAnimation(TutorialCharacterState.WaveHand, tutorialCharacterPosition
            , () =>
            {
                tc.TutorialSpeechBubble.Show(StringUtils.SPEEDBOOST_POWERUP_TUTORIAL_MESSAGE, speechBubblePosition);
            });
        }

        public void DeInitialize()
        {
            if (isUnlocked)
            {
                OnButtonReleased -= OnPowerupButtonClicked;
            }
            selectedIndicator.SetActive(false);
            lockObj.SetActive(false);
            iconImg.gameObject.SetActive(false);
            counterText.gameObject.SetActive(false);
            isSelected = false;
        }
        private void SetCountText(int count)
        {
            counterText.text = count.ToString();
        }
        private void OnPowerupButtonClicked()
        {
            isSelected = !isSelected;
            selectedIndicator.SetActive(isSelected);
            if (isTutorialActive)
            {
                isTutorialActive = false;
                var tc = TutorialController.GetInstance;
                tc.RemoveTutorialCanvas(this.gameObject);
                tc.ClearButtonHighlight();
                tc.HideBlockerOverlay();
                tc.TutorialHand.Hide();
                tc.TutorialCharacter.SkipAnimation();
                tc.TutorialSpeechBubble.Hide();
            }
            if (isSelected)
            {
                GameController.GetInstance.PowerupController.AddPowerupInList(powerUpType);
            }
            else
            {
                GameController.GetInstance.PowerupController.RemovePowerupFromList(powerUpType);
            }
        }
    }
}
