using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Util
{
    public static class MathHelper
    {
        public static Vector3 ConvertOrientationToVector(float orientation)
        {
            return new Vector3((float)Math.Sin(orientation), 0, (float)Math.Cos(orientation));
        }

        public static float ConvertVectorToOrientation(Vector3 vector)
        {
            return Mathf.Atan2(vector.x, vector.z);
        }

        public static Vector3 PerpendicularVector2D(Vector3 vector)
        {
            Vector3 perpendicularVector = new Vector3(vector.z, vector.y, -vector.x);

            perpendicularVector.Normalize();

            return perpendicularVector;
        }

        public static Vector3 Rotate2D(Vector3 vector, float angle)
        {
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);

            var x = vector.x*cos - vector.z*sin;
            var z = vector.x*sin + vector.z*cos;
            return new Vector3(x,vector.y,z);
        }

        //method adapted from https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
        //returns a negative value if there is no collision, and the time to the first collision if there is a collision detected
        public static float TimeToCollisionBetweenRayAndCircle(Vector3 position, Vector3 direction, Vector3 circleCenter, float radius)
        {
            // geometric solution
            float sqrRadius = radius * radius;
            Vector3 centerVector = circleCenter - position;
            float tca = Vector3.Dot(centerVector, direction);

            //no collision if the projection is negative, the current ray is moving away from the circle
            if (tca < 0) return -1;

            float sqrLineDistanceToCenter = Vector3.Dot(centerVector, centerVector) - tca * tca;

            //no collision if the distance to center is bigger than the radius of the circle
            if (sqrLineDistanceToCenter > sqrRadius) return -1;

            float thc = Mathf.Sqrt(sqrRadius - sqrLineDistanceToCenter);

            float t0 = tca - thc;
            float t1 = tca + thc;

            //if t0 is bigger, swap with t1
            if (t0 > t1)
            {
                var temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1; //if t0 is negative, use t1 instead
            }

            return t0;
        }


        //method adapted from https://gamedev.stackexchange.com/questions/18436/most-efficient-aabb-vs-ray-collision-algorithms
        //returns a negative value if there is no collision, and the time to the first collision if there is a collision detected
        public static float TimeToCollisionBetweenRayAndBox(Vector3 position, Vector3 direction, Vector3 boxCenter, Vector3 boxDimensions)
        {
            var dirFracX = 1.0f / direction.x;
            var dirFracY = 1.0f / direction.y;
            var dirFracZ = 1.0f / direction.z;

            Vector3 leftBottom = Vector3.zero;
            Vector3 rightBottom = Vector3.zero;
            Vector3 leftTop = Vector3.zero;
            Vector3 rightTop = Vector3.zero;

            leftBottom.x = boxCenter.x - (boxDimensions.x) / 2;
            rightBottom.x = boxCenter.x + (boxDimensions.x) / 2;
            leftTop.x = boxCenter.x - (boxDimensions.x) / 2;
            rightTop.x = boxCenter.x + (boxDimensions.x) / 2;

            leftBottom.z = boxCenter.z - (boxDimensions.z) / 2;
            rightBottom.z = boxCenter.z - (boxDimensions.z) / 2;
            leftTop.z = boxCenter.z + (boxDimensions.z) / 2;
            rightTop.z = boxCenter.z + (boxDimensions.z) / 2;

            Debug.DrawLine(leftTop, rightTop, Color.red);
            Debug.DrawLine(rightBottom, leftBottom, Color.red);
            Debug.DrawLine(rightTop, rightBottom, Color.red);
            Debug.DrawLine(leftBottom, leftTop, Color.red);

            float t1 = (leftBottom.x - position.x) * dirFracX;
            float t2 = (rightTop.x - position.x) * dirFracX;
            float t3 = (leftBottom.z - position.z) * dirFracZ;
            float t4 = (rightTop.z - position.z) * dirFracZ;

            float tMin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)));
            float tMax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)));

            if(tMax < 0)
            {
                return -1;
            }

            if(tMin > tMax)
            {
                return -1;
            }

            return tMin;
        }
    }
}
