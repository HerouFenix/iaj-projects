//adapted to IAJ classes by João Dias and Manuel Guimarães

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.IAJ.Unity.Movement.VO
{
    public class RVOMovement : DynamicMovement.DynamicVelocityMatch
    {
        public override string Name
        {
            get { return "RVO"; }
        }

        protected List<KinematicData> Characters { get; set; }
        protected List<GameObject> Obstacles { get; set; }

        protected List<Collider> ObstacleColliders { get; set; }
        protected List<Vector3> ObstaclePositions { get; set; }

        public float CharacterSize { get; set; }
        public float ObstacleSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<GameObject> obs, KinematicData character)
        {
            this.DesiredMovement = goalMovement;
            base.Target = new KinematicData();

            // Add characters list
            this.Characters = movingCharacters;
            this.Characters.Remove(character);
            //this.Characters = new List<KinematicData>();

            // Add obstacle list
            this.Obstacles = obs;

            this.ObstacleColliders = new List<Collider>();
            this.ObstaclePositions = new List<Vector3>();

            // Create dictionary of colliders
            foreach (var obst in this.Obstacles)
            {
                this.ObstaclePositions.Add(obst.transform.position);
                this.ObstacleColliders.Add(obst.GetComponent<Collider>());
            }
        }

        private Vector3 getBestSample(Vector3 desiredVelocity, List<Vector3> samples)
        {
            var bestSample = Vector3.zero;
            var minimumPenalty = Mathf.Infinity;

            foreach (var sample in samples)
            {
                var distancePenalty = (desiredVelocity - sample).magnitude; // Penalty based on the difference to the desired velocity
                //Debug.Log("Desired:" + desiredVelocity);
                //Debug.Log("Sample:" + sample);
                //Debug.Log(distancePenalty);

                var maximumTimePenalty = 0.0f;

                // Get maximum time penalty with the characters
                foreach (var ch in this.Characters)
                {
                    //if (ch == this.Character) continue; //Ignore ourselves

                    // Distance between our character and this specific character
                    var deltaP = ch.Position - this.Character.Position;

                    if (deltaP.magnitude > this.IgnoreDistance) continue; // Ignore this character (too far away)

                    var rayVector = 2 * sample - this.Character.velocity - ch.velocity;

                    // FOR DEBUG PURPOSES ONLY //
                    //Vector3 leftBottom = Vector3.zero;
                    //Vector3 rightBottom = Vector3.zero;
                    //Vector3 leftTop = Vector3.zero;
                    //Vector3 rightTop = Vector3.zero;
                    //
                    //leftBottom.x = ch.Position.x - this.CharacterSize;
                    //rightBottom.x = ch.Position.x + this.CharacterSize;
                    //leftTop.x = ch.Position.x - this.CharacterSize;
                    //rightTop.x = ch.Position.x + this.CharacterSize;
                    //
                    //leftBottom.z = ch.Position.z - this.CharacterSize;
                    //rightBottom.z = ch.Position.z - this.CharacterSize;
                    //leftTop.z = ch.Position.z + this.CharacterSize;
                    //rightTop.z = ch.Position.z + this.CharacterSize;
                    //
                    //Debug.DrawLine(leftBottom, leftTop, Color.yellow);
                    //Debug.DrawLine(leftTop, rightTop, Color.yellow);
                    //Debug.DrawLine(rightTop, rightBottom, Color.yellow);
                    //Debug.DrawLine(rightBottom, leftBottom, Color.yellow);
                    /////////////////////////////

                    var timeCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, ch.Position, this.CharacterSize);
                    //Debug.Log(tc);

                    var timePenalty = 0.0f; // Penalty based on how close to collision we would be using this sample.
                    var timeWeight = 15.0f; // The higher this value the more we value avoiding character collisions

                    if (timeCollision > 0)
                    { // Future Collision
                        timePenalty = timeWeight / timeCollision;
                    }
                    else if (timeCollision == 0)
                    { // Immediate Collision
                        timePenalty = Mathf.Infinity;
                    }

                    if (timePenalty > maximumTimePenalty)
                    {
                        maximumTimePenalty = timePenalty;
                    }
                }

                int index = 0;
                // Get maximum time penalty with the obstacles
                foreach (var obs in this.Obstacles)
                {
                    Vector3 obstaclePosition = this.ObstaclePositions[index];

                    Collider obstacleCollider = this.ObstacleColliders[index];
                    index++;

                    var timeCollision = -1.0f;
                    var timeWeight = 8.0f; // The higher this value the more we value avoiding character collisions

                    if (obstacleCollider.GetType() == typeof(BoxCollider))
                    { // Deal with walls and roadblocks

                        //if (obs.name.Contains("RoadBlock"))
                        //{
                        //    var deltaP = obstaclePosition - this.Character.Position;
                        //
                        //    if (deltaP.magnitude > this.IgnoreDistance) continue; // Ignore this character (too far away)
                        //
                        //    var rayVector = 2 * sample - this.Character.velocity;
                        //
                        //    // FOR DEBUG PURPOSES ONLY //
                        //    //Vector3 leftBottom = Vector3.zero;
                        //    //Vector3 rightBottom = Vector3.zero;
                        //    //Vector3 leftTop = Vector3.zero;
                        //    //Vector3 rightTop = Vector3.zero;
                        //    //
                        //    //leftBottom.x = obstaclePosition.x - this.CharacterSize;
                        //    //rightBottom.x = obstaclePosition.x + this.CharacterSize;
                        //    //leftTop.x = obstaclePosition.x - this.CharacterSize;
                        //    //rightTop.x = obstaclePosition.x + this.CharacterSize;
                        //    //
                        //    //leftBottom.z = obstaclePosition.z - this.CharacterSize;
                        //    //rightBottom.z = obstaclePosition.z - this.CharacterSize;
                        //    //leftTop.z = obstaclePosition.z + this.CharacterSize;
                        //    //rightTop.z = obstaclePosition.z + this.CharacterSize;
                        //    //
                        //    //Debug.DrawLine(leftBottom, leftTop, Color.red);
                        //    //Debug.DrawLine(leftTop, rightTop, Color.red);
                        //    //Debug.DrawLine(rightTop, rightBottom, Color.red);
                        //    //Debug.DrawLine(rightBottom, leftBottom, Color.red);
                        //    /////////////////////////////
                        //
                        //    //timeCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, obstaclePosition, this.CharacterSize);
                        //    timeCollision = MathHelper.TimeToCollisionBetweenRayAndBox(this.Character.Position, rayVector, obstaclePosition, new Vector3(this.CharacterSize*2, 0, this.CharacterSize*2));
                        //
                        //    //Debug.Log(tc);
                        //}
                        //else
                        //{

                        // FIRST METHOD OF COLLISION DETECTION (using the closest point on the collider bound
                        //var closestPoint = obstacleCollider.ClosestPointOnBounds(this.Character.Position);
                        //var deltaP = closestPoint - this.Character.Position;
                        //if (deltaP.magnitude > (this.IgnoreDistance)) continue; // Ignore this wall (too far away)


                        // SECOND METHOD OF COLLISION DETECTION (using raycasts) - SLOWER but more effective
                        if (this.Character.velocity.magnitude == 0) continue;
                        
                        Ray mainRay = new Ray(this.Character.Position, this.Character.velocity.normalized);
                        
                        Vector3 leftWhisker = MathHelper.Rotate2D(this.Character.velocity, -MathConstants.MATH_PI_6);
                        Vector3 rightWhisker = MathHelper.Rotate2D(this.Character.velocity, MathConstants.MATH_PI_6);
                        
                        Ray leftRay = new Ray(this.Character.Position, leftWhisker.normalized);
                        Ray rightRay = new Ray(this.Character.Position, rightWhisker.normalized);
                        
                        RaycastHit hit;
                        
                        if (!obstacleCollider.Raycast(mainRay, out hit, this.IgnoreDistance) && !obstacleCollider.Raycast(leftRay, out hit, this.IgnoreDistance/2) && !obstacleCollider.Raycast(rightRay, out hit, this.IgnoreDistance/2))
                        {
                            continue;
                        }

                        var rayVector = 2 * sample - this.Character.velocity;

                        timeCollision = MathHelper.TimeToCollisionBetweenRayAndBox(this.Character.Position, rayVector, obstaclePosition, obstacleCollider.bounds.size);
                        //Debug.Log(tc);

                        timeWeight = 9.0f;
                        //}

                    }
                    else if (obstacleCollider.GetType() == typeof(SphereCollider))
                    { // Deal with roundabout
                        var deltaP = obstaclePosition - this.Character.Position;

                        if (deltaP.magnitude > (this.IgnoreDistance + this.ObstacleSize + 1)) continue; // Ignore this character (too far away)

                        var rayVector = 2 * sample - this.Character.velocity;

                        // FOR DEBUG PURPOSES ONLY //
                        //Vector3 leftBottom = Vector3.zero;
                        //Vector3 rightBottom = Vector3.zero;
                        //Vector3 leftTop = Vector3.zero;
                        //Vector3 rightTop = Vector3.zero;
                        //
                        //leftBottom.x = obstaclePosition.x - this.ObstacleSize;
                        //rightBottom.x = obstaclePosition.x + this.ObstacleSize;
                        //leftTop.x = obstaclePosition.x - this.ObstacleSize;
                        //rightTop.x = obstaclePosition.x + this.ObstacleSize;
                        //
                        //leftBottom.z = obstaclePosition.z - this.ObstacleSize;
                        //rightBottom.z = obstaclePosition.z - this.ObstacleSize;
                        //leftTop.z = obstaclePosition.z + this.ObstacleSize;
                        //rightTop.z = obstaclePosition.z + this.ObstacleSize;
                        //
                        //Debug.DrawLine(leftBottom, leftTop, Color.blue);
                        //Debug.DrawLine(leftTop, rightTop, Color.blue);
                        //Debug.DrawLine(rightTop, rightBottom, Color.blue);
                        //Debug.DrawLine(rightBottom, leftBottom, Color.blue);
                        /////////////////////////////

                        timeCollision = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, obstaclePosition, this.ObstacleSize);
                        //Debug.Log(tc);

                        timeWeight = 8.0f;
                    }



                    var timePenalty = 0.0f; // Penalty based on how close to collision we would be using this sample.

                    if (timeCollision > 0)
                    { // Future Collision
                        timePenalty = timeWeight / timeCollision;
                    }
                    else if (timeCollision == 0)
                    { // Immediate Collision
                        timePenalty = Mathf.Infinity;
                    }

                    if (timePenalty > maximumTimePenalty)
                    {
                        maximumTimePenalty = timePenalty;
                    }

                }


                // Total Penalty
                var penalty = distancePenalty + maximumTimePenalty;

                if (penalty < minimumPenalty)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;
                }
            }


            return bestSample;
        }

        public override MovementOutput GetMovement()
        {
            // 1 - First, compute the desired velocity (the velocity when going straight to the target)
            var desiredMovementOutput = this.DesiredMovement.GetMovement();

            var desiredVelocity = this.Character.velocity + desiredMovementOutput.linear;

            //Trim desired velocity if bigger than max
            if (desiredVelocity.magnitude > this.MaxSpeed)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= this.MaxSpeed;
            }


            // 2 - Generate random samples
            List<Vector3> samples = new List<Vector3>();

            // Add the desired velocity to our samples
            samples.Add(desiredVelocity);

            int noSamples = 100; // Number of samples to generate
            for (int i = 0; i < noSamples; i++)
            {
                var angle = Mathf.Abs(RandomHelper.RandomBinomial() * MathConstants.MATH_2PI);
                var magnitude = Mathf.Abs(RandomHelper.RandomBinomial() * this.MaxSpeed);

                var velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;

                //Debug.Log(velocitySample);
                //Debug.Log(magnitude);

                samples.Add(velocitySample);
            }

            // 3 - Find the best sample out of the ones generated
            base.Target.velocity = this.getBestSample(desiredVelocity, samples);
            //Debug.Log(base.Target.velocity);

            return base.GetMovement();
        }


    }
}
