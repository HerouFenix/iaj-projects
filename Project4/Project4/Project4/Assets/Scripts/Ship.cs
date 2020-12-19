using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{

    float rotationSpeed = 75.0f;
    float thrustForce = 20f;

    public AudioClip crash;
    public AudioClip shoot;

    public GameObject bullet;

    private GameController gameController;

    void Start()
    {
        // Get a reference to the game controller object and the script
        GameObject gameControllerObject =
            GameObject.FindWithTag("GameController");

        gameController =
            gameControllerObject.GetComponent<GameController>();
    }

    void FixedUpdate()
    {

        // Rotate the ship if necessary
        transform.Rotate(0, -Input.GetAxis("Horizontal") *
            rotationSpeed * Time.deltaTime, 0);

        // Thrust the ship if necessary
        GetComponent<Rigidbody>().
            AddForce(transform.forward * thrustForce *
                Input.GetAxis("Vertical"));

        // Has a bullet been fired
        /*
        if (Input.GetMouseButtonDown(0))
            ShootBullet();
        */

    }

    void OnTriggerEnter(Collider c)
    {

        // Anything except a bullet is an asteroid
        if (c.gameObject.tag != "Bullet")
        {

            AudioSource.PlayClipAtPoint
                (crash, Camera.main.transform.position);

            // Move the ship to the centre of the screen
            transform.position = new Vector3(0, 0, 0);

            // Remove all velocity from the ship
            GetComponent<Rigidbody2D>().
                velocity = new Vector3(0, 0, 0);

            gameController.DecrementLives();
        }
    }

    /*
    void ShootBullet()
    {

        // Spawn a bullet
        Instantiate(bullet,
            new Vector3(transform.position.x, transform.position.y, 0),
            transform.rotation);

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, Camera.main.transform.position);
    }
    */
}