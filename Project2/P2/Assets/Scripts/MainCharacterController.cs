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

    public GameObject movementText;
    public DynamicCharacter character;

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
    }

    public void InitializeMovement()
    {
        this.seekMovement = new DynamicSeek
        {
            Character = this.character.KinematicData,
            Target = new KinematicData { Position = path[pathNodeCounter] },
            MaxAcceleration = MAX_ACCELERATION,
        };

        this.arriveMovement = new DynamicArrive
        {
            ArriveTarget = new KinematicData { Position = path[this.pathNodeCounter] },
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            DebugColor = Color.yellow,
        };

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
            if (this.pathNodeCounter < this.path.Count)
            {
                if (this.EuclidiaDistance(this.character.KinematicData.Position, this.path[pathNodeCounter]) < 1)
                {
                    this.arriveMovement.ArriveTarget = new KinematicData { Position = this.path[pathNodeCounter++] };
                }
            }
        }

    }

    private float EuclidiaDistance(Vector3 pos1, Vector3 pos2)
    {
        float x = (pos2.x - pos1.x);
        float y = (pos2.z - pos1.z);

        float result = Mathf.Sqrt(x * x + y * y);
        return result;
    }
}
