using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class UISettings : UIPage
    {
        [BoxGroup("References", "References")]
        [SerializeField] Image backgroundImage;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform panelRectTransform;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform contentRectTransform;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;

        public override void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            backgroundImage.AddEvent(EventTriggerType.PointerDown, OnBackgroundClicked);
        }

        public override void PlayShowAnimation()
        {
            RecalculatePanelSize();

            panelRectTransform.anchoredPosition = Vector2.down * 2000;
            panelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.SineOut);

            backgroundImage.SetAlpha(0);
            backgroundImage.DOFade(0.3f, 0.3f).OnComplete(() => UIController.OnPageOpened(this));
        }

        public override void PlayHideAnimation()
        {
            panelRectTransform.DOAnchoredPosition(Vector2.down * 2000, 0.3f).SetEasing(Ease.Type.SineIn);

            backgroundImage.DOFade(0, 0.3f).OnComplete(() => UIController.OnPageClosed(this));
        }

        private void RecalculatePanelSize()
        {
            float height = Mathf.Abs(contentRectTransform.sizeDelta.y);

            int childCount = contentRectTransform.childCount;
            for(int i = 0; i < childCount; i++)
            {
                Transform childTransform = contentRectTransform.GetChild(i);
                if (childTransform != null)
                {
                    SettingsElementsGroup settingsElementsGroup = childTransform.GetComponent<SettingsElementsGroup>();
                    if(settingsElementsGroup != null)
                    {
                        if (settingsElementsGroup.IsGroupActive())
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y;
                        }
                    }
                    else
                    {
                        if(childTransform.gameObject.activeSelf)
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y;
                        }
                    }
                }
            }

            panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, height);
        }

        private void OnCloseButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UISettings>();
        }

        private void OnBackgroundClicked(PointerEventData data)
        {
            UIController.HidePage<UISettings>();
        }
    }
}
