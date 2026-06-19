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
        [SerializeField] private Color baseColor;
        [SerializeField] private Color baseTextureColor;
        [SerializeField] private float basePower = 2f;
        [SerializeField] private float closingPower = 10f;
        [SerializeField] private float shieldOffsetDuration = 30;

        [Header("Explode Settings")]
        [SerializeField] private Color explodeColor;
        [SerializeField] private Color explodeTextureColor;
        [SerializeField] private float explodeExpandMultiplier = 1.15f;
        [SerializeField] private float explodeExpandDuration = 0.2f;
        [SerializeField] private float explodeCollapseDuration = 0.3f;
        [SerializeField] private Vector3 explodeEndScale = Vector3.zero;
        [SerializeField] private float explodeDuration = 0.25f;
        [SerializeField] private Ease explodeExpandEase = Ease.OutQuad;
        [SerializeField] private Ease explodeCollapseEase = Ease.InQuad;

        private TweenHandle scaleTween;
        private TweenHandle powerTween;
        private TweenHandle textureOffsetTween;
        private TweenHandle colorTween;
        private TweenHandle colorTextureTween;

        private Material material;

        // Shader property IDs
        private static readonly int FresnelPowerId = Shader.PropertyToID("_Power");
        private static readonly int OffsetId = Shader.PropertyToID("_Offset");
        private static readonly int ColorId = Shader.PropertyToID("_Color_A");
        private static readonly int TextureColorId = Shader.PropertyToID("_Texture_1_Color");

        private void Awake()
        {
            transform.localScale = Vector3.zero;
            material = meshRenderer.sharedMaterial;
        }

        public void PlaySpawnAnimation()
        {
            material.SetColor(ColorId, baseColor);
            material.SetColor(TextureColorId, baseTextureColor);
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
            colorTween.Cancel();
            colorTextureTween.Cancel();
            transform.localScale = Vector3.zero;
            material.SetColor(ColorId, baseColor);
            material.SetColor(TextureColorId, baseTextureColor);
            // scaleTween = TweenManager.Scale(transform.localScale, Vector3.zero, transform, scaleReturnDuration,
            //     scaleReturnEase);
        }

        private void StartStretchLoop()
        {
            // Cancel any existing scale tween before starting the stretch loop
            scaleTween.Cancel();
            scaleTween = TweenManager.Scale(baseScale, stretchedScale, transform, stretchDuration,
                stretchEase, loops: stretchLoops, loopType: stretchLoopType);
            textureOffsetTween.Cancel();
            textureOffsetTween = TweenManager.SetFloat(0, 100, shieldOffsetDuration, (x) => material.SetVector(OffsetId, new Vector2(x, x)));
        }

        public void Explode()
        {
            scaleTween.Cancel();
            powerTween.Cancel();
            Vector3 currentScale = transform.localScale;
            Vector3 expandScale = currentScale * explodeExpandMultiplier;
            Vector3 endScale =  explodeEndScale;

            scaleTween = TweenManager.Scale(currentScale, expandScale, transform, explodeExpandDuration, explodeExpandEase, onComplete: () =>
                {
                    TweenManager.Scale(expandScale, endScale, transform, explodeCollapseDuration, explodeCollapseEase);
                });
            float colorTweenDuration = explodeExpandDuration + explodeCollapseDuration;
            colorTween = TweenManager.SetMaterialColor(material, ColorId, baseColor, explodeColor, colorTweenDuration);
            colorTextureTween = TweenManager.SetMaterialColor(material, TextureColorId, baseTextureColor, explodeTextureColor, colorTweenDuration);
            powerTween = TweenManager.SetFloat(basePower, closingPower, explodeDuration, (power) => material.SetFloat(FresnelPowerId, power));
        }
    }
}
