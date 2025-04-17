using UnityEngine;

namespace Bokka.BeachRescue
{
    [RequireComponent(typeof(Animator))]
    public class StartPointBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer graphicsSpRenderer;

        public Vector3 Position { get => transform.position; }
        public Quaternion Rotation { get => transform.rotation; }
        public SpriteRenderer MainRenderer { get => graphicsSpRenderer; }

        private Animator animatorRef;

        private int idleAnimationHash;
        private int lightAnimationHash;

        private void Awake()
        {
            animatorRef = GetComponent<Animator>();

            idleAnimationHash = Animator.StringToHash("IdleAnimation");
            lightAnimationHash = Animator.StringToHash("LightHighlight");
        }

        public void Init()
        {
            PlayIdleAnimation();

            graphicsSpRenderer.gameObject.SetActive(true);
            graphicsSpRenderer.color = Color.white;
        }

        public void PlayLightAnimation()
        {
            animatorRef.SetTrigger(lightAnimationHash);
        }

        public void PlayIdleAnimation()
        {
            animatorRef.SetTrigger(idleAnimationHash);
        }
    }
}