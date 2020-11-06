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
    public float CULL_LOOK_ITERATION;
    public bool USE_PRIORITY;
    public float AVOID_MARGIN;

    public float COLLISION_RADIUS;
    public Vector3 spawnPosition;

    public GameObject movementText;
    public DynamicCharacter character;

    public bool finished = false;

    public List<Vector3> path;
    public List<int> middles = new List<int>();


    //private DynamicPatrol patrolMovement;
    private DynamicArrive arriveMovement;
    public PriorityMovement priorityMovement;
    private DynamicSeek seekMovement;

    public GameObject[] obstacles;

    private int pathNodeCounter = 0;

    public float cellSize;

    //early initialization
    void Awake()
    {
        // Creating a new Dynamic Character
        this.character = new DynamicCharacter(this.gameObject);
        this.priorityMovement = new PriorityMovement
        {
            Character = this.character.KinematicData
        };
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

            Vector3 leftWhisker = MathHelper.Rotate2D(travelingDirection, -MathConstants.MATH_PI_4);
            leftWhisker = leftWhisker.normalized;
            Vector3 rightWhisker = MathHelper.Rotate2D(travelingDirection, MathConstants.MATH_PI_4);
            rightWhisker = rightWhisker.normalized;

            Vector3 checkPoint = p0;
            float distance = this.EuclidianDistance(p0, p2);
            int quotient = (int)(distance / CULL_LOOK_ITERATION);
            int remainder = (int)(distance % CULL_LOOK_ITERATION);

            bool collision = false;

            for (int intermediate = 0; intermediate < quotient; intermediate++)
            {
                checkPoint = checkPoint + travelingDirection.normalized * CULL_LOOK_ITERATION;

                if (Physics.Raycast(checkPoint, travelingDirection, CULL_LOOK_ITERATION) || Physics.Raycast(checkPoint, rightWhisker, CULL_LOOK_ITERATION / 2.0f) || Physics.Raycast(checkPoint, leftWhisker, CULL_LOOK_ITERATION / 2.0f))
                {
                    collision = true;
                    break;
                }
            }
            if (remainder != 0 && !collision) // Also check the point itself
            {
                checkPoint = p2;
                if (Physics.Raycast(checkPoint, travelingDirection, CULL_LOOK_ITERATION) || Physics.Raycast(checkPoint, rightWhisker, CULL_LOOK_ITERATION / 2.0f) || Physics.Raycast(checkPoint, leftWhisker, CULL_LOOK_ITERATION / 2.0f))
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

        if (this.USE_PRIORITY)
        {
            foreach (var obstacle in obstacles)
            {
                var avoidObstacleMovement = new DynamicAvoidObstacle(obstacle)
                {
                    MaxAcceleration = this.MAX_ACCELERATION*2,
                    AvoidMargin = this.AVOID_MARGIN,
                    MaxLookAhead = this.MAX_LOOK_AHEAD,
                    Character = this.character.KinematicData,
                    DebugColor = Color.magenta
                };
                this.priorityMovement.Movements.Add(avoidObstacleMovement);
            }
        }

        this.arriveMovement = new DynamicArrive
        {
            ArriveTarget = new KinematicData { Position = path[pathNodeCounter] },
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            SlowRadius = 6.5f + 0.015f * this.EuclidianDistance(this.character.KinematicData.Position, this.path[pathNodeCounter]),
        };

        if (this.USE_PRIORITY)
        {
            this.priorityMovement.Movements.Add(arriveMovement);
            this.character.Movement = this.priorityMovement;
        }
        else
        {
            this.character.Movement = this.arriveMovement;
        }
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
                if (this.EuclidianDistance(this.character.KinematicData.Position, this.path[pathNodeCounter]) < this.cellSize / 2)
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

                        this.arriveMovement.SlowRadius = 6.5f + 0.015f * totalDistance; // The larger the distance, the sooner we should start slowing down

                        if (pathNodeCounter == this.path.Count - 1)
                        {// Slow down harsher for the last node
                            this.arriveMovement.SlowRadius = 6.5f + 0.025f * totalDistance;
                            this.arriveMovement.StopRadius = 2.5f;
                        }
                        //else
                        //{
                        //    // Fix sharp turns that lead into a long stretch
                        //    if (middles.Contains(pathNodeCounter))
                        //    {
                        //        this.arriveMovement.SlowRadius = 6.5f + 0.015f * totalDistance;
                        //    }
                        //    else
                        //    {
                        //        // Do an even bigger slowdown and add a middle point if need be
                        //        if ((this.path[pathNodeCounter].x == this.path[pathNodeCounter + 1].x || this.path[pathNodeCounter].z == this.path[pathNodeCounter + 1].z) ||
                        //            (this.path[pathNodeCounter].x == this.path[pathNodeCounter + 1].x+1 || this.path[pathNodeCounter].z == this.path[pathNodeCounter + 1].z+1) ||
                        //            (this.path[pathNodeCounter].x == this.path[pathNodeCounter + 1].x-1 || this.path[pathNodeCounter].z == this.path[pathNodeCounter + 1].z-1))
                        //        {
                        //            this.arriveMovement.SlowRadius = 6.5f + 0.02f * totalDistance;
                        //
                        //            float distance = this.EuclidianDistance(this.path[pathNodeCounter], this.path[pathNodeCounter + 1]);
                        //            Debug.Log(distance);
                        //            if (distance > 150 && !middles.Contains(pathNodeCounter))
                        //            { // Add middle point
                        //                Debug.Log("Adding middle point");
                        //                middles.Add(pathNodeCounter + 1);
                        //                if (this.path[pathNodeCounter].z == this.path[pathNodeCounter + 1].z)
                        //                    this.path.Insert(pathNodeCounter + 1, new Vector3(this.path[pathNodeCounter].x + distance / 2, 0, this.path[pathNodeCounter].z));
                        //                else
                        //                    this.path.Insert(pathNodeCounter + 1, new Vector3(this.path[pathNodeCounter].x, 0, this.path[pathNodeCounter].z + distance / 2));
                        //            }
                        //        }
                        //    }
                        //
                        //    /////////////////////////////////////////////////////////////////
                        //}

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
