using LitMotion;
using UnityEngine;

namespace BeachHero
{
    public class ShieldEffect : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Vector3 baseScale = new Vector3(2.38f, 2.38f, 2.38f);

        [Header("Stretch Settings")]
        [SerializeField] private Vector3 stretchedScale = new Vector3(2.1f, 2.38f, 2.78f);
        [SerializeField] private float stretchAmount = 0.25f; // kept for backward-compat / potential use
        [SerializeField] private float stretchDuration = 0.4f;
        [SerializeField] private Ease stretchEase = Ease.InOutSine;
        [SerializeField] private int stretchLoops = -1;
        [SerializeField] private LoopType stretchLoopType = LoopType.Yoyo;
    
        [Header("Scale Settings")]
        [SerializeField] private float scaleDuration = 0.25f;
        [SerializeField] private float scaleReturnDuration = 0.1f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;
        [SerializeField] private Ease scaleReturnEase = Ease.InBack;

        [Header("Fresnel Settings")]
        [SerializeField] private float basePower = 2f;
        [SerializeField] private float closingPower = 10f;
        [SerializeField] private float shieldOffsetDuration = 30;

        private TweenHandle scaleTween;
        private TweenHandle powerTween;
        private TweenHandle textureOffsetTween;
        private Material material;

        // Shader property IDs
        private static readonly int FresnelPowerId = Shader.PropertyToID("_Power");
        private static readonly int OffsetId = Shader.PropertyToID("_Offset");

        private void Awake()
        {
            transform.localScale = Vector3.zero;
            material = meshRenderer.sharedMaterial;
        }

        public void PlaySpawnAnimation()
        {
            scaleTween = TweenManager.Scale(Vector3.zero, baseScale, transform, scaleDuration,
                scaleEase, onComplete: () => StartStretchLoop());
            powerTween = TweenManager.SetFloat(closingPower, basePower, scaleDuration, (power) => material.SetFloat(FresnelPowerId, power));
            transform.localScale = baseScale;
        }

        public void Stop()
        {
            scaleTween.Cancel();
            powerTween.Cancel();
            textureOffsetTween.Cancel();
            scaleTween = TweenManager.Scale(transform.localScale, Vector3.zero, transform, scaleReturnDuration,
                scaleReturnEase);
        }

        private void StartStretchLoop()
        {
            // Cancel any existing scale tween before starting the stretch loop
            // 
            scaleTween.Cancel();
            scaleTween = TweenManager.Scale(baseScale, stretchedScale, transform, stretchDuration,
                stretchEase, loops: stretchLoops, loopType: stretchLoopType);
            textureOffsetTween.Cancel();
            textureOffsetTween = TweenManager.SetFloat(0, 100, shieldOffsetDuration, (x) => material.SetVector(OffsetId, new Vector2(x, x)));
        }

        public void UpdateScale(Vector3 direction)
        {
            //Vector3 targetScale = new Vector3(
            //    baseScale.x + Mathf.Abs(direction.x) * stretchAmount,
            //    baseScale.y,
            //    baseScale.z);

            //// Smooth (example - scaleSpeed not defined here) 
            //transform.localScale = Vector3.Lerp(
            //    transform.localScale,
            //    targetScale,
            //    Time.deltaTime * scaleSpeed
            //);
        }
    }
}
