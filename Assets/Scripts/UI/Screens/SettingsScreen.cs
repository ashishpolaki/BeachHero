using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class SettingsScreen : BaseScreen
    {
        [SerializeField] private CustomToggle soundToggle;
        [SerializeField] private CustomToggle musicToggle;
        [SerializeField] private CustomToggle hapticToggle;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button closePanelbutton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            soundToggle.OnToggleChanged += OnSoundToggleChanged;
            musicToggle.OnToggleChanged += OnMusicToggleChanged;
            hapticToggle.OnToggleChanged += OnHapticToggleChanged;
            privacyPolicyButton.ButtonRegister(OnPrivacyPolicy);
            closePanelbutton.ButtonRegister(ClosePanel);

            // Initialize toggles based on saved settings
            soundToggle.Init(SaveSystem.LoadBool(StringUtils.SOUND_ON, true));
            musicToggle.Init(SaveSystem.LoadBool(StringUtils.MUSIC_ON, true));
            hapticToggle.Init(SaveSystem.LoadBool(StringUtils.HAPTICS_ON, true));
        }

        public override void Close()
        {
            base.Close();
            soundToggle.OnToggleChanged -= OnSoundToggleChanged;
            musicToggle.OnToggleChanged -= OnMusicToggleChanged;
            hapticToggle.OnToggleChanged -= OnHapticToggleChanged;
            privacyPolicyButton.ButtonDeRegister();
            closePanelbutton.ButtonDeRegister();
            soundToggle.Close();
            musicToggle.Close();
            hapticToggle.Close();
        }

        private void OnSoundToggleChanged(bool isOn)
        {
            AudioController.GetInstance.OnSoundToggleChange(isOn);
        }
        private void OnMusicToggleChanged(bool isOn)
        {
            AudioController.GetInstance.OnGameMusicToggleChange(isOn);
        }
        private void OnHapticToggleChanged(bool isOn)
        {
            SaveSystem.SaveBool(StringUtils.HAPTICS_ON, isOn);
            HapticsManager.GetInstance.ToggleHaptics(isOn);
        }
        private void OnPrivacyPolicy()
        {
            string privacyPolicyUrl = "https://docs.google.com/document/d/1_mwHvKDhOdo8nGuqsc6ngpCEDmX630t6OnWap1rGtJc/edit?usp=sharing";
            Application.OpenURL(privacyPolicyUrl);
        }
        private void ClosePanel()
        {
            Close();
        }
    }
}
