using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{

    float rotationSpeed = 75.0f;
    float thrustForce = 7.5f;
    float MAX_VELOCITY = 75.0f;

    float timeSinceLastShot = 2.0f;

    public AudioClip crash;
    public AudioClip shoot;

    public GameObject bullet;

    private GameController gameController;

    private Rigidbody body;
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Get a reference to the game controller object and the script
        GameObject gameControllerObject =
            GameObject.FindWithTag("GameController");

        gameController =
            gameControllerObject.GetComponent<GameController>();
    }


    void Update()
    {
        // Ellapsed time since last shot
        timeSinceLastShot += Time.deltaTime;

        // Move
        Vector3 extraForce = transform.forward * thrustForce * Input.GetAxis("Vertical");
        var forceDir = transform.InverseTransformDirection(extraForce).z;
        var localDir = transform.InverseTransformDirection(body.velocity).z;
        if ((forceDir > 0 && localDir > 0) || (forceDir < 0 && localDir < 0))
        {
            if (body.velocity.magnitude <= MAX_VELOCITY)
            {
                body.AddForce(extraForce);
            }
        }
        else
        {
            body.AddForce(extraForce);
        }

        // Rotate 
        transform.Rotate(0, Input.GetAxis("Horizontal") *
            rotationSpeed * Time.deltaTime, 0);

        // Shoot
        if (Input.GetKeyDown(KeyCode.Space) && timeSinceLastShot > 1.5f)
        {
            ShootBullet();
            timeSinceLastShot = 0.0f;
        }
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
            body.
                velocity = new Vector3(0, 0, 0);

            gameController.DecrementLives();
        }
    }

    
    void ShootBullet()
    {
        Vector3 spawnPos = transform.position + transform.forward * 10.0f;

        // Spawn a bullet
        Instantiate(bullet,
            spawnPos,
            transform.rotation);

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, Camera.main.transform.position);
    }
    
}