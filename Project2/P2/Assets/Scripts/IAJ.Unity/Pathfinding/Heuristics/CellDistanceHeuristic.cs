using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class CellDistanceHeuristic : IHeuristic
    {
        public float CellSize;
        protected const int MOVE_STRAIGHT_COST = 10;
        protected const int MOVE_DIAGONAL_COST = 14;

        public CellDistanceHeuristic(float cellSize)
        {
            this.CellSize = cellSize;
        }

        public float H(NodeRecord node, NodeRecord goalNode)
        {
            //int xDistance = Mathf.Abs(goalNode.x - node.x);
            //int yDistance = Mathf.Abs(goalNode.y - node.y);
            //int remaining = Mathf.Abs(xDistance - yDistance);
            //
            //return Mathf.Sqrt(this.CellSize * this.CellSize + this.CellSize * this.CellSize) * Mathf.Min(xDistance, yDistance) + this.CellSize * remaining;

            int xDistance = Mathf.Abs(goalNode.x - node.x);
            int yDistance = Mathf.Abs(goalNode.y - node.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
    }
}