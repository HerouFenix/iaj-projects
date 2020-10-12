using UnityEngine;
using Assets.Scripts.IAJ.Unity.Util;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicCharacter
    {
        public GameObject GameObject { get; protected set; }
        public KinematicData KinematicData { get; protected set; }
        private DynamicMovement movement;
        public DynamicMovement Movement
        {
            get { return this.movement; }
            set
            {
                this.movement = value;
                if (this.movement != null) this.movement.Character = this.KinematicData;
            }
        }
        public float Drag { get; set; }
        public float MaxSpeed { get; set; }
        public bool UsingActuators { get; set; }
        public DynamicCharacter(GameObject gameObject)
        {
            this.KinematicData = new KinematicData(new StaticData(gameObject.transform));
            this.GameObject = gameObject;
            this.Drag = 1;
            this.MaxSpeed = 20.0f;

            this.UsingActuators = false;
        }

        // Update is called once per frame
        public void Update()
        {
            if (this.Movement != null)
            {
                MovementOutput output = this.Movement.GetMovement();

                if (output != null)
                {
                    Debug.DrawRay(this.GameObject.transform.position, output.linear, this.Movement.DebugColor);

                    //APPLY OUTPUT FILTERING

                    if (this.UsingActuators)
                    {
                        //Debug.Log("Using actuators");
                        //Debug.Log("Next Orientation:" + output.linear.normalized);
                        //Debug.Log("Current Orientation:" + this.KinematicData.GetOrientationAsVector());
                        //Debug.Log("Difference:" + MathHelper.ConvertVectorToOrientation(this.KinematicData.GetOrientationAsVector() - output.linear.normalized));
                        //Debug.Log("Current speed:" + this.KinematicData.velocity.magnitude);

                        // See how much we want to turn (our angular velocity)
                        var difference = MathHelper.ConvertVectorToOrientation(this.KinematicData.GetOrientationAsVector() - output.linear.normalized);

                        //If the car is stopped or moving slowly we can't suddenly turn it, instead accelerate forward. We also cant turn much if our speed isnt high enough
                        if (this.KinematicData.velocity.magnitude <= 1.5f && difference != 0 || this.KinematicData.velocity.magnitude < difference * 1.5f)
                        {
                            // Make it go forward to get up speed
                            output.linear = this.GameObject.transform.forward.normalized * MaxSpeed;
                        }
                    }
                    
                    ////////////////////////

                    this.KinematicData.Integrate(output, this.Drag, Time.deltaTime);
                    this.KinematicData.SetOrientationFromVelocity();
                    this.KinematicData.TrimMaxSpeed(this.MaxSpeed);
                }
            }
        }
    }
}
