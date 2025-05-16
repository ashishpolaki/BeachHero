using UnityEngine;

public class CycloneEffect : MonoBehaviour
{
    public Transform cycloneCenter; // The center of the cyclone
    public float rotationSpeed = 50f; // Speed of rotation around the cyclone
    public float pullSpeed = 2f; // Speed at which the object is pulled toward the center
    public float depth = -10f; // Target depth (y position) the object should reach
    public float descendSpeed = 1f; // Speed at which the object descends

    public float turbulenceIntensity = 0.5f; // Intensity of random turbulence
    public float turbulenceFrequency = 2f; // Frequency of turbulence changes
    public float tiltIntensity = 15f; // Maximum tilt angle for the boat
    public float tiltSpeed = 2f; // Speed of tilt changes

    private float radius;
    private float angle = 0f;

    void Start()
    {
        // Calculate the initial angle based on the player's position relative to the cyclone center
        Vector3 offset = transform.position - cycloneCenter.position;
        angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;

        radius = Vector3.Distance(cycloneCenter.position, transform.position);
    }

    void Update()
    {
        // Gradually reduce the radius to simulate being pulled toward the center
        radius = Mathf.Max(0, radius - pullSpeed * Time.deltaTime);

        // Calculate the new position in a circular path
        angle += rotationSpeed * Time.deltaTime;
        float x = cycloneCenter.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * Mathf.Max(radius, 0.1f);
        float z = cycloneCenter.position.z + Mathf.Sin(angle * Mathf.Deg2Rad) * Mathf.Max(radius, 0.1f);

        // Gradually move the object toward the target depth
        float y = Mathf.MoveTowards(transform.position.y, depth, descendSpeed * Time.deltaTime);

        // Add turbulence for a more dynamic effect
        float turbulenceX = Mathf.PerlinNoise(Time.time * turbulenceFrequency, 0) * turbulenceIntensity;
        float turbulenceZ = Mathf.PerlinNoise(0, Time.time * turbulenceFrequency) * turbulenceIntensity;

        // Update the player's position with turbulence
        transform.position = new Vector3(x + turbulenceX, y, z + turbulenceZ);

        // Add rotation changes to simulate the boat being tossed around
        float tiltX = Mathf.Sin(Time.time * tiltSpeed) * tiltIntensity; // Tilting forward and backward
        float tiltZ = Mathf.Cos(Time.time * tiltSpeed) * tiltIntensity; // Tilting side to side

        // Apply the rotation to the boat
        Quaternion targetRotation = Quaternion.Euler(tiltX, angle, tiltZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

}
