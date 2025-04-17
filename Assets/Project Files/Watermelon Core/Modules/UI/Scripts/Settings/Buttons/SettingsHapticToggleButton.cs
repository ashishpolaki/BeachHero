#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;

namespace Bokka
{
    public class SettingsHapticToggleButton : SettingsButtonBase
    {
        [SerializeField] Image imageRef;

        [Space]
        [SerializeField] Sprite activeSprite;
        [SerializeField] Sprite disableSprite;

        private bool isActive = true;

        public override void Init()
        {

        }

        private void OnEnable()
        {
            isActive = Haptic.IsActive;

            Redraw();

            Haptic.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            Haptic.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(bool value)
        {
            isActive = value;

            Redraw();
        }

        private void Redraw()
        {
            imageRef.sprite = isActive ? activeSprite : disableSprite;
        }

        public override void OnClick()
        {
            Haptic.IsActive = !isActive;

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}