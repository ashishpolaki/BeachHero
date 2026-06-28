using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinColorUI : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;

        private BoatCustomisationUIScreen boatCustomisationUIScreen;
        private int colorIndex;
        private bool isSelected = false;

        private void OnEnable()
        {
            selectButton.ButtonRegister(OnSelectButtonClicked);
        }
        private void OnDisable()
        {
            selectButton.ButtonDeRegister(OnSelectButtonClicked);
        }
        public void InitSkinColor(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinColorData skinColorData, int _index, bool _isSelected = false)
        {
            colorIndex = _index;
            iconImage.color = skinColorData.previewColor;
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            isSelected = _isSelected;
            selectButton.interactable = !_isSelected;
           // backgroundImage.DOFade(isSelected ? 1 : unSelectedFadeAlpha,0);
        }
        private void OnSelectButtonClicked()
        {
             boatCustomisationUIScreen.ApplyBoatColor(colorIndex);
        }
        public void Select()
        {
            isSelected = true;
          //  backgroundImage.DOFade(1 , 0);
            selectButton.interactable = false;
        }
        public void UnSelect()
        {
            isSelected = false;
         //   backgroundImage.DOFade(unSelectedFadeAlpha, 0);
            selectButton.interactable = true;
        }
    }
}
