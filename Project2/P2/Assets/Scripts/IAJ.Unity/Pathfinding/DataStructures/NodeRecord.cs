using System;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public enum NodeStatus
    {
        Unvisited,
        Open,
        Closed
    }

    public class NodeRecord  : IComparable<NodeRecord>
    {
        //Coordinates
        public int x;
        public int y;

        public NodeRecord parent;
        public float gCost;
        public float hCost;
        public float fCost;
        public NodeStatus status;
        public bool isWalkable;

        public static int next_index = 0;
        public int NodeIndex;

        public override string ToString()
        {
            return x + "," + y;
        }

        public int CompareTo(NodeRecord other)
        {
            return this.fCost.CompareTo(other.fCost);
        }

        public NodeRecord(int x, int y)
        {
            
            this.x = x;
            this.y = y;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
            isWalkable = true;
            status = NodeStatus.Unvisited;

            NodeIndex = next_index;
            next_index++;
        }
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}
