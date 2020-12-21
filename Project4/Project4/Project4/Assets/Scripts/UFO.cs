using UnityEngine;
using System.Collections;

public class UFO : MonoBehaviour
{

    public AudioClip destroy;
    public AudioClip shoot;
    private GameObject playerObj = null;

    private GameController gameController;

    float TIME_BETWEEN_SHOTS = 3.0f;

    float timeTillNextShot = 3.0f;

    bool canShoot = false;


    public GameObject bullet;

    // Use this for initialization
    void Start()
    {

        // Get a reference to the game controller object and the script
        GameObject gameControllerObject =
            GameObject.FindWithTag("GameController");

        playerObj = GameObject.FindWithTag("GameController");

        gameController =
            gameControllerObject.GetComponent<GameController>();

        // Push the UFO in the direction it is facing
        GetComponent<Rigidbody>().
            AddForce(transform.right * 1500.0f);

    }

    void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag.Equals("Bullet"))
        {

            // Destroy the bullet
            Destroy(c.gameObject);

            // Just a small asteroid destroyed
            gameController.DecrementAsteroids();


            // Play a sound
            AudioSource.PlayClipAtPoint(
                destroy, Camera.main.transform.position);

            // Add to the score
            gameController.IncrementScore();

            // Destroy the current asteroid
            Destroy(gameObject);

        }

    }

    private void FixedUpdate()
    {
        transform.position.Set(transform.position.x, 0.0f, transform.position.z);
    }

    private void Update()
    {
        if (!canShoot)
        {
            // Ellapsed time since last shot
            timeTillNextShot -= Time.deltaTime;

            if (timeTillNextShot <= 0.0f)
            {
                canShoot = true;
                timeTillNextShot = 0.0f;
            }
        }

        // Shoot
        if (canShoot)
        {
            canShoot = false;
            ShootBullet();
            timeTillNextShot = TIME_BETWEEN_SHOTS;
        }
    }


    void ShootBullet()
    {
        Vector3 spawnPos = transform.position;

        Vector3 diff = playerObj.transform.position - transform.position;
        diff.Normalize();

        // Spawn a bullet
        Instantiate(bullet,
            spawnPos,
            Quaternion.Euler(0,0,0));

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, Camera.main.transform.position);
    }

}