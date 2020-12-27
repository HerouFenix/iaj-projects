using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Ship : Agent
{
    private StatsRecorder stats;
    public LayerMask BulletLayer;

    float rotationSpeed = 90.0f;
    float thrustForce = 25.0f;
    float MAX_VELOCITY = 75.0f;
    int STEPS_BETWEEN_SHOTS = 40;

    int stepsUntilShoot = 0;

    bool canShoot = true;

    public AudioClip crash;
    public AudioClip shoot;

    public GameObject bullet;

    public GameController gameController;

    private Rigidbody body;

    private void Awake()
    {

    }

    void Start()
    {
    }


    void FixedUpdate()
    {
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

    void OnTriggerEnter(Collider c)
    {

        // Anything except a bullet is an asteroid
        if (c.gameObject.tag != "Bullet" && c.gameObject.tag != "SpaceShip")
        {
            Destroy(c.gameObject);

            AudioSource.PlayClipAtPoint
                (crash, this.gameController.cam.transform.position);

            // Move the ship to the centre of the screen
            transform.position = transform.parent.position;

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
        var bulletInstance = Instantiate(bullet,
            spawnPos,
            transform.rotation);

        bulletInstance.GetComponent<EuclideanTorus>().cam = this.gameController.cam;
        bulletInstance.transform.SetParent(transform.parent);

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, this.gameController.cam.transform.position);


        canShoot = false;
        stepsUntilShoot = STEPS_BETWEEN_SHOTS;
    }

    public void IncrementScore()
    {
        AddReward(0.15f);
    }

    public void FinishEpisode()
    {
        stats.Add("Game/Score", this.gameController.score);
        stats.Add("Game/Wave", this.gameController.wave);

        EndEpisode();
    }

    // ML Agent Stuff

    public override void Initialize()
    {
        body = GetComponent<Rigidbody>();
        stats = Academy.Instance.StatsRecorder;
    }

    /*
    private bool checkRayCastHitID(RaycastHit hit)
    {
        if (hit.transform.gameObject.tag.Contains("Asteroid") && hit.transform.gameObject.GetComponent<TrainingAsteroid>().ID == ID)
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
    */

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.DrawRay(transform.position, transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(-15, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(-30, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(-60, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.forward * 200.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.forward * 200.0f);
        //
        //Debug.DrawRay(transform.position, transform.right * 100.0f);
        //Debug.DrawRay(transform.position, -transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, -transform.right * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.right * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * -transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * -transform.forward * 100.0f);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.right * 100.0f);

        // Front Ray //////////////////////
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 200.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, transform.forward * 200.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position,transform.forward * 200.0f);
        }

        /*
        // Front Sensors //////////////////////
        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-15, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-15, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-15, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-30, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-30, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-30, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-45, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-60, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-60, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-60, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-75, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-75, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-75, Vector3.up) * transform.forward * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(75, Vector3.up) * transform.forward, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(75, Vector3.up) * transform.forward * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(75, Vector3.up) * transform.forward * 175.0f);
        }

        // Back Sensors //////////////////////
        if (Physics.Raycast(transform.position, transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(15, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(30, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(45, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(60, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(75, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(75, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(75, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(90, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(90, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(90, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(105, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(105, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(105, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(120, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(120, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(120, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(135, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(135, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(135, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(150, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(150, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(150, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(165, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(165, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(165, Vector3.up) * transform.right * 175.0f);
        }

        if (Physics.Raycast(transform.position, Quaternion.AngleAxis(180, Vector3.up) * transform.right, out hit, 175.0f, ~BulletLayer))
        {
            sensor.AddObservation(true);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(180, Vector3.up) * transform.right * 175.0f, Color.red);
        }
        else
        {
            sensor.AddObservation(false);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(180, Vector3.up) * transform.right * 175.0f);
        }
        */

        //// Position
        //sensor.AddObservation(transform.position);
        //
        //// Rotation
        //sensor.AddObservation(transform.rotation.y);

        //// Velocity
        //sensor.AddObservation(body.velocity);

    }

    private void Move(float dir)
    {
        if (dir == 0.0f)
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


        //var continuousActions = actionBuffers.ContinuousActions;
        //Rotate(continuousActions[0]);
        //Move(continuousActions[1]);

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

        // Rotate
        //continuousActionsOut[0] = Input.GetAxis("Horizontal");

        // Move
        //continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public override void OnEpisodeBegin()
    {
        // Reset Environment
        base.OnEpisodeBegin();
    }


}