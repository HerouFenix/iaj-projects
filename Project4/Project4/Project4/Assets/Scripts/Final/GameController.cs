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
    public Camera cam;

    public int score;
    public int hiscore;
    public int asteroidsRemaining;
    public int lives;
    public int wave;
    private int increaseEachWave = 1;

    private int totalAsteroidsInWave = 0;
    public int totalAsteroidsRemaining = 0;

    public Text scoreText;
    public Text livesText;
    public Text hiscoreText;
    public Text waveText;

    public bool disableWaveIncrease = false;
    public bool disableAliens = false;
    public bool infiniteLives = false;
    public bool unbreakableAsteroidsOnly = false;
    public int startWave = 1;

    public bool training = true;

    public List<GameObject> enemies;

    bool firstReset = true;

    // Use this for initialization
    void Start()
    {
        enemies = new List<GameObject>();
        hiscore = PlayerPrefs.GetInt("hiscore", 0);

        /*
        Unity.MLAgents.Academy.Instance.OnEnvironmentReset += delegate ()
        {
            if (firstReset)
            {
                firstReset = !firstReset;
                return;
            }

            if (enemies.Count > 0)
            {
                totalAsteroidsInWave = 0;
                enemies.Clear();
                ship.GetComponent<Ship>().FinishEpisode();

                // Restart the game
                BeginGame();
            }
        };
        */

        BeginGame();
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
            wave++;
            this.SpawnAsteroids();
        }
        // Decrease Wave
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            wave--;
            this.SpawnAsteroids();
        }
    }

    public void BeginGame()
    {

        score = 0;
        lives = 1;

        wave = startWave;
        if (disableWaveIncrease)
        {
            increaseEachWave = wave;
        }

        // Prepare the HUD
        scoreText.text = "SCORE:" + score;
        hiscoreText.text = "HISCORE: " + hiscore;
        livesText.text = "LIVES: " + lives;
        waveText.text = "WAVE: " + wave;

        SpawnAsteroids();
    }

    public void SpawnAsteroids()
    {
        DestroyExistingAsteroids();

        // Decide how many asteroids to spawn
        // If any asteroids left over from previous game, subtract them

        if (training)
        {
            var EnvironmentParameters = Unity.MLAgents.Academy.Instance.EnvironmentParameters;
            int bigAsteroids = (int)EnvironmentParameters.GetWithDefault("BigAsteroids", 0);
            int ufos = (int)EnvironmentParameters.GetWithDefault("UFOs", 0);
            int unbreakableAsteroids = (int)EnvironmentParameters.GetWithDefault("UnbreakableAsteroids", 6);
            unbreakableAsteroids = 1;
            if (unbreakableAsteroids == 10)
            {
                ship.GetComponent<Ship>().justMove = true;
            }
            else
            {
                ship.GetComponent<Ship>().justMove = false;
            }

            asteroidsRemaining = bigAsteroids + unbreakableAsteroids;

            for (int i = 0; i < bigAsteroids; i++)
            {
                // Spawn an Asteroid
                //Vector3 startPosition = new Vector3(this.transform.position.x,
                //        0, this.transform.position.z+100.0f);


                int maxCount = 0;
                Vector3 startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                        0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));

                while (maxCount < 100000000)
                {
                    float dist = Vector3.Distance(startPosition, ship.transform.position);
                    if (dist > 75.0f)
                    {
                        break;
                    }
                    maxCount++;

                    startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                        0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));
                }


                //var instance = Instantiate(bigAsteroid, startPosition,
                //    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                var instance = Instantiate(bigAsteroid, startPosition,
                    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                instance.GetComponent<EuclideanTorus>().cam = cam;
                instance.GetComponent<Asteroid>().gameController = this;
                instance.transform.SetParent(transform.parent);

                enemies.Add(instance);
                totalAsteroidsInWave += 9; // 1 Big + 2 Medium + 6 Small
            }

            for (int i = 0; i < unbreakableAsteroids; i++)
            {
                // Spawn an Asteroid
                //Vector3 startPosition = new Vector3(this.transform.position.x,
                //        0, this.transform.position.z+100.0f);


                int maxCount = 0;
                Vector3 startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                        0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));

                while (maxCount < 100000000)
                {
                    float dist = Vector3.Distance(startPosition, ship.transform.position);
                    if (dist > 75.0f)
                    {
                        break;
                    }
                    maxCount++;

                    startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                        0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));
                }


                //var instance = Instantiate(bigAsteroid, startPosition,
                //    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                var instance = Instantiate(bigAsteroid, startPosition,
                    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                instance.GetComponent<EuclideanTorus>().cam = cam;
                instance.GetComponent<Asteroid>().gameController = this;
                instance.GetComponent<Asteroid>().breakable = false;
                instance.transform.SetParent(transform.parent);

                enemies.Add(instance);
                totalAsteroidsInWave += 1; // 1 NonBreakable
            }

            for (int i = 0; i < ufos; i++)
            {
                // After wave 5 spawn a UFO
                asteroidsRemaining += 1;
                int maxCount = 0;
                int[] sign = { -1, 1 };
                int leftOrRight = Random.Range(0, 1);
                Vector3 startPosition = new Vector3(this.transform.position.x + 320 * sign[leftOrRight],
                        0, Random.Range(this.transform.position.z - 185.0f, this.transform.position.z + 185.0f));

                while (maxCount < 1000)
                {
                    float dist = Vector3.Distance(startPosition, ship.transform.position);
                    if (dist > 90.0f)
                    {
                        break;
                    }
                    maxCount++;

                    leftOrRight = Random.Range(0, 1);
                    startPosition = new Vector3(this.transform.position.x + 320 * sign[leftOrRight],
                        0, Random.Range(this.transform.position.z - 185.0f, this.transform.position.z + 185.0f));
                }

                var instance = Instantiate(UFO, startPosition,
                    Quaternion.Euler(90.0f, 0, 0));
                instance.GetComponent<EuclideanTorus>().cam = cam;
                instance.GetComponent<UFO>().gameController = this;
                instance.transform.SetParent(transform.parent);
                enemies.Add(instance);

                totalAsteroidsInWave++;
            }
        }
        else
        {
            if (wave < 10 || disableWaveIncrease)
            {
                if (disableWaveIncrease)
                {
                    asteroidsRemaining = increaseEachWave;
                }
                else
                {
                    asteroidsRemaining = (wave * increaseEachWave);
                }

                for (int i = 0; i < asteroidsRemaining; i++)
                {
                    // Spawn an Asteroid
                    //Vector3 startPosition = new Vector3(this.transform.position.x,
                    //        0, this.transform.position.z+100.0f);


                    int maxCount = 0;
                    Vector3 startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                            0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));

                    while (maxCount < 100000000)
                    {
                        float dist = Vector3.Distance(startPosition, ship.transform.position);
                        if (dist > 75.0f)
                        {
                            break;
                        }
                        maxCount++;

                        startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                            0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));
                    }


                    //var instance = Instantiate(bigAsteroid, startPosition,
                    //    Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                    var instance = Instantiate(bigAsteroid, startPosition,
                        Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                    instance.GetComponent<EuclideanTorus>().cam = cam;
                    instance.GetComponent<Asteroid>().gameController = this;
                    instance.transform.SetParent(transform.parent);

                    if (unbreakableAsteroidsOnly)
                    {
                        instance.GetComponent<Asteroid>().breakable = false;
                    }

                    enemies.Add(instance);
                    totalAsteroidsInWave += 9; // 1 Big + 2 Medium + 6 Small
                }

                if (wave > 4 && !disableAliens)
                {
                    // After wave 5 spawn a UFO
                    asteroidsRemaining += 1;
                    int maxCount = 0;
                    int[] sign = { -1, 1 };
                    int leftOrRight = Random.Range(0, 1);
                    Vector3 startPosition = new Vector3(this.transform.position.x + 320 * sign[leftOrRight],
                            0, Random.Range(this.transform.position.z - 185.0f, this.transform.position.z + 185.0f));

                    while (maxCount < 1000)
                    {
                        float dist = Vector3.Distance(startPosition, ship.transform.position);
                        if (dist > 90.0f)
                        {
                            break;
                        }
                        maxCount++;

                        leftOrRight = Random.Range(0, 1);
                        startPosition = new Vector3(this.transform.position.x + 320 * sign[leftOrRight],
                            0, Random.Range(this.transform.position.z - 185.0f, this.transform.position.z + 185.0f));
                    }

                    var instance = Instantiate(UFO, startPosition,
                        Quaternion.Euler(90.0f, 0, 0));
                    instance.GetComponent<EuclideanTorus>().cam = cam;
                    instance.GetComponent<UFO>().gameController = this;
                    instance.transform.SetParent(transform.parent);
                    enemies.Add(instance);
                    totalAsteroidsInWave++;
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
                    startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                            0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));

                    while (maxCount < 1000)
                    {
                        float dist = Vector3.Distance(startPosition, ship.transform.position);
                        if (dist > 90.0f)
                        {
                            break;
                        }
                        maxCount++;

                        startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                            0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));
                    }

                    var instance = Instantiate(bigAsteroid, startPosition,
                        Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                    instance.GetComponent<EuclideanTorus>().cam = cam;
                    instance.GetComponent<Asteroid>().gameController = this;
                    instance.transform.SetParent(transform.parent);
                    enemies.Add(instance);

                    if (unbreakableAsteroidsOnly)
                    {
                        instance.GetComponent<Asteroid>().breakable = false;
                    }

                    totalAsteroidsInWave += 9;
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
                    if (randomValue < 0.3)
                    {
                        asteroid = smallAsteroid;
                        totalAsteroidsInWave++;
                    }
                    else if (randomValue < 0.7)
                    {
                        asteroid = mediumAsteroid;
                        totalAsteroidsInWave += 4; // 1 Medium + 3 small
                    }
                    else if (randomValue < 0.9)
                    {
                        if (disableAliens)
                        {
                            asteroid = mediumAsteroid;
                            totalAsteroidsInWave += 4;
                        }
                        else
                        {
                            asteroid = UFO;
                            ufo = true;
                            totalAsteroidsInWave++;
                        }
                    }
                    else
                    {
                        asteroid = bigAsteroid;
                        totalAsteroidsInWave += 9;
                    }

                    if (ufo)
                    {
                        // UFO
                        maxCount = 0;
                        int[] sign = { -1, 1 };
                        int leftOrRight = Random.Range(0, 1);
                        startPosition = new Vector3(this.transform.position.x + 320 * sign[leftOrRight],
                            0, Random.Range(this.transform.position.z - 185.0f, this.transform.position.z + 185.0f));

                        while (maxCount < 1000)
                        {
                            float dist = Vector3.Distance(startPosition, ship.transform.position);
                            if (dist > 90.0f)
                            {
                                break;
                            }
                            maxCount++;

                            leftOrRight = Random.Range(0, 1);
                            startPosition = new Vector3(this.transform.position.x + 320 * sign[leftOrRight],
                            0, Random.Range(this.transform.position.z - 185.0f, this.transform.position.z + 185.0f));
                        }

                        var instance = Instantiate(UFO, startPosition,
                            Quaternion.Euler(90.0f, 0, 0));
                        instance.GetComponent<EuclideanTorus>().cam = cam;
                        instance.GetComponent<UFO>().gameController = this;
                        instance.transform.SetParent(transform.parent);
                        enemies.Add(instance);

                        totalAsteroidsInWave++;
                    }
                    else
                    {
                        maxCount = 0;
                        startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                            0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));

                        while (maxCount < 1000)
                        {
                            float dist = Vector3.Distance(startPosition, ship.transform.position);
                            if (dist > 90.0f)
                            {
                                break;
                            }
                            maxCount++;

                            startPosition = new Vector3(Random.Range(this.transform.position.x - 530.0f, this.transform.position.x + 530.0f),
                            0, Random.Range(this.transform.position.z - 320.0f, this.transform.position.z + 320.0f));
                        }

                        var instance = Instantiate(asteroid, startPosition,
                            Quaternion.Euler(0, Random.Range(-0.0f, 359.0f), 0));
                        instance.GetComponent<EuclideanTorus>().cam = cam;
                        instance.GetComponent<Asteroid>().gameController = this;
                        instance.transform.SetParent(transform.parent);
                        enemies.Add(instance);

                        if (unbreakableAsteroidsOnly)
                        {
                            instance.GetComponent<Asteroid>().breakable = false;
                        }
                    }
                }
            }
        }
        totalAsteroidsRemaining = totalAsteroidsInWave;
    }

    public void IncrementScore()
    {
        score++;
        ship.GetComponent<Ship>().IncrementScore(1.0f / totalAsteroidsInWave + 0.05f);
        //ship.GetComponent<Ship>().IncrementScore(0.1f); // Refund Bullet + a little snack

        totalAsteroidsRemaining--;
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
            waveText.text = "WAVE: " + wave;

            totalAsteroidsRemaining = 0;

            //if (ship.GetComponent<Ship>().GetCumulativeReward() + 1.0f > 1.0f)
            //{
            //    ship.GetComponent<Ship>().SetReward(1 - ship.GetComponent<Ship>().GetCumulativeReward()); // So that reward is capped at 1
            //}
            //else
            //{
            //ship.GetComponent<Ship>().SetReward(1.0f + 0.05f); // Finished the wave, set reward to 1.0
            //}

            totalAsteroidsInWave = 0;

            ship.GetComponent<Ship>().FinishEpisode();

            SpawnAsteroids();
        }
    }

    public void DecrementLives(bool died = true)
    {
        lives--;
        livesText.text = "LIVES: " + lives;

        if (died) // If game over due to timeout, don't give extra penalization
            ship.GetComponent<Ship>().SetReward(-1.0f);

        // Has player run out of lives?
        if (lives < 1)
        {
            totalAsteroidsInWave = 0;

            ship.GetComponent<Ship>().FinishEpisode();

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
            if (current.transform.parent == this.transform.parent)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids2 =
            GameObject.FindGameObjectsWithTag("SmallAsteroid");

        foreach (GameObject current in asteroids2)
        {
            if (current.transform.parent == this.transform.parent)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids3 =
            GameObject.FindGameObjectsWithTag("MediumAsteroid");

        foreach (GameObject current in asteroids3)
        {
            if (current.transform.parent == this.transform.parent)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids4 =
            GameObject.FindGameObjectsWithTag("UFO");

        foreach (GameObject current in asteroids4)
        {
            if (current.transform.parent == this.transform.parent)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids5 =
            GameObject.FindGameObjectsWithTag("Bullet");

        foreach (GameObject current in asteroids5)
        {
            if (current.transform.parent == this.transform.parent)
                GameObject.Destroy(current);
        }

        GameObject[] asteroids6 =
            GameObject.FindGameObjectsWithTag("Bullet_UFO");

        foreach (GameObject current in asteroids6)
        {
            if (current.transform.parent == this.transform.parent)
                GameObject.Destroy(current);
        }

        enemies.Clear();
    }

}
