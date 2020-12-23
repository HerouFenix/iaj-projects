using UnityEngine;
using System.Collections;

public class Asteroid : MonoBehaviour
{
    public int ID;

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

        transform.position = transform.position + transform.forward * 12.0f;

        // Push the asteroid in the direction it is facing
        GetComponent<Rigidbody>().
            AddForce(transform.forward * Random.Range(1400.0f, 1600.0f));
        
        // Give a random angular velocity/rotation
        GetComponent<Rigidbody>()
           .angularVelocity = new Vector3(0,Random.Range(-5.0f, 5.0f),0);

    }

    void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag.Equals("Bullet") && c.gameObject.GetComponent<Bullet>().ID == ID)
        {

            // Destroy the bullet
            Destroy(c.gameObject);

            // If large asteroid spawn new ones
            if (tag.Equals("BigAsteroid"))
            {
                // Spawn medium asteroids
                var instance = Instantiate(mediumAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y + Random.Range(15f, 60.0f), 0));
                instance.GetComponent<Asteroid>().ID = ID;

                // Spawn medium asteroids
                instance = Instantiate(mediumAsteroid,
                new Vector3(transform.position.x - 10.0f,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y - Random.Range(15.0f, 60.0f), 0));
                instance.GetComponent<Asteroid>().ID = ID;

                gameController.SplitAsteroid(1, ID); // +1

            }
            else if (tag.Equals("MediumAsteroid"))
            {
                float rot1 = Random.Range(15f, 60.0f);

                // Spawn small asteroids
                var instance = Instantiate(smallAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y + rot1, 0));
                instance.GetComponent<Asteroid>().ID = ID;

                float rot2 = Random.Range(rot1, rot1 + 60);

                // Spawn small asteroids
                instance = Instantiate(smallAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y + rot2, 0));
                instance.GetComponent<Asteroid>().ID = ID;

                // Spawn small asteroids
                instance = Instantiate(smallAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y - Random.Range(15.0f, 60.0f), 0));
                instance.GetComponent<Asteroid>().ID = ID;

                gameController.SplitAsteroid(2, ID); // +2
            }
            else
            {
                // Just a small asteroid destroyed
                gameController.DecrementAsteroids(ID);
            }

            // Play a sound
            AudioSource.PlayClipAtPoint(
                destroy, Camera.main.transform.position);

            // Add to the score
            gameController.IncrementScore(ID);

            // Destroy the current asteroid
            Destroy(gameObject);

        }

    }

    private void FixedUpdate()
    {
        transform.position.Set(transform.position.x, 0.0f, transform.position.z);
    }
}