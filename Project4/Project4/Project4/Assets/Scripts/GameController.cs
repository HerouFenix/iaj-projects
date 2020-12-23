using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{

    public GameObject bigAsteroid;
    public GameObject mediumAsteroid;
    public GameObject smallAsteroid;
    public GameObject UFO;
    public GameObject ship;

    private int currentID = 0;

    private Dictionary<int, int[]> gameInstances = new Dictionary<int, int[]>();
    private Dictionary<int, GameObject> shipInstances = new Dictionary<int, GameObject>();

    // Order: Score, Lives, Wave, AsteroidsRemaining

    private int score;
    private int hiscore;
    private int lives;
    private int wave;

    private int increaseEachWave = 1;

    public Text scoreText;
    public Text livesText;
    public Text hiscoreText;
    public Text waveText;

    public bool disableWaveIncrease = false;
    public bool disableAliens = false;

    public int numberOfAgents = 1;

    // Use this for initialization
    void Start()
    {
        hiscore = PlayerPrefs.GetInt("hiscore", 0);
        hiscoreText.text = "HISCORE: " + hiscore;

        for (int i = 0; i < numberOfAgents; i++)
        {
            BeginGame(currentID++, true);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Quit if player presses escape
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        // Increase Wave
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            gameInstances[0][2]++;
            this.SpawnAsteroids(0);
        }
        // Decrease Wave
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            gameInstances[0][2]--;
            this.SpawnAsteroids(0);
        }

    }

    void BeginGame(int ID, bool spawn = false)
    {
        GameObject shipInstance;
        if (spawn)
        {

            shipInstance = Instantiate(ship, new Vector3(ID, 0, 0),
                    Quaternion.Euler(0, 0, 0));
            var color = Random.ColorHSV();
            shipInstance.GetComponent<Renderer>().materials[2].SetColor("_Color", color);

            shipInstance.GetComponent<Ship>().ID = ID;

            gameInstances.Add(ID, new int[4] { 0, 1, 1, 0 });
            shipInstances.Add(ID, shipInstance);
        }
        else
        {
            gameInstances[ID] = new int[4] { 0, 1, 1, 0 };
            shipInstance = shipInstances[ID];
        }

        shipInstance.GetComponent<Ship>().score = 0;
        shipInstance.GetComponent<Ship>().wave = 1;
        shipInstance.GetComponent<Ship>().asteroidsRemaining = 0;
        shipInstance.GetComponent<Ship>().lives = 1;

        // Prepare the HUD
        if (ID == 0)
        {
            scoreText.text = "SCORE:" + gameInstances[ID][0];
            livesText.text = "LIVES: " + gameInstances[ID][1];
            waveText.text = "WAVE: " + gameInstances[ID][2];
        }

        SpawnAsteroids(ID);
    }

    void SpawnAsteroids(int ID)
    {

        DestroyExistingAsteroids(ID);

        // Decide how many asteroids to spawn
        // If any asteroids left over from previous game, subtract them
        //asteroidsRemaining = 1;
        Camera cam = Camera.main;

        if (gameInstances[ID][2] < 10 || disableWaveIncrease)
        {
            if (disableWaveIncrease)
            {
                gameInstances[ID][3] = 1;
            }
            else
            {
                gameInstances[ID][3] = (gameInstances[ID][2] * increaseEachWave);
            }

            for (int i = 0; i < gameInstances[ID][3]; i++)
            {
                // Spawn an Asteroid
                int maxCount = 0;
                Vector3 startPosition = new Vector3(Random.Range(-530.0f, 530.0f),
                        0, Random.Range(-320.0f, 320.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, shipInstances[ID].transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    startPosition = new Vector3(Random.Range(-550.0f, 550.0f),
                        0, Random.Range(-330.0f, 330.0f));
                }

                var instance = Instantiate(bigAsteroid, startPosition,
                    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                instance.GetComponent<Asteroid>().ID = ID;

            }

            if (gameInstances[ID][2] > 4 && !disableAliens)
            {
                // After wave 5 spawn a UFO
                gameInstances[ID][3] += 1;
                int maxCount = 0;
                int[] sign = { -1, 1 };
                int leftOrRight = Random.Range(0, 1);
                Vector3 startPosition = new Vector3(320 * sign[leftOrRight],
                        0, Random.Range(-185.0f, 185.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, shipInstances[ID].transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    leftOrRight = Random.Range(0, 1);
                    startPosition = new Vector3(320 * sign[leftOrRight],
                        0, Random.Range(-185.0f, 185.0f));
                }

                var instance = Instantiate(UFO, startPosition,
                    Quaternion.Euler(90.0f, 0, 0));
                instance.GetComponent<UFO>().ID = ID;
            }
        }
        else
        {
            // After wave 15 we spawn 15 big asteroids and a rundem number of random sized extras
            gameInstances[ID][3] = 15;
            int maxCount;
            Vector3 startPosition;
            for (int i = 0; i < gameInstances[ID][3]; i++)
            {
                // Spawn an Asteroid
                maxCount = 0;
                startPosition = new Vector3(Random.Range(-530.0f, 530.0f),
                        0, Random.Range(-320.0f, 320.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, shipInstances[ID].transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    startPosition = new Vector3(Random.Range(-550.0f, 550.0f),
                        0, Random.Range(-330.0f, 330.0f));
                }

                var instance = Instantiate(bigAsteroid, startPosition,
                    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                instance.GetComponent<Asteroid>().ID = ID;

            }

            // Extra asteroids
            int extraNumber = Random.Range(1, 5);
            gameInstances[ID][3] += extraNumber;
            for (int i = 0; i < extraNumber; i++)
            {
                // Spawn an Asteroid

                //Pick random asteroid size
                float randomValue = Random.Range(0.0f, 1.0f);
                GameObject asteroid;
                bool ufo = false;
                if (randomValue < 0.3)
                {
                    asteroid = smallAsteroid;
                }
                else if (randomValue < 0.7)
                {
                    asteroid = mediumAsteroid;
                }
                else if (randomValue < 0.9)
                {
                    if (disableAliens)
                    {
                        asteroid = mediumAsteroid;
                    }
                    else
                    {
                        asteroid = UFO;
                        ufo = true;
                    }
                }
                else
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
                        float dist = Vector3.Distance(startPosition, shipInstances[ID].transform.position);
                        if (dist > 90.0f)
                        {
                            break;
                        }
                        maxCount++;

                        leftOrRight = Random.Range(0, 1);
                        startPosition = new Vector3(320 * sign[leftOrRight],
                            0, Random.Range(-185.0f, 185.0f));
                    }

                    var instance = Instantiate(UFO, startPosition,
                        Quaternion.Euler(90.0f, 0, 0));
                    instance.GetComponent<UFO>().ID = ID;
                }
                else
                {
                    maxCount = 0;
                    startPosition = new Vector3(Random.Range(-530.0f, 530.0f),
                            0, Random.Range(-320.0f, 320.0f));

                    while (maxCount < 1000)
                    {
                        float dist = Vector3.Distance(startPosition, shipInstances[ID].transform.position);
                        if (dist > 90.0f)
                        {
                            break;
                        }
                        maxCount++;

                        startPosition = new Vector3(Random.Range(-550.0f, 550.0f),
                            0, Random.Range(-330.0f, 330.0f));
                    }

                    var instance = Instantiate(asteroid, startPosition,
                        Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                    instance.GetComponent<Asteroid>().ID = ID;
                }
            }
        }
        shipInstances[ID].GetComponent<Ship>().asteroidsRemaining = gameInstances[ID][3];
        if (ID == 0)
        {
            waveText.text = "WAVE: " + gameInstances[ID][2];
        }
    }

    public void IncrementScore(int ID)
    {
        gameInstances[ID][0]++;
        shipInstances[ID].GetComponent<Ship>().score = gameInstances[ID][0];

        if (ID == 0)
        {
            scoreText.text = "SCORE:" + gameInstances[ID][0];
        }

        if (gameInstances[ID][0] > hiscore)
        {
            hiscore = score;
            hiscoreText.text = "HISCORE: " + hiscore;

            // Save the new hiscore
            PlayerPrefs.SetInt("hiscore", hiscore);
        }

        // Has player destroyed all asteroids?
        if (gameInstances[ID][3] < 1)
        {
            // Start next wave
            gameInstances[ID][2]++;
            shipInstances[ID].GetComponent<Ship>().wave = gameInstances[ID][2];
            SpawnAsteroids(ID);

        }
    }

    public void DecrementLives(int ID)
    {
        gameInstances[ID][1]--;
        shipInstances[ID].GetComponent<Ship>().lives = gameInstances[ID][1];
        if (ID == 0)
        {
            livesText.text = "LIVES: " + lives;
        }

        // Has player run out of lives?
        if (gameInstances[ID][1] < 1)
        {
            // Restart the game
            BeginGame(ID);
        }
    }

    public void DecrementAsteroids(int ID)
    {
        gameInstances[ID][3]--;
        shipInstances[ID].GetComponent<Ship>().asteroidsRemaining = gameInstances[ID][3];
    }

    public void SplitAsteroid(int split_amount, int ID)
    {
        // Two extra asteroids
        // - big one
        // + 3 little ones
        // = 2
        gameInstances[ID][3] += split_amount;
        shipInstances[ID].GetComponent<Ship>().asteroidsRemaining = gameInstances[ID][3];

    }

    void DestroyExistingAsteroids(int ID)
    {
        GameObject[] asteroids =
            GameObject.FindGameObjectsWithTag("BigAsteroid");

        foreach (GameObject current in asteroids)
        {
            if (current.GetComponent<Asteroid>().ID == ID)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids2 =
            GameObject.FindGameObjectsWithTag("SmallAsteroid");

        foreach (GameObject current in asteroids2)
        {
            if (current.GetComponent<Asteroid>().ID == ID)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids3 =
            GameObject.FindGameObjectsWithTag("MediumAsteroid");

        foreach (GameObject current in asteroids3)
        {
            if (current.GetComponent<Asteroid>().ID == ID)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids4 =
            GameObject.FindGameObjectsWithTag("UFO");

        foreach (GameObject current in asteroids4)
        {
            if (current.GetComponent<UFO>().ID == ID)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids5 =
            GameObject.FindGameObjectsWithTag("Bullet");

        foreach (GameObject current in asteroids5)
        {
            if (current.GetComponent<Bullet>().ID == ID)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids6 =
            GameObject.FindGameObjectsWithTag("Bullet_UFO");

        foreach (GameObject current in asteroids6)
        {
            if (current.GetComponent<BulletUFO>().ID == ID)
                GameObject.Destroy(current);
        }
    }

}