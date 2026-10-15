using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    // Kui kõrgel kaamera palli kohal on
    public float height = 24f;
    // Kaamera kalle allapoole kraadides (90 = otse ülevalt)
    public float tilt = 65f;
    // Kaamera suund kraadides (-90 = vaatab labürindi poole, -X suunas)
    public float yaw = -90f;
    // Kui kaugele palli ette kaamera vaatab (pall on siis ekraani alumises osas)
    public float lookAhead = 8f;
    // Kui kiiresti kaamera pallile järele jõuab (väiksem = kiirem)
    public float smoothTime = 0.15f;

    private Vector3 velocity;

    void Start()
    {
        transform.position = TargetPosition();
        transform.rotation = Quaternion.Euler(tilt, yaw, 0f);
    }

    // LateUpdate on kaamera liigutamiseks parem, et vältida pildi värisemist
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(tilt, yaw, 0f);
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition(), ref velocity, smoothTime);
    }

    // Kaamera asukoht: palli taga ja kohal, nii et ta vaatab punkti, mis on lookAhead võrra palli ees
    Vector3 TargetPosition()
    {
        float back = height / Mathf.Tan(tilt * Mathf.Deg2Rad);
        Vector3 offset = new Vector3(0f, height, lookAhead - back);
        return player.transform.position + Quaternion.Euler(0f, yaw, 0f) * offset;
    }
}
