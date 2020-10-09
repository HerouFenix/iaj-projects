using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicAvoidObstacle : DynamicSeek
    {
        public override string Name
        {
            get { return "Avoid Obstacle"; }
        }

        private GameObject obstacle;

        public GameObject Obstacle
        {
            get { return this.obstacle; }
            set
            {
                this.obstacle = value;
                this.ObstacleCollider = value.GetComponent<Collider>();
            }
        }

        private Collider ObstacleCollider { get; set; }
        public float MaxLookAhead { get; set; }

        public float AvoidMargin { get; set; }

        public float FanAngle { get; set; }

        public DynamicAvoidObstacle(GameObject obstacle)
        {
            this.Obstacle = obstacle;
            this.Target = new KinematicData();
        }

        public override MovementOutput GetMovement()
        {
            RaycastHit hit;
            Color mainRayColor = Color.white;
            Color leftRayColor = Color.white;
            Color rightRayColor = Color.white;

            MovementOutput movementOutput = new MovementOutput();

            // If we're not moving, no need to check for collisions
            // This had to be added due to errors when creating the rays with a 0 velocity
            if (this.Character.velocity.magnitude <= 0)
            {
                return movementOutput;
            }

            Ray mainRay = new Ray(this.Character.Position, this.Character.velocity.normalized);

            /*
            Vector3 leftWhisker = (Quaternion.Euler(0, -37, 0) * this.Character.velocity);
            Vector3 rightWhisker = (Quaternion.Euler(0, 37, 0) * this.Character.velocity);
            */

            Vector3 leftWhisker = MathHelper.Rotate2D(this.Character.velocity, -MathConstants.MATH_PI_6);
            Vector3 rightWhisker = MathHelper.Rotate2D(this.Character.velocity, MathConstants.MATH_PI_6);

            Ray leftRay = new Ray(this.Character.Position, leftWhisker.normalized);
            Ray rightRay = new Ray(this.Character.Position, rightWhisker.normalized);

            //Debug.DrawRay(this.Character.Position, new Vector3(1.0f, 0.0f, 1.0f), color);

            bool collision = false;

            //Check Collisions
            if (ObstacleCollider.Raycast(mainRay, out hit, this.MaxLookAhead))
            {
                mainRayColor = Color.red;
                collision = true;
            }else if (ObstacleCollider.Raycast(leftRay, out hit, this.MaxLookAhead/2))
            {
                leftRayColor = Color.red;
                collision = true;
            }else if(ObstacleCollider.Raycast(rightRay, out hit, this.MaxLookAhead/2))
            {
                rightRayColor = Color.red;
                collision = true;
            }


            if (collision)
            {
                base.Target.Position = hit.point + hit.normal * this.AvoidMargin;
                movementOutput = base.GetMovement();
            }

            Debug.DrawRay(this.Character.Position, this.Character.velocity.normalized * this.MaxLookAhead, mainRayColor);
            Debug.DrawRay(this.Character.Position, leftWhisker.normalized * this.MaxLookAhead/2, leftRayColor);
            Debug.DrawRay(this.Character.Position, rightWhisker.normalized * this.MaxLookAhead/2, rightRayColor);

            return movementOutput;
        }
    }
}
