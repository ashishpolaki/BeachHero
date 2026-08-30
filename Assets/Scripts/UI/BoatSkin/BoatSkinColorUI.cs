using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinColorUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private UIButton selectButton;
        [SerializeField] private GameObject lockObject;

        private BoatCustomisationUIScreen boatCustomisationUIScreen;
        private int colorIndex;
        private bool isSelected = false;

        public bool IsSelected => isSelected;

        private void OnEnable()
        {
            selectButton.OnButtonReleased += (OnSelectButtonClicked);
        }
        private void OnDisable()
        {
            selectButton.OnButtonReleased -= (OnSelectButtonClicked);
        }
        public void InitSkinColor(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinColorData skinColorData,
            int _index, bool _isUnlocked, bool _isSelected = false)
        {
            colorIndex = _index;
            if (iconImage != null)
            {
                iconImage.color = skinColorData.previewColor;
            }
            if (lockObject != null)
            {
                lockObject.SetActive(!_isUnlocked);
            }
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            
            if (_isSelected)
            {
                Select();
            }
            else
            {
                UnSelect();
            }
        }

        private void OnSelectButtonClicked()
        {
            boatCustomisationUIScreen?.ApplyBoatColor(colorIndex);
        }

        public void Select()
        {
            isSelected = true;
            if (selectButton != null)
            {
                selectButton.SetInteractable(false);
            }
        }

        public void UnSelect()
        {
            isSelected = false;
            if (selectButton != null)
            {
                selectButton.SetInteractable(true);
            }
        }
    }
}
