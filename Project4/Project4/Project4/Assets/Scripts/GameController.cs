using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public GameObject bigAsteroid;
    public GameObject mediumAsteroid;
    public GameObject smallAsteroid;
    public GameObject UFO;
    public GameObject ship;

    private int score;
    private int hiscore;
    private int asteroidsRemaining;
    private int lives;
    private int wave;
    private int increaseEachWave = 1;

    public Text scoreText;
    public Text livesText;
    public Text hiscoreText;
    public Text waveText;

    // Use this for initialization
    void Start()
    {

        hiscore = PlayerPrefs.GetInt("hiscore", 0);
        BeginGame();
    }

    // Update is called once per frame
    void Update()
    {

        // Quit if player presses escape
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

    }

    void BeginGame()
    {

        score = 0;
        lives = 1;
        wave = 1;

        // Prepare the HUD
        scoreText.text = "SCORE:" + score;
        hiscoreText.text = "HISCORE: " + hiscore;
        livesText.text = "LIVES: " + lives;
        waveText.text = "WAVE: " + wave;

        SpawnAsteroids();
    }

    void SpawnAsteroids()
    {

        DestroyExistingAsteroids();

        // Decide how many asteroids to spawn
        // If any asteroids left over from previous game, subtract them
        //asteroidsRemaining = 1;
        Camera cam = Camera.main;

        if (wave < 10)
        {
            asteroidsRemaining = (wave * increaseEachWave);

            for (int i = 0; i < asteroidsRemaining; i++)
            {
                // Spawn an Asteroid
                int maxCount = 0;
                Vector3 startPosition = new Vector3(Random.Range(-530.0f, 530.0f),
                        0, Random.Range(-320.0f, 320.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, ship.transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    startPosition = new Vector3(Random.Range(-550.0f, 550.0f),
                        0, Random.Range(-330.0f, 330.0f));
                }

                Instantiate(bigAsteroid, startPosition,
                    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));

            }

            // After wave 5 spawn a UFO
            asteroidsRemaining += 1;
            if (wave > 4)
            {
                int maxCount = 0;
                int[] sign = { -1, 1 };
                int leftOrRight = Random.Range(0, 1);
                Vector3 startPosition = new Vector3(320 * sign[leftOrRight],
                        0, Random.Range(-185.0f, 185.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, ship.transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    leftOrRight = Random.Range(0, 1);
                    startPosition = new Vector3(320 * sign[leftOrRight],
                        0, Random.Range(-185.0f, 185.0f));
                }

                Instantiate(UFO, startPosition,
                    Quaternion.Euler(90.0f, 0, 0));
            }
        }
        else
        {
            // After wave 15 we spawn 15 big asteroids and a rundem number of random sized extras
            asteroidsRemaining = 15;
            int maxCount;
            Vector3 startPosition;
            for (int i = 0; i < asteroidsRemaining; i++)
            {
                // Spawn an Asteroid
                maxCount = 0;
                startPosition = new Vector3(Random.Range(-530.0f, 530.0f),
                        0, Random.Range(-320.0f, 320.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, ship.transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    startPosition = new Vector3(Random.Range(-550.0f, 550.0f),
                        0, Random.Range(-330.0f, 330.0f));
                }

                Instantiate(bigAsteroid, startPosition,
                    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));

            }

            // Extra asteroids
            int extraNumber = Random.Range(1, 5);
            asteroidsRemaining += extraNumber;
            for (int i = 0; i < extraNumber; i++)
            {
                // Spawn an Asteroid

                //Pick random asteroid size
                float randomValue = Random.Range(0.0f, 1.0f);
                GameObject asteroid;
                bool ufo = false;
                if (randomValue < 0.4)
                {
                    asteroid = smallAsteroid;
                }
                else if (randomValue < 0.8)
                {
                    asteroid = mediumAsteroid;
                }
                else if(randomValue < 0.9)
                {
                    asteroid = UFO;
                    ufo = true;
                }else
                {
                    asteroid = bigAsteroid;
                }

                if (ufo)
                {
                    // UFO
                    maxCount = 0;
                    int[] sign = { -1, 1 };
                    int leftOrRight = Random.Range(0, 1);
                    startPosition = new Vector3(320 * sign[leftOrRight],
                            0, Random.Range(-185.0f, 185.0f));

                    while (maxCount < 1000)
                    {
                        float dist = Vector3.Distance(startPosition, ship.transform.position);
                        if (dist > 90.0f)
                        {
                            break;
                        }
                        maxCount++;

                        leftOrRight = Random.Range(0, 1);
                        startPosition = new Vector3(320 * sign[leftOrRight],
                            0, Random.Range(-185.0f, 185.0f));
                    }

                    Instantiate(UFO, startPosition,
                        Quaternion.Euler(90.0f, 0, 0));
                }
                else
                {
                    maxCount = 0;
                    startPosition = new Vector3(Random.Range(-530.0f, 530.0f),
                            0, Random.Range(-320.0f, 320.0f));

                    while (maxCount < 1000)
                    {
                        float dist = Vector3.Distance(startPosition, ship.transform.position);
                        if (dist > 90.0f)
                        {
                            break;
                        }
                        maxCount++;

                        startPosition = new Vector3(Random.Range(-550.0f, 550.0f),
                            0, Random.Range(-330.0f, 330.0f));
                    }

                    Instantiate(asteroid, startPosition,
                        Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                }
            }
        }


        waveText.text = "WAVE: " + wave;
    }

    public void IncrementScore()
    {
        score++;

        scoreText.text = "SCORE:" + score;

        if (score > hiscore)
        {
            hiscore = score;
            hiscoreText.text = "HISCORE: " + hiscore;

            // Save the new hiscore
            PlayerPrefs.SetInt("hiscore", hiscore);
        }

        // Has player destroyed all asteroids?
        if (asteroidsRemaining < 1)
        {
            // Start next wave
            wave++;
            SpawnAsteroids();

        }
    }

    public void DecrementLives()
    {
        lives--;
        livesText.text = "LIVES: " + lives;

        // Has player run out of lives?
        if (lives < 1)
        {
            // Restart the game
            BeginGame();
        }
    }

    public void DecrementAsteroids()
    {
        asteroidsRemaining--;
    }

    public void SplitAsteroid(int split_amount)
    {
        // Two extra asteroids
        // - big one
        // + 3 little ones
        // = 2
        asteroidsRemaining += split_amount;

    }

    void DestroyExistingAsteroids()
    {
        GameObject[] asteroids =
            GameObject.FindGameObjectsWithTag("BigAsteroid");

        foreach (GameObject current in asteroids)
        {
            GameObject.Destroy(current);
        }

        GameObject[] asteroids2 =
            GameObject.FindGameObjectsWithTag("SmallAsteroid");

        foreach (GameObject current in asteroids2)
        {
            GameObject.Destroy(current);
        }

        GameObject[] asteroids3 =
            GameObject.FindGameObjectsWithTag("MediumAsteroid");

        foreach (GameObject current in asteroids3)
        {
            GameObject.Destroy(current);
        }

        GameObject[] asteroids4 =
            GameObject.FindGameObjectsWithTag("UFO");

        foreach (GameObject current in asteroids4)
        {
            GameObject.Destroy(current);
        }
    }

}