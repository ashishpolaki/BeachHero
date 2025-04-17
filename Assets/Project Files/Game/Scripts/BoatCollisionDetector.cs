using UnityEngine;

namespace Bokka.BeachRescue
{
    public class BoatCollisionDetector : MonoBehaviour
    {
        [SerializeField] BoatBehaviour boat;

        private void OnTriggerEnter(Collider other)
        {
            Vector3 collisionPoint = transform.position + (other.transform.position - transform.position) * 0.5f;

            var interactable = other.GetComponent<IInteractable>();

            if (interactable != null)
                interactable.Interact();

            if (other.CompareTag("Character"))
            {
                CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
                character.Save();
            }
            else if (other.CompareTag("Obstacle"))
            {
                boat.OnCollisionWithObstacle();
            }
        }

    }
}