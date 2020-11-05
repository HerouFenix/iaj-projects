using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float CellSize;
        public EuclideanDistance(float cellSize)
        {
            this.CellSize = cellSize;
        }

        public float H(NodeRecord node, NodeRecord goalNode)
        {
            // The following doesn't really work unless we set the moving distances (i.e the cost to walk between nodes) to 1
            //float x = (goalNode.x - node.x);
            //float y = (goalNode.y - node.y);
            //
            //return Mathf.Sqrt(x * x + y * y);

            float x = (goalNode.x - node.x)*this.CellSize;
            float y = (goalNode.y - node.y)* this.CellSize;
                        
            return Mathf.Sqrt(x*x + y*y);
        }
    }
}
