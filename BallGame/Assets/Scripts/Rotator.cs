using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed1 = 15f;
    public float rotationSpeed2 = 30f;
    public float rotationSpeed3 = 45f;

    void Update()
    {
        transform.Rotate(new Vector3(rotationSpeed1, rotationSpeed2, rotationSpeed3) * Time.deltaTime, Space.World);
    }
}