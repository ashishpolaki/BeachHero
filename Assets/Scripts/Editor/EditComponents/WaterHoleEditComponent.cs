#if UNITY_EDITOR
using UnityEngine;

namespace BeachHero
{
    public class WaterHoleEditComponent : MonoBehaviour
    {
        [Range(0, 10f)] public float cycloneRadius;
        [SerializeField] private GameObject waterGraphicsObject;
        [HideInInspector] public Vector2 shaderPosition;
        [SerializeField] private int index;
        private WaterHoleObstacle WaterHoleObstacle;
        private bool onValidate = false;
        private Vector3 whirlPoolLastPosition;

        public void Init(WaterHoleObstacleData waterHoleObstacleData, int index)
        {
            this.index = index;
            cycloneRadius = waterHoleObstacleData.scale;
            waterGraphicsObject = GameObject.Find("Water Graphics");
            if (waterGraphicsObject == null)
            {
                Debug.LogError("Water Graphics object not found in the scene. Please ensure it exists before initializing WaterHoleObstacle.");
                return;
            }
            // Set the position of the water graphics object to match the water hole obstacle data
            transform.position = new Vector3(waterHoleObstacleData.position.x, waterGraphicsObject.transform.position.y * 2f, waterHoleObstacleData.position.z);
            //WhirlPool Position
            Vector3 obstacleInversePosition = waterGraphicsObject.transform.InverseTransformPoint(waterHoleObstacleData.position);
            Vector3 whirlPoolPosition = new Vector3(0.5f, 0, 0.5f) - new Vector3(obstacleInversePosition.x, 0, obstacleInversePosition.z);
            shaderPosition = new Vector2(whirlPoolPosition.x, whirlPoolPosition.z);

            WaterHoleObstacle = GetComponent<WaterHoleObstacle>();
            WaterHoleObstacle.Init(waterHoleObstacleData, index);
            onValidate = true;
        }

        private void OnDrawGizmos()
        {
            if (transform.position != whirlPoolLastPosition)
            {
                whirlPoolLastPosition = transform.position;
            }
            //WhirlPool Position
            Vector3 obstacleInversePosition = waterGraphicsObject.transform.InverseTransformPoint(transform.position);
            Vector3 whirlPoolPosition = new Vector3(0.5f, 0, 0.5f) - new Vector3(obstacleInversePosition.x, 0, obstacleInversePosition.z);
            shaderPosition = new Vector2(whirlPoolPosition.x, whirlPoolPosition.z);
            WaterHoleObstacle.Init(new WaterHoleObstacleData
            {
                position = transform.position,
                shaderPosition = shaderPosition,
                scale = cycloneRadius,
            }, index);
        }

        private void OnValidate()
        {
            if (!onValidate) return;
            WaterHoleObstacle.Init(new WaterHoleObstacleData
            {
                position = transform.position,
                shaderPosition = shaderPosition,
                scale = cycloneRadius,
            }, index);
        }
    }
}
#endif
