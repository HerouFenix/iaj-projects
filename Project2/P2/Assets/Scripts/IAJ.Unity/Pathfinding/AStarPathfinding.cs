using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class AStarPathfinding
    {
        protected const int MOVE_STRAIGHT_COST = 10;
        protected const int MOVE_DIAGONAL_COST = 14;
        public Grid<NodeRecord> grid { get; set; }
        public uint NodesPerSearch { get; set; }
        public int MaxOpenNodes { get; protected set; }
        public uint TotalExploredNodes { get; protected set; }
        public float TotalProcessingTime { get; set; }
        public bool InProgress { get; set; }
        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }

        public int counter = 0;

        //heuristic function
        public IHeuristic Heuristic { get; protected set; }

        public int StartPositionX { get; set; }
        public int StartPositionY { get; set; }
        public int GoalPositionX { get; set; }
        public int GoalPositionY { get; set; }
        public NodeRecord GoalNode { get; set; }
        public NodeRecord StartNode { get; set; }

        public AStarPathfinding(int width, int height, float cellSize, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            grid = new Grid<NodeRecord>(width, height, cellSize, (Grid<NodeRecord> global, int x, int y) => new NodeRecord(x, y));
            this.Open = open;
            this.Closed = closed;
            this.InProgress = false;
            this.Heuristic = heuristic;
            //this.NodesPerSearch = uint.MaxValue;
            this.NodesPerSearch = 15;

        }
        virtual public void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            this.StartPositionX = startX;
            this.StartPositionY = startY;
            this.GoalPositionX = goalX;
            this.GoalPositionY = goalY;
            this.StartNode = grid.GetGridObject(StartPositionX, StartPositionY);
            this.GoalNode = grid.GetGridObject(GoalPositionX, GoalPositionY);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalExploredNodes = 0;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord(StartNode.x, StartNode.y)
            {
                gCost = 0,
                hCost = this.Heuristic.H(this.StartNode, this.GoalNode),
                NodeIndex = StartNode.NodeIndex
            };

            initialNode.CalculateFCost();

            this.Open.Initialize();
            this.Open.AddToOpen(initialNode);
            this.Closed.Initialize();
        }

        virtual public bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false, int totalNodesSearched = 0)
        {
            int openSize = this.Open.CountOpen();
            // Check if our open list is now bigger than our all-time maximum
            if (openSize  > this.MaxOpenNodes)
            {
                this.MaxOpenNodes = openSize;
            }

            // Check if we have no more open nodes
            if (openSize == 0)
            {
                solution = null;
                return false;
            }

            // CurrentNode is the best one from the Open set, start with that
            var currentNode = this.Open.GetBestAndRemove();

            this.TotalExploredNodes++;  // Increment total number of explored nodes counter

            // Check if the current node is the goal node
            if(currentNode.x == this.GoalPositionX && currentNode.y == this.GoalPositionY)
            {
                solution = this.CalculatePath(currentNode);
                return true;
            }

            // Add current to  closed so we dont re-expand it
            this.Closed.AddToClosed(currentNode);

            currentNode.status = NodeStatus.Closed;
            this.grid.SetGridObject(currentNode.x, currentNode.y, currentNode);

            //Handle the neighbours/children with something like this

            foreach (var neighbourNode in GetNeighbourList(currentNode))
            {
                // Check whether the neighbour is walkable
                if(neighbourNode.isWalkable)
                    this.ProcessChildNode(currentNode, neighbourNode);
            }

            // Search a total of Nodes per frame (amount specified by NodesPerSearch)
            if(totalNodesSearched >= this.NodesPerSearch)
            {
                solution = null;

                if (returnPartialSolution)
                {
                    solution = this.CalculatePath(currentNode);
                }
                return false;
            }
            else
            {
                return this.Search(out solution, returnPartialSolution, ++totalNodesSearched);
            }
        }


        protected virtual void ProcessChildNode(NodeRecord parentNode, NodeRecord neighbourNode)
        {
            //this is where you process a child node 
            var child = this.GenerateChildNodeRecord(parentNode, neighbourNode);

            
            foreach (NodeRecord open in this.Open.All())
            {
                if(open.x == child.x && open.y == child.y)
                {

                    // Child is in open
                    if (open.CompareTo(child) == 1)
                    {

                        this.Open.Replace(open, child);
                        child.status = NodeStatus.Open;
                        this.grid.SetGridObject(child.x, child.y, child);
                    }

                    return;
                }
            }

            foreach (NodeRecord closed in this.Closed.All())
            {
                if (closed.x == child.x && closed.y == child.y)
                {
                    // Child is in closed
                    if (closed.CompareTo(child) == 1)
                    {
                        this.Closed.RemoveFromClosed(closed);
                        this.Open.AddToOpen(child);

                        child.status = NodeStatus.Open;
                        this.grid.SetGridObject(child.x, child.y, child);
                    }

                    return;
                }
            }

            // Child is neither in closed nor open
            this.Open.AddToOpen(child);
            child.status = NodeStatus.Open;
            this.grid.SetGridObject(child.x, child.y, child);
        }


        protected virtual NodeRecord GenerateChildNodeRecord(NodeRecord parent, NodeRecord neighbour)
        {
            var childNodeRecord = new NodeRecord(neighbour.x, neighbour.y)
            {
                parent = parent,
                gCost = parent.gCost + CalculateDistanceCost(parent, neighbour),
                hCost = this.Heuristic.H(neighbour, this.GoalNode),
                NodeIndex = neighbour.NodeIndex
            };

            childNodeRecord.CalculateFCost();

            return childNodeRecord;
        }

        //Retrieve all the neighbours possible optimization here
        protected List<NodeRecord> GetNeighbourList(NodeRecord currentNode)
        {
            List<NodeRecord> neighbourList = new List<NodeRecord>();

            if (currentNode.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
                //Left down
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                //Left up
                if (currentNode.y + 1 < grid.getHeight())
                    neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
            if (currentNode.x + 1 < grid.getWidth())
            {
                // Right
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
                //Right down
                if (currentNode.y - 1 >= 0)
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                //Right up
                if (currentNode.y + 1 < grid.getHeight())
                    neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
            // Down
            if (currentNode.y - 1 >= 0)
                neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
            //Up
            if (currentNode.y + 1 < grid.getHeight())
                neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

            return neighbourList;
        }


        public NodeRecord GetNode(int x, int y)
        {
            return grid.GetGridObject(x, y);
        }

        private int CalculateDistanceCost(NodeRecord a, NodeRecord b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }


        public List<NodeRecord> CalculatePath(NodeRecord endNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();
            path.Add(endNode);
            NodeRecord currentNode = endNode;
            //Go through the list of nodes from the end to the beggining
            while (currentNode.parent != null)
            {
                path.Add(currentNode.parent);
                currentNode = currentNode.parent;

            }
            //the list is reversed
            path.Reverse();
            return path;
        }
    }
}
