using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    // LateUpdate on kaamera liigutamiseks parem, et vältida pildi värisemist
    void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}