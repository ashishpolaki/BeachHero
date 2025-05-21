using UnityEngine;

namespace BeachHero
{
    public class CoinMagnet : MonoBehaviour
    {
        public float magnetRadius = 5f; // Radius within which coins are attracted
        public float attractionSpeed = 10f; // Speed at which coins are attracted to the player

        public Transform[] coins;

        private void Update()
        {
            foreach (Transform coinTransform in coins)
            {
                // Calculate the distance between the player and the coin
                float distance = Vector3.Distance(transform.position, coinTransform.transform.position);

                // If the coin is within the magnet radius, attract it toward the player
                CoinCollectable coin = coinTransform.GetComponent<CoinCollectable>();
                if (distance <= magnetRadius)
                {
                    if (!coin.CanMoveToTarget)
                    {
                        coin.SetTarget(transform);
                    }
                }
                coin.UpdateState();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize the magnet radius in the editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }
}
