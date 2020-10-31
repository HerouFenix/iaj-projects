using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(NodeRecord node, NodeRecord goalNode)
        {
            float x = goalNode.x - node.x;
            float y = goalNode.y - node.y;
            return Mathf.Sqrt(x*x + y*y);
        }
    }
}
