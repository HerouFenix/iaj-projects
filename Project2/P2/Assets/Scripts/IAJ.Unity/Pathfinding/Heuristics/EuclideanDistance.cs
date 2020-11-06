using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        protected const int MOVE_STRAIGHT_COST = 10;
        protected const int MOVE_DIAGONAL_COST = 14;

        //public float CellSize;
        //public EuclideanDistance(float cellSize)
        //{
        //    this.CellSize = cellSize;
        //}

        public float H(NodeRecord node, NodeRecord goalNode)
        {
            // The following doesn't really work unless we set the moving distances (i.e the cost to walk between nodes) to 1
            //float x = (goalNode.x - node.x);
            //float y = (goalNode.y - node.y);
            //
            //return Mathf.Sqrt(x * x + y * y);

            //float x = (goalNode.x - node.x)*this.CellSize;
            //float y = (goalNode.y - node.y)* this.CellSize;

            //float x = (goalNode.x - node.x) * MOVE_STRAIGHT_COST;
            //float y = (goalNode.y - node.y) * MOVE_STRAIGHT_COST;
            //
            //return Mathf.RoundToInt(Mathf.Sqrt(x * x + y * y));

            float x = (goalNode.x - node.x);
            float y = (goalNode.y - node.y);
            
            return Mathf.FloorToInt(Mathf.Sqrt(x * x + y * y) * MOVE_STRAIGHT_COST);

            //int xDistance = Mathf.Abs(goalNode.x - node.x);
            //int yDistance = Mathf.Abs(goalNode.y - node.y);
            //int remaining = Mathf.Abs(xDistance - yDistance);
            //
            //return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
    }
}
