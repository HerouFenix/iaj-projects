using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class CellDistanceHeuristic : IHeuristic
    {
        public float CellSize;
        public CellDistanceHeuristic(float cellSize)
        {
            this.CellSize = cellSize;
        }

        public float H(NodeRecord node, NodeRecord goalNode)
        {
            int xDistance = Mathf.Abs(goalNode.x - node.x);
            int yDistance = Mathf.Abs(goalNode.y - node.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return Mathf.Sqrt(this.CellSize * this.CellSize + this.CellSize * this.CellSize) * Mathf.Min(xDistance, yDistance) + this.CellSize * remaining;
        }
    }
}