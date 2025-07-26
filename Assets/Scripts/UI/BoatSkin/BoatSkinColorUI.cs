using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BeachHero
{
    public class BoatSkinColorUI : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private float unSelectedFadeAlpha = 0.5f;

        private BoatCustomisationUIScreen boatCustomisationUIScreen;
        private int index;
        private bool isSelected = false;

        private void OnEnable()
        {
            selectButton.ButtonRegister(OnSelectButtonClicked);
        }
        private void OnDisable()
        {
            selectButton.ButtonDeRegister();
        }
        public void InitSkinColor(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinColorData skinColorData, int _index, bool _isSelected = false)
        {
            index = _index;
            iconImage.color = skinColorData.ShaderColors[0];
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            isSelected = _isSelected;
            selectButton.interactable = !_isSelected;
            backgroundImage.DOFade(isSelected ? 1 : unSelectedFadeAlpha,0);
        }
        private void OnSelectButtonClicked()
        {
             boatCustomisationUIScreen.SetBoatColor(index);
        }
        public void Select()
        {
            isSelected = true;
            backgroundImage.DOFade(1 , 0);
            selectButton.interactable = false;
        }
        public void UnSelect()
        {
            isSelected = false;
            backgroundImage.DOFade(unSelectedFadeAlpha, 0);
            selectButton.interactable = true;
        }
    }
}
