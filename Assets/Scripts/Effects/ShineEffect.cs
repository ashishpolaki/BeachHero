using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class ShineEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image image;

        [Header("Shader Settings")]
        [SerializeField] private float shineWidth = 0.05f;
        [SerializeField] private float shineIntensity = 0.48f;
        [SerializeField] private float shineRotation = 1.51f;

        [Header("Tween Settings")]
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private float duration = 1.5f;
        [SerializeField] private bool loop = true;

        private Material _mat;
        private TweenHandle shineTween;

        //Shader property IDs for performance
        private static readonly int ShineWidth = Shader.PropertyToID("_ShineWidth");
        private static readonly int ShineGlow = Shader.PropertyToID("_ShineGlow");
        private static readonly int ShineRotate = Shader.PropertyToID("_ShineRotate");
        private static readonly int ShineLocation = Shader.PropertyToID("_ShineLocation");

        private void Awake()
        {
            _mat = Instantiate(image.material);   
            image.material = _mat;
            _mat.SetFloat(ShineWidth, shineWidth);
            _mat.SetFloat(ShineGlow, shineIntensity);
            _mat.SetFloat(ShineRotate, shineRotation);
        }
        public void Play()
        {
            shineTween = TweenManager.SetFloat(0f, 1f, duration,
                 x => _mat.SetFloat(ShineLocation, x), ease, 2f, loop ? -1 : 1);
        }
        public void Stop()
        {
            _mat.SetFloat(ShineLocation, 0f);
            shineTween.Cancel();
        }
    }
}
