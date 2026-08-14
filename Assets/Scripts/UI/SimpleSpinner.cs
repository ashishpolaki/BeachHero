using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class SimpleSpinner : MonoBehaviour
    {
        [SerializeField] private Image spinnerImage;
        [SerializeField] private GameObject bg;

        [Header("Rotation")]
        public bool Rotation = true;
        [Range(-10, 10), Tooltip("Value in Hz (revolutions per second).")]
        public float RotationSpeed = 1f;
        public AnimationCurve RotationAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Rainbow")]
        public bool Rainbow = true;
        [Range(-10, 10), Tooltip("Value in Hz (revolutions per second).")]
        public float RainbowSpeed = 0.5f;
        [Range(0, 1)]
        public float RainbowSaturation = 1f;
        public AnimationCurve RainbowAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Options")]
        public bool RandomPeriod = true;

        private CancellationTokenSource cts;

        public void StartSpinning()
        {
            StopSpinning();
            cts = new CancellationTokenSource();
            bg.gameObject.SetActive(true);
            AnimateAsync(cts.Token).Forget();
        }

        public void StopSpinning()
        {
            bg.gameObject.SetActive(false);
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        private async UniTaskVoid AnimateAsync(CancellationToken token)
        {
            float period = RandomPeriod ? Random.Range(0f, 1f) : 0f;

            while (!token.IsCancellationRequested)
            {
                float time = Time.time;

                if (Rotation)
                {
                    float rotEval = RotationAnimationCurve.Evaluate((RotationSpeed * time + period) % 1f);
                    spinnerImage.transform.localEulerAngles = new Vector3(0f, 0f, -360f * rotEval);
                }

                if (Rainbow && spinnerImage != null)
                {
                    float rainbowEval = RainbowAnimationCurve.Evaluate((RainbowSpeed * time + period) % 1f);
                    spinnerImage.color = Color.HSVToRGB(rainbowEval, RainbowSaturation, 1f);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }
}
