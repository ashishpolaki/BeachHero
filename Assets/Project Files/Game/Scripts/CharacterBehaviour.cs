using UnityEngine;

namespace Watermelon.BeachRescue
{
    public class CharacterBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] Color unselectedZoneColor = Color.white;

        [Header("References")]
        [SerializeField] WaitingTimerBehaviour waitingTimerRef;
        [SerializeField] Transform graphicsHolderTransform;
        [SerializeField] ParticleSystem pickedUpParticle;
        [SerializeField] GameObject bayRingObject;
        [SerializeField] GameObject zoneIdicatorObject;
        [SerializeField] SpriteRenderer zoneIndicatorSprRenderer;

        public bool IsActive { get; private set; }
        public bool IsSaved { get; private set; }

        private CharacterSave data;
        private Animator animatorRef;

        private int DRAWN_HASH = Animator.StringToHash("Drown");
        private int IDLE_HASH = Animator.StringToHash("Idle");

        private void Awake()
        {
            animatorRef = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            pickedUpParticle.gameObject.SetActive(false);
        }

        public void Init(CharacterSave characterData)
        {
            data = characterData;
            ResetCharacter();

            animatorRef.SetTrigger(IDLE_HASH);
            bayRingObject.SetActive(true);

            waitingTimerRef.Init(data.WaitingPercentage, OnTimeIsUp);
        }

        public void RunSimulation()
        {
            waitingTimerRef.RunCountdown();
            IsActive = true;
            zoneIdicatorObject.SetActive(false);
        }

        public void ResetCharacter()
        {
            IsActive = false;
            IsSaved = false;

            graphicsHolderTransform.gameObject.SetActive(true);
            waitingTimerRef.Init(data.WaitingPercentage, OnTimeIsUp);
            zoneIdicatorObject.SetActive(true);
            zoneIndicatorSprRenderer.color = unselectedZoneColor;

            animatorRef.SetTrigger(IDLE_HASH);
            bayRingObject.SetActive(true);
        }

        public void Save()
        {
            if (IsSaved || !IsActive)
                return;

            IsSaved = true;
            IsActive = false;
            graphicsHolderTransform.gameObject.SetActive(false);
            waitingTimerRef.HideTimer();

            pickedUpParticle.PlayCase();

            LevelController.Instance.OnCharacterSaved();
        }

        private void OnTimeIsUp()
        {
            IsActive = false;

            animatorRef.SetTrigger(DRAWN_HASH);
            bayRingObject.SetActive(false);

            LevelController.Instance.OnCharacterDrowned();
        }

        public void InitTimerForLevelEditor(float waitingPercentage)
        {
            waitingTimerRef.InitTimerForLevelEditor(waitingPercentage);
        }
    }
}