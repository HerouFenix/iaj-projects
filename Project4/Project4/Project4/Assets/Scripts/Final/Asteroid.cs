using UnityEngine;
using System.Collections;

public class Asteroid : MonoBehaviour
{
    public AudioClip destroy;
    public GameObject smallAsteroid;
    public GameObject mediumAsteroid;

    public GameController gameController;

    // Use this for initialization
    void Start()
    {
        
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
        if (c.gameObject.tag.Equals("Bullet"))
        {

            // Destroy the bullet
            Destroy(c.gameObject);

            // If large asteroid spawn new ones
            if (tag.Equals("BigAsteroid") && false)
            {
                // Spawn medium asteroids
                var instance = Instantiate(mediumAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y + Random.Range(15f, 60.0f), 0));
                instance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
                instance.GetComponent<Asteroid>().gameController = this.gameController;
                instance.transform.SetParent(transform.parent);

                // Spawn medium asteroids
                instance = Instantiate(mediumAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y - Random.Range(15.0f, 60.0f), 0));
                instance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
                instance.GetComponent<Asteroid>().gameController = this.gameController;
                instance.transform.SetParent(transform.parent);

                gameController.SplitAsteroid(1); // +1

            }
            else if (tag.Equals("MediumAsteroid") && false)
            {
                float rot1 = Random.Range(15f, 60.0f);

                // Spawn small asteroids
                var instance = Instantiate(smallAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y + rot1, 0));
                instance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
                instance.GetComponent<Asteroid>().gameController = this.gameController;
                instance.transform.SetParent(transform.parent);

                float rot2 = Random.Range(rot1, rot1 + 60);

                // Spawn small asteroids
                instance = Instantiate(smallAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y + rot2, 0));
                instance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
                instance.GetComponent<Asteroid>().gameController = this.gameController;
                instance.transform.SetParent(transform.parent);

                // Spawn small asteroids
                instance = Instantiate(smallAsteroid,
                new Vector3(transform.position.x,
                    0, transform.position.z),
                Quaternion.Euler(0, transform.rotation.y - Random.Range(15.0f, 60.0f), 0));
                instance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
                instance.GetComponent<Asteroid>().gameController = this.gameController;
                instance.transform.SetParent(transform.parent);

                gameController.SplitAsteroid(2); // +2
            }
            else
            {
                // Just a small asteroid destroyed
                gameController.DecrementAsteroids();
            }

            // Play a sound
            AudioSource.PlayClipAtPoint(
                destroy, this.gameController.cam.transform.position);

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
}