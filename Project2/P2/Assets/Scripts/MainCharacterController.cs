using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Movement.Arbitration;
using Assets.Scripts.IAJ.Unity.Movement;

public class MainCharacterController : MonoBehaviour
{

    public float X_WORLD_SIZE;
    public float Z_WORLD_SIZE;
    public float MAX_ACCELERATION;
    public float MAX_SPEED;

    public float MAX_LOOK_AHEAD;
    public float AVOID_MARGIN;

    public float COLLISION_RADIUS;
    public Vector3 spawnPosition;

    public GameObject movementText;
    public DynamicCharacter character;

    public bool finished = false;

    public List<Vector3> path;

    //private DynamicPatrol patrolMovement;
    private DynamicArrive arriveMovement;
    private DynamicSeek seekMovement;

    private int pathNodeCounter = 0;

    //early initialization
    void Awake()
    {
        // Creating a new Dynamic Character
        this.character = new DynamicCharacter(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        pathNodeCounter = 0;
        this.cullPath();

        InitializeMovement();
    }

    public void cullPath()
    {
        List<Vector3> removedPath = new List<Vector3>();

        Vector3 p0 = this.spawnPosition;
        //int removedCounter = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            //if(removedCounter >= 20)
            //{ // Used to avoid car going too far off course in long stretches after curves
            //    removedCounter = 0;
            //    continue;
            //}
            Vector3 p1 = path[i];
            Vector3 p2 = path[i + 1];

            Vector3 travelingDirection = (p2 - p0);
            travelingDirection.y = 0;
            travelingDirection = travelingDirection.normalized;

            Vector3 leftWhisker = MathHelper.Rotate2D(travelingDirection, -MathConstants.MATH_PI_6);
            leftWhisker = leftWhisker.normalized;
            Vector3 rightWhisker = MathHelper.Rotate2D(travelingDirection, MathConstants.MATH_PI_6);
            rightWhisker = rightWhisker.normalized;

            Vector3 checkPoint = p0;
            float distance = this.EuclidianDistance(p0, p2);
            int quotient = (int)(distance / MAX_LOOK_AHEAD);
            int remainder = (int)(distance % MAX_LOOK_AHEAD);

            bool collision = false;

            for (int intermediate = 0; intermediate < quotient; intermediate++)
            {
                checkPoint = checkPoint + travelingDirection.normalized * MAX_LOOK_AHEAD;

                if (Physics.Raycast(checkPoint, travelingDirection, MAX_LOOK_AHEAD) || Physics.Raycast(checkPoint, rightWhisker, MAX_LOOK_AHEAD / 2) || Physics.Raycast(checkPoint, leftWhisker, MAX_LOOK_AHEAD / 2))
                {
                    collision = true;
                    break;
                }
            }
            if (remainder != 0 && !collision) // Also check the point itself
            {
                checkPoint = p2;
                if (Physics.Raycast(checkPoint, travelingDirection, MAX_LOOK_AHEAD) || Physics.Raycast(checkPoint, rightWhisker, MAX_LOOK_AHEAD / 2) || Physics.Raycast(checkPoint, leftWhisker, MAX_LOOK_AHEAD / 2))
                {
                    collision = true;
                }
            }

            if (!collision)
            {
                removedPath.Add(p1);
            //    removedCounter++;
            }
            else
            {
                p0 = p1;
            //    removedCounter = 0;
            }

        }

        this.path = this.path.Except(removedPath).ToList();
    }

    public void InitializeMovement()
    {
        //this.seekMovement = new DynamicSeek
        //{
        //    Character = this.character.KinematicData,
        //    Target = new KinematicData { Position = path[pathNodeCounter] },
        //    MaxAcceleration = MAX_ACCELERATION,
        //};

        this.arriveMovement = new DynamicArrive
        {
            ArriveTarget = new KinematicData { Position = path[pathNodeCounter] },
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            SlowRadius = 6.5f + 0.015f * this.EuclidianDistance(this.character.KinematicData.Position, this.path[pathNodeCounter]),
            DebugColor = Color.yellow,
        };

        //this.character.Movement = this.seekMovement;
        this.character.Movement = this.arriveMovement;
    }

    public void Update()
    {
        this.UpdateMovingGameObject();
    }

    private void UpdateMovingGameObject()
    {
        if (this.character.Movement != null)
        {
            this.character.Update();
            
            if (!finished)
            {
                if (this.EuclidianDistance(this.character.KinematicData.Position, this.path[pathNodeCounter]) < 5)
                {
                    pathNodeCounter++;
                    if (pathNodeCounter >= this.path.Count) // If we arrived at our final target, stop
                    {
                        this.finished = true;
                        //this.character.Movement = null;
                    }
                    else
                    {
                        float totalDistance = this.EuclidianDistance(this.character.KinematicData.Position, this.path[pathNodeCounter]); // The distance were gonna travel
                        //this.seekMovement.Target = new KinematicData { Position = this.path[pathNodeCounter] };

                        this.arriveMovement.SlowRadius = 6.5f + 0.015f * totalDistance; // The larger the distance, the sooner we should start slowing down

                        if(pathNodeCounter == this.path.Count)
                        {
                            this.arriveMovement.StopRadius = 3.0f;
                        }

                        this.arriveMovement.ArriveTarget = new KinematicData { Position = this.path[pathNodeCounter] };

                    }
                }
            }
            
        }

    }

    private float EuclidianDistance(Vector3 pos1, Vector3 pos2)
    {
        float x = (pos2.x - pos1.x);
        float y = (pos2.z - pos1.z);

        float result = Mathf.Sqrt(x * x + y * y);
        return result;
    }
}
