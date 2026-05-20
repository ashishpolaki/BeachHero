using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class SettingsScreen : BaseScreen
    {
        //[SerializeField] private CustomToggle soundToggle;
        //[SerializeField] private CustomToggle musicToggle;
        [SerializeField] private Slider soundSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private CustomToggle hapticToggle;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button closePanelbutton;
        [SerializeField] private Button backButton;
        [SerializeField] private float handleDisableValue = 0.175f;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            //soundToggle.OnToggleChanged += OnSoundToggleChanged;
            //musicToggle.OnToggleChanged += OnMusicToggleChanged;
            // hapticToggle.OnToggleChanged += OnHapticToggleChanged;
            privacyPolicyButton.ButtonRegister(OnPrivacyPolicy);
            closePanelbutton.ButtonRegister(ClosePanel);
            backButton.ButtonRegister(ClosePanel);
            LoadData();
        }

        public override void Close()
        {
            base.Close();
            // soundToggle.OnToggleChanged -= OnSoundToggleChanged;
            // musicToggle.OnToggleChanged -= OnMusicToggleChanged;
            hapticToggle.OnToggleChanged -= OnHapticToggleChanged;
            privacyPolicyButton.ButtonDeRegister(OnPrivacyPolicy);
            closePanelbutton.ButtonDeRegister(ClosePanel);
            backButton.ButtonDeRegister(ClosePanel);
        }

        private void LoadData()
        {
            // Initialize toggles based on saved settings
            //soundToggle.Init(SaveSystem.LoadBool(StringUtils.SOUND_ON, true));
            //musicToggle.Init(SaveSystem.LoadBool(StringUtils.MUSIC_ON, true));
            hapticToggle.Init(SaveSystem.LoadBool(StringUtils.HAPTICS_ON, true));
            soundSlider.value = AudioController.GetInstance.LoadSoundVolume();
            musicSlider.value = AudioController.GetInstance.LoadMusicVolume();
        }

        private void OnSoundSliderChanged(float value)
        {
            AudioController.GetInstance.SetSoundVolume(value);
            soundSlider.handleRect.gameObject.SetActive(value > handleDisableValue);
        }

        private void OnMusicSliderChanged(float value)
        {
            AudioController.GetInstance.SetMusicVolume(value);
            musicSlider.handleRect.gameObject.SetActive(value > handleDisableValue);
        }

        //private void OnSoundToggleChanged(bool isOn)
        //{
        //    AudioController.GetInstance.OnSoundToggleChange(isOn);
        //}
        //private void OnMusicToggleChanged(bool isOn)
        //{
        //    AudioController.GetInstance.OnGameMusicToggleChange(isOn);
        //}
        private void OnHapticToggleChanged(bool isOn)
        {
            SaveSystem.SaveBool(StringUtils.HAPTICS_ON, isOn);
            HapticsManager.GetInstance.ToggleHaptics(isOn);
            if (isOn)
            {
                HapticsManager.GetInstance.HeavyImpactHaptic();
            }
        }
        private void OnPrivacyPolicy()
        {
            string privacyPolicyUrl = "https://docs.google.com/document/d/1_mwHvKDhOdo8nGuqsc6ngpCEDmX630t6OnWap1rGtJc/edit?usp=sharing";
            Application.OpenURL(privacyPolicyUrl);
        }
        private void ClosePanel()
        {
            UIController.GetInstance.EndTransition();
            Close();
        }
    }
}
