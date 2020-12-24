using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Ship : Agent
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
    int STEPS_BETWEEN_SHOTS = 40;

    float timeTillNextShot = 0.0f;
    int stepsUntilShoot = 0;

    bool canShoot = true;

    public AudioClip crash;
    public AudioClip shoot;

    public GameObject bullet;

    private GameController gameController;

    private Rigidbody body;

    private void Awake()
    {

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
        //Debug.DrawRay(transform.position, transform.forward*100.0f);
        //Debug.DrawRay(transform.position, transform.right * 100.0f);
        //Debug.DrawRay(transform.position, -transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, -transform.right * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.right * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * -transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * -transform.right * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.right * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * -transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * -transform.right * 100.0f);

        // Move
        //Vector3 extraForce = transform.forward * thrustForce * Input.GetAxis("Vertical");
        //var forceDir = transform.InverseTransformDirection(extraForce).z;
        //var localDir = transform.InverseTransformDirection(body.velocity).z;
        //if ((forceDir > 0 && localDir > 0) || (forceDir < 0 && localDir < 0))
        //{
        //    if (body.velocity.magnitude <= MAX_VELOCITY)
        //    {
        //        body.AddForce(extraForce);
        //    }
        //}
        //else
        //{
        //    body.AddForce(extraForce);
        //}

        // Not thrusting
        //if (Input.GetAxis("Vertical") == 0.0f)
        //{
        //    // Decrease velocity overtime
        //    body.velocity = body.velocity * 0.995f;
        //}

        // Rotate 
        //transform.Rotate(0, Input.GetAxis("Horizontal") *
        //    rotationSpeed * Time.deltaTime, 0);

        if (!canShoot)
        {
            stepsUntilShoot--;

            if (stepsUntilShoot <= 0)
            {
                canShoot = true;
            }
        }

        // Shoot
        //if (Input.GetKey(KeyCode.Space) && canShoot)
        //{
        //    ShootBullet();
        //}
    }

    /*
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
            ShootBullet();
        }
    }
    */

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

            AddReward(-1);
            EndEpisode();
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

        canShoot = false;
        stepsUntilShoot = STEPS_BETWEEN_SHOTS;
    }


    // ML Agent Stuff

    public override void Initialize()
    {
        body = GetComponent<Rigidbody>();
    }

    private bool checkRayCastHitID(RaycastHit hit)
    {
        if (hit.transform.gameObject.tag.Contains("Asteroid") && hit.transform.gameObject.GetComponent<Asteroid>().ID == ID)
        {
            return true;
        }

        if (hit.transform.gameObject.tag.Equals("UFO") && hit.transform.gameObject.GetComponent<UFO>().ID == ID)
        {
            return true;
        }

        if (hit.transform.gameObject.tag.Equals("Bullet_UFO") && hit.transform.gameObject.GetComponent<BulletUFO>().ID == ID)
        {
            return true;
        }

        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // In Shooting Range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 100.0f) && checkRayCastHitID(hit))
        {
            sensor.AddObservation(true);
        }
        else
        {
            sensor.AddObservation(false);
        }

        // In Danger
        if ((Physics.Raycast(transform.position, transform.forward, out hit, 75.0f) && checkRayCastHitID(hit)) || (Physics.Raycast(transform.position, transform.right, out hit, 75.0f) && checkRayCastHitID(hit)) || (Physics.Raycast(transform.position, -transform.forward, out hit, 75.0f) && checkRayCastHitID(hit)) || (Physics.Raycast(transform.position, -transform.right, out hit, 75.0f) && checkRayCastHitID(hit))
            || (Physics.Raycast(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.forward, out hit, 75.0f) && checkRayCastHitID(hit)) || (Physics.Raycast(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.right, out hit, 75.0f) && checkRayCastHitID(hit)) || (Physics.Raycast(transform.position, Quaternion.AngleAxis(45, Vector3.up) * -transform.forward, out hit, 75.0f) && checkRayCastHitID(hit)) || (Physics.Raycast(transform.position, Quaternion.AngleAxis(45, Vector3.up) * -transform.right, out hit, 75.0f) && checkRayCastHitID(hit)))
        {
            sensor.AddObservation(true);
        }
        else
        {
            sensor.AddObservation(false);
        }

        // Can Shoot
        sensor.AddObservation(canShoot);

        // Position
        sensor.AddObservation(transform.position);

        // Rotation
        sensor.AddObservation(transform.rotation);

        // Velocity
        sensor.AddObservation(body.velocity);

        // Angular Velocity
        sensor.AddObservation(body.angularVelocity);
    }

    private void Move(float dir)
    {
        if(dir == 0.0f)
        {
            // Decrease velocity overtime
                body.velocity = body.velocity * 0.995f;
        }
        else
        {
            Vector3 extraForce = transform.forward * thrustForce * dir;
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
        }
    }

    private void Rotate(float dir)
    {
        transform.Rotate(0, dir *
           rotationSpeed * Time.deltaTime, 0);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var discreteActions = actionBuffers.DiscreteActions;
        if (discreteActions[0] == 1 && canShoot)
            ShootBullet();


        var continuousActions = actionBuffers.ContinuousActions;
        Move(continuousActions[0]);

        Rotate(continuousActions[1]);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        // Shoot
        if (Input.GetKey(KeyCode.Space) && canShoot)
        {
            discreteActionsOut[0] = 1;
        }

        var continuousActionsOut = actionsOut.ContinuousActions;

        // Move
        continuousActionsOut[0] = Input.GetAxis("Vertical");

        // Rotate
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    public override void OnEpisodeBegin()
    {
        // Reset Environment
        base.OnEpisodeBegin();
    }


}