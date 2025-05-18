using UnityEngine;

namespace BeachHero
{
    public class SavedCharacter : MonoBehaviour
    {
        [SerializeField] private SavedCharacterUI savedCharacterUI;
        [SerializeField] private ParticleSystem pickUpParticle;
        [SerializeField] private GameObject graphicsSkin;
        [SerializeField] private GameObject graphicsUI;
        [SerializeField] private Animator animatorRef;
        [SerializeField] private float waitTimePercentage;
        private float waitTime;
        private float levelTime;
        private bool isPickedUp = false;
        private bool isDrown;

        private int DRAWN_HASH = Animator.StringToHash("Drown");
        private int IDLE_HASH = Animator.StringToHash("Idle");

        public void Init(Vector3 _position, float _waitTimePercentage, float levelTime)
        {
            pickUpParticle.Stop();
            pickUpParticle.gameObject.SetActive(false);
            graphicsUI.SetActive(true);
            graphicsSkin.SetActive(true);
            animatorRef.SetTrigger(IDLE_HASH);
            isPickedUp = false;
            isDrown = false;
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            this.levelTime = levelTime;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
            savedCharacterUI.UpdateTimer(waitTimePercentage);
        }

        public void UpdateState()
        {
            if (isPickedUp || isDrown)
            {
                return;
            }
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                waitTime = 0;
                OnTimeUp();
            }
            float waitPercentage = Mathf.Clamp01(waitTime / levelTime);
            savedCharacterUI.UpdateTimer(waitPercentage);
        }
        public void OnTimeUp()
        {
            isDrown = true;
            animatorRef.SetTrigger(DRAWN_HASH);
            GameController.GetInstance.OnCharacterDrowned();
            graphicsUI.SetActive(false);
        }
        public void OnPickUp()
        {
            pickUpParticle.gameObject.SetActive(true);
            pickUpParticle.Play();
            graphicsSkin.SetActive(false);
            graphicsUI.SetActive(false);
            isPickedUp = true;
            GameController.GetInstance.OnCharacterPickUp();
        }
    }
}
