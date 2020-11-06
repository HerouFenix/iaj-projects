using System;
using System.Collections.Generic;

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

        public bool[] directions = { false, false, false, false }; // N S E W

        public string travelingDirection;

        public Dictionary<string, int> distances;

        public override string ToString()
        {
            return x + "," + y;
        }

        public int CompareTo(NodeRecord other)
        {
            var comp = this.fCost.CompareTo(other.fCost);
            if (comp == 0)
            {
                comp = this.hCost.CompareTo(other.hCost);
            }

            return comp;
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

            distances = new Dictionary<string, int>();
            distances.Add("S", 0);
            distances.Add("SE",0);
            distances.Add("E", 0);
            distances.Add("NE", 0);
            distances.Add("N", 0);
            distances.Add("NW", 0);
            distances.Add("W", 0);
            distances.Add("SW", 0);

            NodeIndex = next_index;
            next_index++;
        }
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}
