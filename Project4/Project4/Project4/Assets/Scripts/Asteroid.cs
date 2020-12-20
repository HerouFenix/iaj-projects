using UnityEngine;
using System.Collections;

public class Asteroid : MonoBehaviour
{

    public AudioClip destroy;
    public GameObject smallAsteroid;
    public GameObject mediumAsteroid;

    private GameController gameController;

    // Use this for initialization
    void Start()
    {

        // Get a reference to the game controller object and the script
        GameObject gameControllerObject =
            GameObject.FindWithTag("GameController");

        gameController =
            gameControllerObject.GetComponent<GameController>();

        // Push the asteroid in the direction it is facing
        GetComponent<Rigidbody>().
            AddForce(transform.forward * Random.Range(1400.0f, 1600.0f));

        // Give a random angular velocity/rotation
        GetComponent<Rigidbody>()
           .angularVelocity = new Vector3(0,Random.Range(-5.0f, 5.0f),0);

    }

    void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag.Equals("Bullet"))
        {

            // Destroy the bullet
            Destroy(c.gameObject);

            // If large asteroid spawn new ones
            if (tag.Equals("BigAsteroid"))
            {
                // Spawn small asteroids
                Instantiate(mediumAsteroid,
                    new Vector3(transform.position.x - 10.0f,
                        0, transform.position.z - 10.0f),
                        Quaternion.Euler(0, 0, 90));

                // Spawn small asteroids
                Instantiate(mediumAsteroid,
                    new Vector3(transform.position.x - 10.0f,
                        0, transform.position.z + 10.0f),
                        Quaternion.Euler(0, 0, 90));

                gameController.SplitAsteroid(1); // +1

            }
            else
            {
                // Just a small asteroid destroyed
                gameController.DecrementAsteroids();
            }

            // Play a sound
            AudioSource.PlayClipAtPoint(
                destroy, Camera.main.transform.position);

            // Add to the score
            gameController.IncrementScore();

            // Destroy the current asteroid
            Destroy(gameObject);

        }

    }

    private void Update()
    {
        transform.position.Set(transform.position.x, 0.0f, transform.position.z);
    }
}