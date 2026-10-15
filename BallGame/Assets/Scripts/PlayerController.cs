using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Text scoreText;
    public Text winText;
    public Text timerText;
    public GameObject wall;
    private Rigidbody rb;
    public int score;
    public float fallLimit = -10f;
    public float restartDelay = 2f;
    private bool lost;
    private bool finished;
    private bool timerRunning;
    private float elapsedTime;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        score = 0;
        SetscoreText();
        winText.text = "";
        SetTimerText();
        AddTrail();
        StyleText(scoreText);
        StyleText(winText);
        StyleText(timerText);
    }

    // Glowing trail behind the ball
    void AddTrail()
    {
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.35f;
        trail.startWidth = 0.6f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(0.3f, 0.85f, 1f, 0.6f);
        trail.endColor = new Color(0.3f, 0.85f, 1f, 0f);
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // Bold text with a dark outline so it's readable on any background
    static void StyleText(Text text)
    {
        if (text == null)
        {
            return;
        }
        text.fontStyle = FontStyle.Bold;
        var outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    // Update is called once per frame
    void Update()
    {
        //Restart level
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
        //Quit game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Game over: no more moving
        if (finished)
        {
            return;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Timer starts on the player's first input
        if (!timerRunning && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            timerRunning = true;
        }
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
            SetTimerText();
        }

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        // Make controls relative to the camera, so "up" always moves away from the camera
        if (Camera.main != null)
        {
            movement = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * movement;
        }

        rb.AddForce(movement * speed);

        //Fell off the platform
        if (!lost && transform.position.y < fallLimit)
        {
            lost = true;
            winText.text = "You fell! Restarting...";
            Invoke(nameof(Restart), restartDelay);
        }
    }

     void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Finish") && !finished && !lost)
        {
            // Crossed the finish line: stop the ball and show the win text
            finished = true;
            timerRunning = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            winText.text = "You won!\n"
                + "Coins: " + score + "\n"
                + "Time: " + FormatTime(elapsedTime) + "\n"
                + "Press R to restart or ESC to quit";
        }

        if(other.gameObject.CompareTag("coin"))
        {
            var coin = other.GetComponent<Rotator>();
            if (coin != null)
            {
                coin.Collect();
            }
            else
            {
                other.gameObject.SetActive(false);
            }
            score++;
            SetscoreText();
            if (score >= 1) // TODO: set back to 10 after testing
            {
                wall.gameObject.SetActive(false);
            }
        }
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SetTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + FormatTime(elapsedTime);
        }
    }

    // Formats seconds as mm:ss.hh, e.g. 01:23.45
    static string FormatTime(float seconds)
    {
        int hundredths = (int)(seconds * 100);
        int minutes = hundredths / 6000;
        float rest = (hundredths % 6000) / 100f;
        return minutes.ToString("00") + ":" + rest.ToString("00.00", System.Globalization.CultureInfo.InvariantCulture);
    }

    void SetscoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}
