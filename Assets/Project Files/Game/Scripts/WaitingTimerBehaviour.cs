using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Bokka.BeachRescue
{
    public class WaitingTimerBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Canvas canvasRef;
        [SerializeField] Image fillImage;
        [SerializeField] GameObject crossImageObject;
        [SerializeField] GameObject fillBarObject;

        private Action CountdownCompletedCallback;

        private float currentWaitingPercentage;
        private float maxWaitingTime;
        private float timeLeft;

        private Coroutine CountdownCoroutineRef;

        public void Init(float waitingPercentage, Action OnCountdownCompletedCallback)
        {
            currentWaitingPercentage = waitingPercentage;
            ResetTimer();

            CountdownCompletedCallback = OnCountdownCompletedCallback;
        }

        public void ResetTimer()
        {
            StopCountdown();
            fillImage.fillAmount = currentWaitingPercentage;

            maxWaitingTime = LevelController.WaitingTimeLength;
            timeLeft = LevelController.WaitingTimeLength * currentWaitingPercentage;

            crossImageObject.SetActive(false);
            fillBarObject.SetActive(true);

            ShowTimer();
        }

        private void ShowTimer()
        {
            canvasRef.enabled = true;
        }

        public void HideTimer()
        {
            StopCountdown();
            canvasRef.enabled = false;
        }

        public void RunCountdown()
        {
            CountdownCoroutineRef = StartCoroutine(CountdownCoroutine());
        }

        public void StopCountdown()
        {
            if (CountdownCoroutineRef != null)
            {
                StopCoroutine(CountdownCoroutineRef);
            }
        }

        private IEnumerator CountdownCoroutine()
        {
            while (timeLeft > 0)
            {

                fillImage.fillAmount = Mathf.Clamp01(timeLeft / maxWaitingTime);
                timeLeft -= Time.deltaTime;

                yield return null;
            }

            crossImageObject.SetActive(true);
            fillBarObject.SetActive(false);

            CountdownCompletedCallback?.Invoke();
        }

        public void InitTimerForLevelEditor(float waitingPercentage)
        {
            fillImage.fillAmount = waitingPercentage;
        }
    }
}