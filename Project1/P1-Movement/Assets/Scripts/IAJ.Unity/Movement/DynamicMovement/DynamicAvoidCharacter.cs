using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidCharacter : DynamicMovement
    {
        public override string Name
        {
            get { return "Avoid Character"; }
        }

        public float MaxTimeLookAhead { get; set; }
        public float AvoidMargin { get; set; }


        public DynamicAvoidCharacter(KinematicData target)
        {
            this.Target = target;
            this.Output = new MovementOutput();
        }

        public override MovementOutput GetMovement()
        {
            this.Output.Clear();
            
            Vector3 deltaPos = this.Target.Position - this.Character.Position;
            deltaPos.y = 0.0f;
            Vector3 deltaVel = this.Target.velocity - this.Character.velocity;
            deltaVel.y = 0.0f;
            
            //Debug.Log("DeltaPos: " + deltaPos);
            //Debug.Log("DeltaVel: " + deltaVel);

            float deltaSqrSpeed = deltaVel.sqrMagnitude;

            if(deltaSqrSpeed == 0)
            {
                return this.Output;
            }

            float timeToClosest = -Vector3.Dot(deltaPos, deltaVel) / deltaSqrSpeed;

            // Time to closest longer than look ahead
            if(timeToClosest > this.MaxTimeLookAhead)
            {
                return this.Output;
            }

            Vector3 futureDeltaPos = deltaPos + deltaVel * timeToClosest;
            float futureDistance = futureDeltaPos.magnitude;
            
            // No collision
            if(futureDistance > 2 * this.AvoidMargin)
            {
                return this.Output;
            }

            // Immediate Collisions
            if(futureDistance <= 0 || deltaPos.magnitude < 2 * this.AvoidMargin)
            {
                this.Output.linear = this.Character.Position - this.Target.Position;
            }
            else
            {
                this.Output.linear = futureDeltaPos * -1;
            }

            this.Output.linear = this.Output.linear.normalized * this.MaxAcceleration;
            this.Output.linear.y = 0.0f;

            return this.Output;
            
        }
    }
}
