//adapted to IAJ classes by João Dias and Manuel Guimarães

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine;

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
        public float CharacterSize { get; set; }
        public float ObstacleSize { get; set; }
        public float IgnoreDistance { get; set; }
        public float MaxSpeed { get; set; }

        protected DynamicMovement.DynamicMovement DesiredMovement { get; set; }

        public RVOMovement(DynamicMovement.DynamicMovement goalMovement, List<KinematicData> movingCharacters, List<GameObject> obs)
        {
            this.DesiredMovement = goalMovement;
            base.Target = new KinematicData();
            this.Characters = movingCharacters;
        }

        private Vector3 getBestSample(Vector3 desiredVelocity, List<Vector3> samples)
        {
            Vector3 bestSample = Vector3.zero;
            float minimumPenalty = Mathf.Infinity;

            foreach(Vector3 sample in samples){
                float distancePenalty = (desiredVelocity - sample).magnitude;
                float maximumTimePenalty = 0;

                foreach(KinematicData ch in this.Characters)
                {
                    if (ch.Equals(this.Character)) continue;

                    Vector3 deltaP = ch.Position - this.Character.Position;

                    if(deltaP.magnitude > this.IgnoreDistance)
                    {
                        continue;
                    }

                    Vector3 rayVector = 2 * sample - this.Character.velocity - ch.velocity;
                    float tc = MathHelper.TimeToCollisionBetweenRayAndCircle(this.Character.Position, rayVector, ch.Position, this.CharacterSize * 2);
                    float timePenalty = 0;

                    float weightAvoidCollision = 50f; //TODO : Try for different weights

                    if(tc > 0)
                    {
                        timePenalty = weightAvoidCollision / tc;
                    }else if(Mathf.Abs(tc-0.0f) <= 0.01f)
                    {
                        timePenalty = Mathf.Infinity;
                    }
                    else
                    {
                        timePenalty = 0;
                    }

                    if(timePenalty > maximumTimePenalty)
                    {
                        maximumTimePenalty = timePenalty;
                    }
                }

                float penalty = distancePenalty + maximumTimePenalty;

                if(penalty < minimumPenalty)
                {
                    minimumPenalty = penalty;
                    bestSample = sample;
                }
            }


            return bestSample;
        }

        public override MovementOutput GetMovement()
        {
            var desiredMovementOutput = this.DesiredMovement.GetMovement();

            Vector3 desiredVelocity = this.Character.velocity + desiredMovementOutput.linear;
            desiredVelocity.y = 0.0f;

            // Trim desired velocity to max speed if needed
            if (desiredVelocity.magnitude > this.MaxSpeed)
            {
                desiredVelocity.Normalize();
                desiredVelocity *= this.MaxSpeed;
            }

            List<Vector3> samples = new List<Vector3>();

            samples.Add(desiredVelocity);

            int numSamples = 100;

            for(int i = 0; i < numSamples; i++)
            {
                float angle = RandomHelper.RandomBinomial(MathConstants.MATH_2PI);

                float magnitude = RandomHelper.RandomBinomial(this.MaxSpeed);

                Vector3 velocitySample = MathHelper.ConvertOrientationToVector(angle) * magnitude;

                samples.Add(velocitySample);
            }

            base.Target.velocity = this.getBestSample(desiredVelocity,samples);

            return base.GetMovement();
        }

        
    }
}
