using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinUI : MonoBehaviour
    {
        [SerializeField] private BoatCustomisationUIScreen boatCustomisationUIScreen;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image outerImage;
        [SerializeField] private Image innerImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject lockObject;

        [SerializeField] private Color outerImageSelectedColor;
        [SerializeField] private Color outerImageUnselectedColor;
        [SerializeField] private Color innerImageSelectedColor;
        [SerializeField] private Color innerImageUnselectedColor;

        private int index;
        private int currentColorIndex;
        private bool isLocked = false;

        #region Unity methods
        private void OnEnable()
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
        private void OnDisable()
        {
            selectButton.onClick.RemoveAllListeners();
        }
        #endregion

        public void SetSkin(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinSO newBoatSkin)
        {
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            index = newBoatSkin.Index;
            currentColorIndex = GameController.GetInstance.SkinController.GetSavedBoatColorIndex(newBoatSkin.Index);
            SetIcon(newBoatSkin.SkinColors[currentColorIndex].sprite);
            UpdateLockState();
        }
        private void SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }
        public void UpdateLockState()
        {
            isLocked = !GameController.GetInstance.SkinController.IsBoatSkinUnlocked(index);
            lockObject.SetActive(isLocked);
        }
        public void SetSelected()
        {
            outerImage.color = outerImageSelectedColor;
            innerImage.color = innerImageSelectedColor;
            selectButton.interactable = false;
        }
        public void SetUnSelected()
        {
            outerImage.color = outerImageUnselectedColor;
            innerImage.color = innerImageUnselectedColor;
            selectButton.interactable = true;
        }
        private void OnSelectButtonClicked()
        {
            boatCustomisationUIScreen.UpdateSelectedBoat(index);
        }
    }
}
