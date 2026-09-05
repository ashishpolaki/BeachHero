using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace BeachHero
{
    public class NotificationBadgeUI : MonoBehaviour
    {
        [SerializeField] private GameObject badgeRoot;

        [Header("Shake Rotation")]
        [SerializeField] private Transform shakeTarget;
        [SerializeField] private Vector3 shakeStrength = new Vector3(0, 0, 15f);
        [SerializeField] private int shakeFrequency = 10;
        [SerializeField] private float shakeDuration = 0.35f;
        [SerializeField] private float shakeDampingRatio = 0.5f;
        [SerializeField] private uint shakeRandomSeed = 123;
        [SerializeField] private Ease shakeEase = Ease.Linear;
        [SerializeField] private bool repeatShake = true;
        [SerializeField, Min(0.1f)] private float shakeRepeatInterval = 3f;

        private TweenHandle shakeHandle;
        private CancellationTokenSource shakeLoopCTS;

        private void OnDisable()
        {
            StopShakeRotationLoop();
        }

        private void OnDestroy()
        {
            StopShakeRotationLoop();
            shakeHandle.Cancel();
        }

        public void Show()
        {
            SetBadgeActive(true);
            PlayShakeRotation();
            StartShakeRotationLoop();
        }

        public void Hide()
        {
            SetBadgeActive(false);
            StopShakeRotationLoop();
        }

        public void PlayShakeRotation()
        {
            Transform target = shakeTarget ?? transform;
            Vector3 startEulerAngles = target.localEulerAngles;

            shakeHandle.Cancel();
            shakeHandle = TweenManager.ShakeRotation(target, startEulerAngles, shakeStrength, shakeFrequency, shakeDuration,
                shakeDampingRatio, shakeRandomSeed, shakeEase, TransformSpace.Local,
                onComplete: () =>
                {
                    if (target != null)
                    {
                        target.localEulerAngles = startEulerAngles;
                    }
                });
        }

        private void StartShakeRotationLoop()
        {
            StopShakeRotationLoop();

            if (!repeatShake)
            {
                return;
            }

            shakeLoopCTS = new CancellationTokenSource();
            PlayShakeRotationLoopAsync(shakeLoopCTS.Token).Forget();
        }

        private void StopShakeRotationLoop()
        {
            if (shakeLoopCTS != null)
            {
                shakeLoopCTS.Cancel();
                shakeLoopCTS.Dispose();
                shakeLoopCTS = null;
            }
        }

        private async UniTaskVoid PlayShakeRotationLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(shakeRepeatInterval), cancellationToken: token);

                    if (!token.IsCancellationRequested)
                    {
                        PlayShakeRotation();
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void SetBadgeActive(bool value)
        {
            if (badgeRoot != null)
            {
                badgeRoot.SetActive(value);
            }
        }
    }
}
