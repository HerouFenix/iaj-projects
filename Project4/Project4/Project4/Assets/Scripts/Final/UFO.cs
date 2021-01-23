using UnityEngine;
using System.Collections;

public class UFO : MonoBehaviour
{
    public AudioClip destroy;
    public AudioClip shoot;
    private GameObject playerObj = null;

    public GameController gameController;

    int STEPS_BETWEEN_SHOTS = 200;

    int stepsUntilShoot;

    bool canShoot = false;


    public GameObject bullet;

    private void Awake()
    {
        stepsUntilShoot = Random.Range(0, 150);
    }

    // Use this for initialization
    void Start()
    {
        // Push the UFO in the direction it is facing
        GetComponent<Rigidbody>().
            AddForce(transform.right * 1500.0f);

        playerObj = gameController.ship;

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
            gameController.enemies.Remove(this.gameObject);

            // Destroy the current asteroid
            Destroy(gameObject);
        }

    }

    private void FixedUpdate()
    {
        transform.position.Set(transform.position.x, 0.0f, transform.position.z);

        if (!canShoot)
        {
            stepsUntilShoot--;

            if (stepsUntilShoot <= 0)
            {
                canShoot = true;
            }
        }

        // Shoot
        if (canShoot)
        {
            ShootBullet();
            canShoot = false;
            stepsUntilShoot = STEPS_BETWEEN_SHOTS;
        }
    }

    void ShootBullet()
    {
        Vector3 spawnPos = transform.position;

        Vector3 diff = playerObj.transform.position - transform.position;
        diff.Normalize();

        // Spawn a bullet
        var bulletInstance = Instantiate(bullet,
            spawnPos,
            Quaternion.Euler(0,0,0));
        bulletInstance.GetComponent<BulletUFO>().playerObj = this.gameController.ship;
        bulletInstance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
        bulletInstance.transform.SetParent(transform.parent);

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, this.gameController.cam.transform.position);
    }

}