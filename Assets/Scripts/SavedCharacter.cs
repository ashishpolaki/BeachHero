using UnityEngine;

namespace BeachHero
{
    public class SavedCharacter : MonoBehaviour
    {
        [SerializeField] private SavedCharacterUI savedCharacterUI;
        [SerializeField] private float waitTimePercentage;
        [SerializeField] private ParticleSystem pickUpParticle;
        [SerializeField] private GameObject graphicsSkin;
        [SerializeField] private GameObject graphicsUI;

        private float waitTime;
        private float levelTime;
        private bool isPickedUp = false;

        public void Init(Vector3 _position, float _waitTimePercentage, float levelTime)
        {
            isPickedUp = false;
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            this.levelTime = levelTime;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
            savedCharacterUI.UpdateTimer(waitTimePercentage);
        }

        public void UpdateState(float deltaTime)
        {
            if (isPickedUp)
            {
                return;
            }
            waitTime -= deltaTime;
            if (waitTime <= 0)
            {
                waitTime = 0;
            }
            float waitPercentage = Mathf.Clamp01(waitTime / levelTime);
            savedCharacterUI.UpdateTimer(waitPercentage);
        }

        public void OnPickUp()
        {
            pickUpParticle.gameObject.SetActive(true);
            pickUpParticle.Play();
            graphicsSkin.SetActive(false);
            graphicsUI.SetActive(false);

            isPickedUp = true;
        }
    }
}
