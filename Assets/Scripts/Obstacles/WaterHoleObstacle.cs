using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class WaterHoleObstacle : Obstacle
    {
        [SerializeField] private Material waterMaterial;
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private float rotationSpeed = 300f; // Speed of rotation around the cyclone
        [SerializeField] private float pullToCenterSpeed = 1f; // Speed at which the object is pulled toward the center
        [SerializeField] private float turbulenceIntensity = 0.6f; // Intensity of random turbulence
        [SerializeField] private float turbulenceFrequency = 2f; // Frequency of turbulence changes
        [SerializeField] private float tiltIntensity = 15f; // Maximum tilt angle for the boat
        [SerializeField] private float tiltSpeed = 2f; // Speed of tilt changes
        [SerializeField] private float depth = -10; // Target depth (y position) the object should reach
        [SerializeField] private float descendSpeed = 1; // Speed at which the object descends
        [SerializeField] private float radiusMultiplier = 0.8f; // Multiplier for the radius of the cyclone effect

        private bool canStartCyclone = false; // Flag to check if the cyclone can start
        private float radius;
        private float angle;
        private Transform targetTransform;
        private Coroutine cycloneCoroutine;
        private int whirlpoolDistanceID = Shader.PropertyToID("_WhirlpoolDistance");
        private int whirlpoolPositionID = Shader.PropertyToID("_WhirlpoolPosition");
        private int whirlpoolEnableID = Shader.PropertyToID("_IsWhirlpoolEnable");

        public void Init(WaterHoleObstacleData obstacleData)
        {
            StopCycloneEffect();
            canStartCyclone = false;
            transform.position = obstacleData.position;
            sphereCollider.radius = obstacleData.scale * radiusMultiplier;

            //water Shader WhirlPool Data
            waterMaterial.SetVector(whirlpoolPositionID, obstacleData.shaderPosition);
            waterMaterial.SetFloat(whirlpoolDistanceID, obstacleData.scale / 20f);
            waterMaterial.SetFloat(whirlpoolEnableID, 1f);
        }

        public override void Hit()
        {
            base.Hit();
        }
        public override void ResetObstacle()
        {
            base.ResetObstacle();
            waterMaterial.SetFloat(whirlpoolEnableID,0f);
            waterMaterial.SetFloat(whirlpoolDistanceID, 0f); // Reset whirlpool distance
        }

        public void OnPlayerHit(Transform playerTransform)
        {
            targetTransform = playerTransform;
            Vector3 offset = targetTransform.position - this.transform.position;
            angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            radius = Vector3.Distance(transform.position, targetTransform.position);
            canStartCyclone = true;
            StartCycloneEffect();
        }
        private void StartCycloneEffect()
        {
            if (cycloneCoroutine != null)
            {
                StopCoroutine(cycloneCoroutine);
            }
            cycloneCoroutine = StartCoroutine(CycloneEffectCoroutine());
        }
        private void StopCycloneEffect()
        {
            if (cycloneCoroutine != null)
            {
                StopCoroutine(cycloneCoroutine);
                cycloneCoroutine = null;
            }
        }
        private IEnumerator CycloneEffectCoroutine()
        {
            while (canStartCyclone)
            {
                // Gradually reduce the radius to simulate being pulled toward the center
                radius = Mathf.Max(0, radius - pullToCenterSpeed * Time.deltaTime);

                // Calculate the new position in a circular path
                angle -= rotationSpeed * Time.deltaTime;
                float x = transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * Mathf.Max(radius, 0.1f);
                float z = transform.position.z + Mathf.Sin(angle * Mathf.Deg2Rad) * Mathf.Max(radius, 0.1f);

                // Gradually move the object toward the target depth
                float y = Mathf.MoveTowards(targetTransform.position.y, depth, descendSpeed * Time.deltaTime);

                // Add turbulence for a more dynamic effect
                float turbulenceX = Mathf.PerlinNoise(Time.time * turbulenceFrequency, 0) * turbulenceIntensity;
                float turbulenceZ = Mathf.PerlinNoise(0, Time.time * turbulenceFrequency) * turbulenceIntensity;

                // Update the player's position with turbulence
                targetTransform.position = new Vector3(x + turbulenceX, y, z + turbulenceZ);

                // Add rotation changes to simulate the boat being tossed around
                float tiltX = Mathf.Sin(Time.time * tiltSpeed) * tiltIntensity; // Tilting forward and backward
                float tiltZ = Mathf.Cos(Time.time * tiltSpeed) * tiltIntensity; // Tilting side to side

                // Apply the rotation to the boat
                Quaternion targetRotation = Quaternion.Euler(tiltX, angle, tiltZ);
                targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, targetRotation, Time.deltaTime * tiltSpeed);

                yield return null; // Wait for the next frame
            }
        }
    }
}
