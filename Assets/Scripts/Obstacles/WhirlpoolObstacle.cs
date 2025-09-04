using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class WhirlpoolObstacle : Obstacle
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
        [SerializeField] private float gameOverdelay = 1; // Delay before the hit effect is applied
        [SerializeField] private float failureHapticDuration = 0.5f; 
        [SerializeField] private float failureHapticCooldown = 0.3f; 

        private bool canStartCyclone = false; // Flag to check if the cyclone can start
        private float radius;
        private float angle;
        private float targetSpeed;
        private Transform targetTransform;
        private Coroutine cycloneCoroutine;
        private int index = -1;

        public void Init(WhirlpoolObstacleData obstacleData, int index)
        {
            this.index = index;
            StopCycloneEffect();
            canStartCyclone = false;
            transform.position = obstacleData.position;
            sphereCollider.radius = obstacleData.scale * radiusMultiplier;

            //water Shader WhirlPool Data
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_DISTANCE}_{index}"), obstacleData.scale / 20f);
            waterMaterial.SetVector(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_POSITION}_{index}"), obstacleData.shaderPosition);
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{index}"), 1f);
        }

        public override async void Hit()
        {
            OnPlayerHit();
            PlayVibration();
            await Task.Delay((int)(gameOverdelay * 1000)); // Wait before calling game over panel
            base.Hit();
        }
        public void OnDisable()
        {
            ResetWaterMaterial();
        }
        private void OnDestroy()
        {
            ResetWaterMaterial();
        }
        private async void PlayVibration()
        {
            float startCooldownTime = Time.time;
            while (Time.time - startCooldownTime < failureHapticDuration)
            {
                HapticsManager.GetInstance.FailureHapticWithCooldown(failureHapticCooldown);
                await Task.Yield();
            }
        }
        private void ResetWaterMaterial()
        {
            if (index < 0)
            {
                return;
            }
            waterMaterial.SetFloat(Shader.PropertyToID($"{StringUtils.WHIRLPOOL_ENABLE}_{index}"), 0f);
        }
        private void OnPlayerHit()
        {
            targetTransform = GameController.GetInstance.LevelController.PlayerTransform;
            targetSpeed = GameController.GetInstance.LevelController.GetPlayerSpeed();
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
            Vector3 cycloneCenter = transform.position;

            while (canStartCyclone)
            {
                // Gradually reduce the radius to simulate being pulled toward the center
                radius = Mathf.Max(0, radius - pullToCenterSpeed * Time.deltaTime);

                // Calculate the cyclone spiral target position
                angle -= rotationSpeed * Time.deltaTime;
                float targetX = cycloneCenter.x + Mathf.Cos(angle * Mathf.Deg2Rad) * Mathf.Max(radius, 0.1f);
                float targetZ = cycloneCenter.z + Mathf.Sin(angle * Mathf.Deg2Rad) * Mathf.Max(radius, 0.1f);

                // Gradually move the object toward the target depth
                float targetY = Mathf.MoveTowards(targetTransform.position.y, depth, descendSpeed * Time.deltaTime);

                // Add turbulence
                float turbulenceX = Mathf.PerlinNoise(Time.time * turbulenceFrequency, 0) * turbulenceIntensity;
                float turbulenceZ = Mathf.PerlinNoise(0, Time.time * turbulenceFrequency) * turbulenceIntensity;

                Vector3 cycloneTargetPos = new Vector3(targetX + turbulenceX, targetY, targetZ + turbulenceZ);

                // Move boat toward cyclone target at its own speed
                targetTransform.position = Vector3.MoveTowards(
                    targetTransform.position,
                    cycloneTargetPos,
                    targetSpeed * Time.deltaTime
                );

                // Tilting effect
                float tiltX = Mathf.Sin(Time.time * tiltSpeed) * tiltIntensity;
                float tiltZ = Mathf.Cos(Time.time * tiltSpeed) * tiltIntensity;

                Quaternion targetRotation = Quaternion.Euler(tiltX, angle, tiltZ);
                targetTransform.rotation = Quaternion.Slerp(
                    targetTransform.rotation,
                    targetRotation,
                    Time.deltaTime * tiltSpeed
                );

                yield return null;
            }
        }

    }
}
