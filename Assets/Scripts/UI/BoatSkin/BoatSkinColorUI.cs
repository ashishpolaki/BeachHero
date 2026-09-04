using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinColorUI : UIButton
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private GameObject selectedObject;
        [SerializeField] private GameObject selectedBorder;
        [SerializeField] private GameObject unSelectedBorder;


        private BoatCustomisationUIScreen boatCustomisationUIScreen;
        private int colorIndex;
        private bool isSelected = false;

        public bool IsSelected => isSelected;

        private void OnEnable()
        {
            OnButtonReleased += (OnSelectButtonClicked);
        }
        private void OnDisable()
        {
            OnButtonReleased -= (OnSelectButtonClicked);
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
            SetInteractable(false);
            selectedObject.SetActive(true);
            unSelectedBorder.SetActive(false);
            selectedBorder.SetActive(true);
        }

        public void UnSelect()
        {
            isSelected = false;
            SetInteractable(true);
            selectedObject.SetActive(false);
            selectedBorder.SetActive(false);
            unSelectedBorder.SetActive(true);
        }

        public override void SetInteractable(bool state)
        {
            base.SetInteractable(state);
        }
    }
}
