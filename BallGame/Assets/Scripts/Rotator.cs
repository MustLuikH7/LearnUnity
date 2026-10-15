using System.Collections;
using UnityEngine;

// Coin animation: stands on its edge, spins, bobs up and down, and pops when collected.
public class Rotator : MonoBehaviour
{
    public float spinSpeed = 180f;   // degrees per second
    public float bobHeight = 0.15f;  // how far up and down it floats
    public float bobSpeed = 3f;
    public float collectTime = 0.25f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private float phase;
    private bool collected;

    void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
        // Different phase per coin so they don't all move in sync (makes a wave along corridors)
        phase = (startPosition.x + startPosition.z) * 0.5f;
    }

    void Update()
    {
        if (collected)
        {
            return;
        }
        float t = Time.time;
        float angle = t * spinSpeed + phase * 40f;
        // Tip the flat cylinder up onto its edge, then spin it around the world up axis
        transform.rotation = Quaternion.Euler(0f, angle, 0f) * Quaternion.Euler(90f, 0f, 0f);
        transform.position = startPosition + Vector3.up * Mathf.Sin(t * bobSpeed + phase) * bobHeight;
    }

    public void Collect()
    {
        if (collected)
        {
            return;
        }
        collected = true;
        GetComponent<Collider>().enabled = false;
        Sparkle();
        StartCoroutine(Pop());
    }

    static Material sparkleMaterial;

    // Small burst of golden sparks
    void Sparkle()
    {
        if (sparkleMaterial == null)
        {
            sparkleMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        var go = new GameObject("CoinSparkle");
        go.transform.position = transform.position;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.4f), new Color(1f, 0.6f, 0.1f));
        main.gravityModifier = 1f;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        go.GetComponent<ParticleSystemRenderer>().material = sparkleMaterial;
        ps.Play();
    }

    // Quick spin, rise and shrink before disappearing
    IEnumerator Pop()
    {
        Vector3 from = transform.position;
        Quaternion rotation = transform.rotation;
        for (float t = 0f; t < collectTime; t += Time.deltaTime)
        {
            float k = t / collectTime;
            transform.position = from + Vector3.up * k * 1.2f;
            transform.rotation = Quaternion.Euler(0f, k * 720f, 0f) * rotation;
            float size = k < 0.3f ? Mathf.Lerp(1f, 1.5f, k / 0.3f) : Mathf.Lerp(1.5f, 0f, (k - 0.3f) / 0.7f);
            transform.localScale = startScale * size;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
