﻿using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{
    public int ID;

    public int score = 0;
    public int wave = 1;
    public int lives = 1;
    public int asteroidsRemaining = 0;

    float rotationSpeed = 90.0f;
    float thrustForce = 25.0f;
    float MAX_VELOCITY = 75.0f;
    float TIME_BETWEEN_SHOTS = 1.0f;

    float timeTillNextShot = 0.0f;

    bool canShoot = true;

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


    void FixedUpdate()
    {
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

        // Not thrusting
        if (Input.GetAxis("Vertical") == 0.0f)
        {
            // Decrease velocity overtime
            body.velocity = body.velocity * 0.995f;
        }

        // Rotate 
        transform.Rotate(0, Input.GetAxis("Horizontal") *
            rotationSpeed * Time.deltaTime, 0);
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
        if (Input.GetKeyDown(KeyCode.Space) && canShoot)
        {
            canShoot = false;
            ShootBullet();
            timeTillNextShot = TIME_BETWEEN_SHOTS;
        }
    }

    void OnTriggerEnter(Collider c)
    {

        // Anything except a bullet is an asteroid
        if (c.gameObject.tag != "Bullet" && c.gameObject.tag != "SpaceShip")
        {
            if (c.gameObject.tag == "UFO")
            {
                if (c.gameObject.GetComponent<UFO>().ID != ID)
                    return;
            }
            else if (c.gameObject.tag == "Bullet_UFO")
            {
                if (c.gameObject.GetComponent<BulletUFO>().ID != ID)
                    return;
            }
            else if (c.gameObject.GetComponent<Asteroid>().ID != ID)
            {
                return;
            }
            Destroy(c.gameObject);

            AudioSource.PlayClipAtPoint
                (crash, Camera.main.transform.position);

            // Move the ship to the centre of the screen
            transform.position = new Vector3(0, 0, 0);

            // Remove all velocity from the ship
            body.
                velocity = new Vector3(0, 0, 0);

            gameController.DecrementLives(ID);
        }
    }


    void ShootBullet()
    {
        Vector3 spawnPos = transform.position + transform.forward * 10.0f;

        // Spawn a bullet
        var bulletInstance = Instantiate(bullet,
            spawnPos,
            transform.rotation);

        bulletInstance.GetComponent<Bullet>().ID = ID;

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, Camera.main.transform.position);
    }

}