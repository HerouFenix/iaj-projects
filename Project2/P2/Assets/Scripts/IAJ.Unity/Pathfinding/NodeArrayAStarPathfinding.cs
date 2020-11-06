using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathfinding : AStarPathfinding
    {
        public NodeRecordArray NodeRecords;
        public NodeArrayAStarPathfinding(int width, int height, float cellSize, IHeuristic heuristic, bool tieBreaking) : base(width, height, cellSize, null, null, heuristic, tieBreaking)
        {
            this.TieBreaking = tieBreaking;
        }

        public override void MapPreprocessing()
        {
            // Register Nodes
            List<NodeRecord> nodes = new List<NodeRecord>();
            for (int x = 0; x < grid.getWidth(); x++)
                for (int y = 0; y < grid.getHeight(); y++)
                {
                    //Add records to list
                    NodeRecord node = GetNode(x, y);
                    //node.solveTies = this.TieBreaking;
                    nodes.Add(node);
                }

            this.NodeRecords = new NodeRecordArray(nodes);
        }

        override public void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            this.StartPositionX = startX;
            this.StartPositionY = startY;
            this.GoalPositionX = goalX;
            this.GoalPositionY = goalY;
            this.StartNode = this.NodeRecords.GetNodeRecord(grid.GetGridObject(StartPositionX, StartPositionY));
            this.GoalNode = this.NodeRecords.GetNodeRecord(grid.GetGridObject(GoalPositionX, GoalPositionY));

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            this.InProgress = true;

            this.TotalProcessingTime = 0.0f;
            this.TotalExploredNodes = 0;
            this.MaxOpenNodes = 0;
            this.MaxClosedNodes = 0;
            this.MaxNodeProcessingTime = int.MinValue;
            this.MinNodeProcessingTime = int.MaxValue;
            this.AllNodesProcessingTime = new List<float>();
            this.Fill = 0;

            this.NodeRecords.Initialize();

            this.StartNode.gCost = 0;
            this.StartNode.hCost = this.Heuristic.H(this.StartNode, this.GoalNode);
            this.StartNode.CalculateFCost();

            this.NodeRecords.AddToOpen(this.StartNode);
        }

        override public bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false, int totalNodesSearched = 0)
        {
            float currentTime = Time.realtimeSinceStartup;

            int openSize = this.NodeRecords.CountOpen();
            // Check if our open list is now bigger than our all-time maximum
            if (openSize > this.MaxOpenNodes)
            {
                this.MaxOpenNodes = openSize;
            }

            // Check if we have no more open nodes
            if (openSize == 0)
            {
                solution = null;
                this.AllNodesProcessingTime.Add(Time.realtimeSinceStartup - currentTime);
                return false;
            }

            // CurrentNode is the best one from the Open set, start with that
            var currentNode = this.NodeRecords.GetBestAndRemove();

            this.TotalExploredNodes++;  // Increment total number of explored nodes counter

            // Check if the current node is the goal node
            if (currentNode.x == this.GoalPositionX && currentNode.y == this.GoalPositionY)
            {
                solution = this.CalculatePath(currentNode);
                this.AllNodesProcessingTime.Add(Time.realtimeSinceStartup - currentTime);
                return true;
            }

            // Add current to  closed so we dont re-expand it
            this.NodeRecords.AddToClosed(currentNode);


            currentNode.status = NodeStatus.Closed;
            this.grid.SetGridObject(currentNode.x, currentNode.y, currentNode);

            //Handle the neighbours/children with something like this

            foreach (var neighbourNode in GetNeighbourList(currentNode))
            {
                // Check whether the neighbour is walkable
                if (neighbourNode.isWalkable)
                    this.ProcessChildNode(currentNode, neighbourNode);
            }

            // Search a total of Nodes per frame (amount specified by NodesPerSearch)
            if (totalNodesSearched >= this.NodesPerSearch)
            {
                solution = null;
                this.AllNodesProcessingTime.Add(Time.realtimeSinceStartup - currentTime);

                if (returnPartialSolution)
                {
                    solution = this.CalculatePath(currentNode);
                }
                return false;
            }
            else
            {
                this.AllNodesProcessingTime.Add(Time.realtimeSinceStartup - currentTime);

                return this.Search(out solution, returnPartialSolution, ++totalNodesSearched);
            }
        }

        /*
        override protected void ProcessChildNode(NodeRecord parentNode, NodeRecord neighbourNode)
        {
            //this is where you process a child node 
            var child = this.GenerateChildNodeRecord(parentNode, neighbourNode);

            //child.solveTies = this.TieBreaking;
            var node = this.NodeRecords.GetNodeRecord(child);

            if (node.status == NodeStatus.Open)
            {
                // Child is in open
                if (node.CompareTo(child) == 1)
                {
                    child.status = NodeStatus.Open;
                    this.NodeRecords.Replace(node, child);
                    this.grid.SetGridObject(child.x, child.y, child);
                }

                return;
            }
            else if (node.status == NodeStatus.Closed)
            {
                // Child is in closed
                if (node.CompareTo(child) == 1)
                {
                    this.NodeRecords.RemoveFromClosed(node);
                    child.status = NodeStatus.Open;
                    this.NodeRecords.AddToOpen(child);

                    this.grid.SetGridObject(child.x, child.y, child);
                }

                return;
            }


            // Child is neither in closed nor open
            child.status = NodeStatus.Open;
            this.NodeRecords.AddToOpen(child);
            this.grid.SetGridObject(child.x, child.y, child);
        }
        */

         override protected void ProcessChildNode(NodeRecord parentNode, NodeRecord neighbourNode)
        {
            //this is where you process a child node 
            //var child = this.GenerateChildNodeRecord(parentNode, neighbourNode);
            neighbourNode = this.NodeRecords.GetNodeRecord(neighbourNode);
            parentNode = this.NodeRecords.GetNodeRecord(parentNode);

            var gCost = parentNode.gCost + CalculateDistanceCost(parentNode, neighbourNode);
            var hCost = this.Heuristic.H(neighbourNode, this.GoalNode);
            var fCost = gCost + hCost;

            //child.solveTies = this.TieBreaking;
            var node = this.NodeRecords.GetNodeRecord(neighbourNode);

            if (node.status == NodeStatus.Open)
            {
                // Child is in open
                if (node.fCost > fCost)
                {
                    node.status = NodeStatus.Open;
                    node.gCost = gCost;
                    node.hCost = hCost;
                    node.fCost = fCost;
                    node.parent = parentNode;

                    this.NodeRecords.Replace(node, node);
                    this.grid.SetGridObject(node.x, node.y, node);
                }

                return;
            }
            else if (node.status == NodeStatus.Closed)
            {
                // Child is in closed
                if (node.fCost > fCost)
                {
                    this.NodeRecords.RemoveFromClosed(node);

                    node.status = NodeStatus.Open;
                    this.NodeRecords.AddToOpen(node.NodeIndex, gCost, hCost, fCost, parentNode.NodeIndex);

                    this.grid.SetGridObject(node.x, node.y, node);
                }

                return;
            }


            // Child is neither in closed nor open
            node.status = NodeStatus.Open;
            this.NodeRecords.AddToOpen(node.NodeIndex, gCost, hCost, fCost, parentNode.NodeIndex);
            this.grid.SetGridObject(node.x, node.y, node);
        }
        

    }
}
