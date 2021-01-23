using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Ship : Agent
{
    private StatsRecorder stats;
    public LayerMask BulletLayer;
    public bool justMove = false;

    float rotationSpeed = 90.0f;
    float thrustForce = 25.0f;
    float MAX_VELOCITY = 75.0f;
    int STEPS_BETWEEN_SHOTS = 40;

    int stepsUntilShoot = 0;

    bool canShoot = true;

    int stepCounter = 0;

    int MAX_STEPS = 5000;

    public AudioClip crash;
    public AudioClip shoot;

    public GameObject bullet;

    public GameController gameController;

    private Rigidbody body;

    public bool getNumberOfAsteroids = false;

    private void Awake()
    {

    }

    void Start()
    {
    }


    void FixedUpdate()
    {
        //stepCounter++;
        //if (stepCounter > MAX_STEPS)
        //{
        //    if (!justMove)
        //        gameController.DecrementLives(false);
        //    else
        //    {
        //        SetReward(1.0f);
        //        FinishEpisode();
        //        gameController.SpawnAsteroids();
        //    }
        //
        //}

        if (!canShoot)
        {
            stepsUntilShoot--;

            if (stepsUntilShoot <= 0)
            {
                canShoot = true;
            }
        }

        // Small penalty at each step
        //this.AddReward(-0.00008f);
        //this.AddReward(-0.0001f);
        //this.AddReward(-0.00002f);
        if (!justMove)
            this.AddReward(-0.00125f);
    }

    void OnTriggerEnter(Collider c)
    {

        // Anything except a bullet is an asteroid
        if (c.gameObject.tag != "Bullet" && c.gameObject.tag != "SpaceShip")
        {
            Destroy(c.gameObject);

            AudioSource.PlayClipAtPoint
                (crash, this.gameController.cam.transform.position);

            this.ResetShip();

            gameController.DecrementLives();
        }
    }

    public void ResetShip()
    {
        // Remove all velocity from the ship
        body.velocity = new Vector3(0, 0, 0);
        body.angularVelocity = new Vector3(0, 0, 0);

        // Move the ship to the centre of the screen
        transform.position = transform.parent.position;

        // Reset Rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);
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
        bulletInstance.GetComponent<Bullet>().ship = this.gameObject;

        // Play a shoot sound
        AudioSource.PlayClipAtPoint(shoot, this.gameController.cam.transform.position);

        AddReward(-0.05f);

        canShoot = false;
        stepsUntilShoot = STEPS_BETWEEN_SHOTS;
    }

    public void IncrementScore(float score)
    {
        AddReward(score);
    }

    public void FinishEpisode()
    {
        stats.Add("Game/Score", this.gameController.score);
        stats.Add("Game/Wave", this.gameController.wave);
        //Debug.Log("Finished Episode");
        //Debug.Log(GetCumulativeReward());
        EndEpisode();
    }

    // ML Agent Stuff

    public override void Initialize()
    {
        body = GetComponent<Rigidbody>();
        stats = Academy.Instance.StatsRecorder;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Shot ready
        sensor.AddObservation(canShoot);

        // Rotation
        sensor.AddObservation(transform.forward);

        // Velocity
        sensor.AddObservation(body.velocity);

        // Position
        sensor.AddObservation(transform.position);

        // Closest Enemy Position + Direction + Distance
        GameObject closestEnemy = null;
        float closestDistance = 10000000.0f;

        foreach (GameObject enemy in gameController.enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = enemy;
            }
        }

        Vector3 directionToEnemy = (closestEnemy.transform.position - this.transform.position).normalized;

        //float dotProd = Vector3.Dot(directionToEnemy, transform.forward);
        //
        //if (dotProd > 0.9)
        //{
        //    sensor.AddObservation(true); // Ship facing enemy
        //}
        //else
        //{
        //    sensor.AddObservation(false); // Ship not facing enemy
        //}

        sensor.AddObservation(closestEnemy.transform.position);
        sensor.AddObservation(closestEnemy.transform.GetComponent<Rigidbody>().velocity); // Enemy Velocity4
        //Debug.Log(Vector3.SignedAngle(transform.forward, directionToEnemy, Vector3.up));
        sensor.AddObservation(Vector3.SignedAngle(transform.forward, directionToEnemy, Vector3.up)); // Angle difference between forward and enemy direction
        sensor.AddObservation(closestDistance);

        if(getNumberOfAsteroids)
            sensor.AddObservation(gameController.totalAsteroidsRemaining); //So it knows how many enemies in total are left
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

        // During lesson zero we just wanna teach the agent how to evade, so no shooting :)
        if (discreteActions[0] == 1 && canShoot && !justMove)
            ShootBullet();


        var continuousActions = actionBuffers.ContinuousActions;
        Rotate(continuousActions[0]);
        Move(continuousActions[1]);

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
        continuousActionsOut[0] = Input.GetAxis("Horizontal");

        // Move
        continuousActionsOut[1] = Input.GetAxis("Vertical");

    }

    public override void OnEpisodeBegin()
    {
        this.ResetShip();

        // Reset timeout
        //stepCounter = 0;

        //if (justMove)
        //    MAX_STEPS = 1250;
        //else
        //    MAX_STEPS = 800;

        base.OnEpisodeBegin();

        //Debug.Log("Starting Episode");
    }


}