﻿using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Movement.Arbitration;
using Assets.Scripts.IAJ.Unity.Movement.VO;
using Assets.Scripts.IAJ.Unity.Movement;

public class MainCharacterController : MonoBehaviour {

    public const float X_WORLD_SIZE = 56;
    public const float Z_WORLD_SIZE = 56;
    public float MAX_ACCELERATION = 20;
    private float MAX_SPEED = 20;
    public float DRAG = 0.01f;

    public float MAX_LOOK_AHEAD = 5.0f;
    public float AVOID_MARGIN = 4.0f;

    public float MAX_TIME_LOOK_AHEAD = 3.0f;
    public float COLLISION_RADIUS = 1.0f;

    public GameObject movementText;
    public DynamicCharacter character;

    public PriorityMovement priorityMovement;
    public BlendedMovement blendedMovement;
    public RVOMovement rvoMovement;
    public Transform destination;
    public Transform spawn;

    private DynamicPatrol patrolMovement;
    private List<Transform> destinations;
   
    //early initialization
    void Awake()
    {
        // Creating a new Dynamic Character
        this.character = new DynamicCharacter(this.gameObject);

        // Making sure we know all the possible destinations
        destinations = new List<Transform>();
        var goals = GameObject.Find("DestinationPoints").transform;
        for (int i = 0; i < goals.childCount; i++)
            destinations.Add(goals.transform.GetChild(i));

        // Initializing the Priority and Blended movements
        this.priorityMovement = new PriorityMovement
        {
            Character = this.character.KinematicData
        };

        this.blendedMovement = new BlendedMovement
        {
            Character = this.character.KinematicData
        };
    }

    // Use this for initialization
    void Start ()
    {
       
    }

    public void InitializeMovement(GameObject[] obstacles, List<DynamicCharacter> characters)
    {


        foreach (var obstacle in obstacles)
        {
            var avoidObstacleMovement = new DynamicAvoidObstacle(obstacle)
            {
                MaxAcceleration = MAX_ACCELERATION,
                AvoidMargin = AVOID_MARGIN,
                MaxLookAhead = MAX_LOOK_AHEAD,
                Character = this.character.KinematicData,
                DebugColor = Color.magenta
            };
            this.blendedMovement.Movements.Add(new MovementWithWeight(avoidObstacleMovement, 2.0f));
            this.priorityMovement.Movements.Add(avoidObstacleMovement);
        }

        if(characters.Any())
        foreach (var otherCharacter in characters)
        {
            if (otherCharacter != this.character)
            {
                    //TODO: add your AvoidCharacter movement here
                    var avoidCharacter = new DynamicAvoidCharacter(otherCharacter.KinematicData)
                    {
                        Character = this.character.KinematicData,
                        MaxAcceleration = MAX_ACCELERATION,
                        DebugColor = Color.cyan,
                        MaxTimeLookAhead = MAX_TIME_LOOK_AHEAD,
                        AvoidMargin = COLLISION_RADIUS

                    };

                this.blendedMovement.Movements.Add(new MovementWithWeight(avoidCharacter, 2.0f));
                this.priorityMovement.Movements.Add(avoidCharacter);
            }
        }

        this.patrolMovement = new DynamicPatrol(this.character.KinematicData.Position, new Vector3(0.0f, 0.0f, 0.0f))
        {
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            DebugColor = Color.yellow
        };

        this.rvoMovement = new RVOMovement(this.patrolMovement, characters.Select(c => c.KinematicData).ToList(), obstacles.ToList())
        {
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            DebugColor = Color.black,
        };

        this.priorityMovement.Movements.Add(patrolMovement);
        this.blendedMovement.Movements.Add(new MovementWithWeight(patrolMovement, 1));
        this.character.Movement = this.priorityMovement;
        this.ChangeTargetRandom();
        
    }

    // Method designed to handle User's Inputs
    public void ChangeMovement(string movement)
    {

        switch (movement)
        {
            case "Priority":
                this.character.Movement = this.priorityMovement;
                break;
            case "Blended":
                this.character.Movement = this.blendedMovement;
                break;

            case "RVO":
                this.character.Movement = this.rvoMovement;
                break;

            case "Stop":
                this.character.Movement = null;
                break;

        }

    }


    // When we create characters dinamically we need to update the ones that already exist
    public void UpdateAvoidCharacterList(List<MainCharacterController> characterControllers, GameObject[] obstacles, List<DynamicCharacter> characters)
    {
        if (characterControllers.Any())
            foreach (var otherCharacter in characterControllers)
            {
                
                if (otherCharacter.character != this.character)
                {
                    //TODO: add your AvoidCharacter movement here
                    var avoidCharacter = new DynamicAvoidCharacter(otherCharacter.character.KinematicData)
                    {
                        Character = this.character.KinematicData,
                        MaxAcceleration = MAX_ACCELERATION,
                        DebugColor = Color.cyan,
                        MaxTimeLookAhead = MAX_TIME_LOOK_AHEAD,
                        AvoidMargin = COLLISION_RADIUS
                    };

                    this.blendedMovement.Movements.Add(new MovementWithWeight(avoidCharacter, 2.0f));
                    this.priorityMovement.Movements.Add(avoidCharacter);
                }
            }
        this.rvoMovement = new RVOMovement(this.patrolMovement, characters.Select(c => c.KinematicData).ToList(), obstacles.ToList())
        {
            Character = this.character.KinematicData,
            MaxAcceleration = MAX_ACCELERATION,
            MaxSpeed = MAX_SPEED,
            DebugColor = Color.black
        };
    }

    void Update()
    {
            this.UpdateMovingGameObject();
    }

    private void UpdateMovingGameObject()
    {
        if (this.character.Movement != null)
        {
            this.character.Update();
            bool changed = this.character.KinematicData.ApplyWorldLimit(X_WORLD_SIZE, Z_WORLD_SIZE);

            //TODO : REMOVE THESE COMMENTS. USED ONLY FOR DEBUGGING AVOID CHARACTER
            //if (changed)
            //    ChangeTargetRandom();
        }

    }

    // The objective of this method is to everytime the character changes location its target also changes
    private void ChangeTargetRandom()
    {
        //Current Patrol destination:
        Vector3 currentDestination;
        currentDestination = patrolMovement.PatrolPosition1.Position;

        //Retrieve the one that is closed
        var closestObjective = destinations.OrderByDescending(x => (x.position - this.character.KinematicData.Position).magnitude);

        // We want a new destination different from the one we have right now
        var newPossibleObjectives = destinations.Where(x => x != closestObjective.Last()).ToList();
        var index = Random.Range(0, destinations.Count - 1);
        var newObjective = newPossibleObjectives[index];

        // Update Patrol Movement with new location;
        patrolMovement.PatrolPosition1.Position = newObjective.transform.position;
    }

  

}
