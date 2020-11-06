using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class JPSPlusPathfinding : AStarPathfinding
    {
        public NodeRecordArray NodeRecords;
        public Dictionary<string, string[]> ValidLookupTable;
        public float CellSize;

        public JPSPlusPathfinding(int width, int height, float cellSize, IHeuristic heuristic, bool tieBreaking) : base(width, height, cellSize, null, null, heuristic, tieBreaking)
        {
            // Valid Directions
            this.ValidLookupTable = new Dictionary<string, string[]>();
            this.ValidLookupTable.Add("S", new string[] { "W", "SW", "S", "SE", "E" });
            this.ValidLookupTable.Add("SE", new string[] { "S", "SE", "E" });
            this.ValidLookupTable.Add("E", new string[] { "S", "SE", "E", "NE", "N" });
            this.ValidLookupTable.Add("NE", new string[] { "E", "NE", "N" });
            this.ValidLookupTable.Add("N", new string[] { "E", "NE", "N", "NW", "W" });
            this.ValidLookupTable.Add("NW", new string[] { "N", "NW", "W" });
            this.ValidLookupTable.Add("W", new string[] { "N", "NW", "W", "SW", "S" });
            this.ValidLookupTable.Add("SW", new string[] { "W", "SW", "S" });

            this.CellSize = cellSize;
            this.TieBreaking = tieBreaking;
        }

        override public void MapPreprocessing()
        {
            // Register Nodes
            registerNodes();

            // Sweep
            sweepLeftRight(); //Right to Left and Left to Right
            sweepUpDown(); //Up to Down and Down to Up
            sweepUpDiagonally(); // DownRight and DownLeft
            sweepDownDiagonally(); // UpRight and UpLeft
        }


        private void checkJumpPoint(NodeRecord node)
        {
            NodeRecord left = null;
            NodeRecord leftDown = null;
            NodeRecord down = null;
            NodeRecord rightDown = null;
            NodeRecord right = null;
            NodeRecord rightUp = null;
            NodeRecord up = null;
            NodeRecord leftUp = null;

            if (node.x - 1 >= 0)
            {
                // Left
                left = GetNode(node.x - 1, node.y);
                //Left down
                if (node.y - 1 >= 0)
                {
                    leftDown = GetNode(node.x - 1, node.y - 1);
                }
                //Left up
                if (node.y + 1 < grid.getHeight())
                {
                    leftUp = GetNode(node.x - 1, node.y + 1);
                }
            }

            if (node.x + 1 < grid.getWidth())
            {
                // Right
                right = GetNode(node.x + 1, node.y);
                //Right down
                if (node.y - 1 >= 0)
                {
                    rightDown = GetNode(node.x + 1, node.y - 1);
                }
                //Right up
                if (node.y + 1 < grid.getHeight())
                {
                    rightUp = GetNode(node.x + 1, node.y + 1);
                }
            }
            // Down
            if (node.y - 1 >= 0)
            {
                down = GetNode(node.x, node.y - 1);
            }
            //Up
            if (node.y + 1 < grid.getHeight())
            {
                up = GetNode(node.x, node.y + 1);
            }

            // Check forced neighbours

            // Left Up Corner
            if (leftUp != null && !leftUp.isWalkable)
            {
                if ((left != null && left.isWalkable) && (up != null && up.isWalkable))
                {
                    node.directions[3] = true;
                    node.directions[0] = true;
                }
            }

            // Right Up Corner
            if (rightUp != null && !rightUp.isWalkable)
            {
                if ((right != null && right.isWalkable) && (up != null && up.isWalkable))
                {
                    node.directions[2] = true;
                    node.directions[0] = true;
                }
            }

            // Left Down Corner
            if (leftDown != null && !leftDown.isWalkable)
            {
                if ((left != null && left.isWalkable) && (down != null && down.isWalkable))
                {
                    node.directions[3] = true;
                    node.directions[1] = true;
                }
            }

            // Right Down Corner
            if (rightDown != null && !rightDown.isWalkable)
            {
                if ((down != null && down.isWalkable) && (right != null && right.isWalkable))
                {
                    node.directions[2] = true;
                    node.directions[1] = true;
                }
            }
        }

        private void registerNodes()
        {
            // Register Nodes
            List<NodeRecord> nodes = new List<NodeRecord>();

            for (int x = 0; x < grid.getWidth(); x++)
                for (int y = 0; y < grid.getHeight(); y++)
                {
                    //Add records to list
                    var currentNode = GetNode(x, y);
                    //currentNode.solveTies = this.TieBreaking;
                    checkJumpPoint(currentNode);
                    nodes.Add(currentNode);
                }

            this.NodeRecords = new NodeRecordArray(nodes);

        }

        private void sweepLeftRight()
        {
            for (int r = 0; r < grid.getHeight(); ++r)
            {
                //SWEEP RIGHT
                {
                    int countMovingLeft = -1;
                    bool jumpPointLastSeen = false;

                    for (int c = 0; c < grid.getWidth(); ++c)
                    {
                        var currentNode = this.NodeRecords.GetNodeRecord(GetNode(c, r));

                        if (!currentNode.isWalkable)
                        {
                            countMovingLeft = -1;
                            jumpPointLastSeen = false;
                            currentNode.distances["W"] = 0;

                            //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later
                            continue;
                        }

                        countMovingLeft++;

                        if (jumpPointLastSeen)
                        {
                            currentNode.distances["W"] = countMovingLeft;
                        }
                        else
                        {
                            currentNode.distances["W"] = -countMovingLeft;
                        }

                        //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later

                        if (currentNode.directions[2])
                        {
                            countMovingLeft = 0;
                            jumpPointLastSeen = true;
                        }
                    }
                }

                //SWEEP LEFT
                {
                    int countMovingRight = -1;
                    bool jumpPointLastSeen = false;

                    for (int c = grid.getWidth() - 1; c >= 0; --c)
                    {
                        var currentNode = this.NodeRecords.GetNodeRecord(GetNode(c, r));

                        if (!currentNode.isWalkable)
                        {
                            countMovingRight = -1;
                            jumpPointLastSeen = false;
                            currentNode.distances["E"] = 0;

                            //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later
                            continue;
                        }

                        countMovingRight++;

                        if (jumpPointLastSeen)
                        {
                            currentNode.distances["E"] = countMovingRight;
                        }
                        else
                        {
                            currentNode.distances["E"] = -countMovingRight;
                        }

                        //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later

                        if (currentNode.directions[3])
                        {
                            countMovingRight = 0;
                            jumpPointLastSeen = true;
                        }
                    }
                }
            }
        }

        private void sweepUpDown()
        {
            for (int c = 0; c < grid.getWidth(); ++c)
            {
                //Sweep UP
                {
                    int countMovingUp = -1;
                    bool jumpPointLastSeen = false;

                    for (int r = 0; r < grid.getHeight(); ++r)
                    {
                        var currentNode = this.NodeRecords.GetNodeRecord(GetNode(c, r));

                        if (!currentNode.isWalkable)
                        {
                            countMovingUp = -1;
                            jumpPointLastSeen = false;
                            currentNode.distances["S"] = 0;

                            //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later
                            continue;
                        }

                        countMovingUp++;

                        if (jumpPointLastSeen)
                        {
                            currentNode.distances["S"] = countMovingUp;
                        }
                        else
                        {
                            currentNode.distances["S"] = -countMovingUp;
                        }

                        //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later

                        if (currentNode.directions[0])
                        {
                            countMovingUp = 0;
                            jumpPointLastSeen = true;
                        }
                    }
                }

                //Sweep DOWN
                {
                    int countMovingDown = -1;
                    bool jumpPointLastSeen = false;

                    for (int r = grid.getHeight() - 1; r >= 0; --r)
                    {
                        var currentNode = this.NodeRecords.GetNodeRecord(GetNode(c, r));

                        if (!currentNode.isWalkable)
                        {
                            countMovingDown = -1;
                            jumpPointLastSeen = false;
                            currentNode.distances["N"] = 0;

                            //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later
                            continue;
                        }

                        countMovingDown++;

                        if (jumpPointLastSeen)
                        {
                            currentNode.distances["N"] = countMovingDown;
                        }
                        else
                        {
                            currentNode.distances["N"] = -countMovingDown;
                        }

                        //this.NodeRecords.Replace(currentNode, currentNode); // Might be unecessary check later

                        if (currentNode.directions[1])
                        {
                            countMovingDown = 0;
                            jumpPointLastSeen = true;
                        }
                    }
                }
            }
        }

        private void sweepUpDiagonally()
        {
            for (int r = 0; r < grid.getHeight(); ++r)
            {
                for (int c = 0; c < grid.getWidth(); ++c)
                {
                    var currentNode = this.NodeRecords.GetNodeRecord(GetNode(c, r));

                    if (currentNode.isWalkable)
                    {
                        bool incrementFromLast = true;

                        // SOUTHWEST
                        if (r == 0 || c == 0 || (!GetNode(c, r - 1).isWalkable || !GetNode(c - 1, r).isWalkable || !GetNode(c - 1, r - 1).isWalkable))
                        {
                            // Wall one away
                            currentNode.distances["SW"] = 0;
                            incrementFromLast = false;

                        }
                        else if (GetNode(c, r - 1).isWalkable && GetNode(c - 1, r).isWalkable)
                        {
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c - 1, r - 1));

                            if (testNode.distances["S"] > 0 || testNode.distances["W"] > 0)
                            {
                                // Diagonal one away
                                currentNode.distances["SW"] = 1;
                                incrementFromLast = false;
                            }
                        }

                        if (incrementFromLast)
                        {
                            // Increment from last
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c - 1, r - 1));

                            int jumpDistance = testNode.distances["SW"];

                            if (jumpDistance > 0)
                            {
                                currentNode.distances["SW"] = 1 + jumpDistance;
                            }
                            else
                            {
                                currentNode.distances["SW"] = -1 + jumpDistance;
                            }
                        }
                        incrementFromLast = true;

                        // SOUTHEAST
                        if (r == 0 || c == 0 || (!GetNode(c, r - 1).isWalkable || !GetNode(c + 1, r).isWalkable || !GetNode(c + 1, r - 1).isWalkable))
                        {
                            // Wall one away
                            currentNode.distances["SE"] = 0;
                            incrementFromLast = false;
                        }
                        else if (GetNode(c, r - 1).isWalkable && GetNode(c + 1, r).isWalkable)
                        {
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c + 1, r - 1));

                            if (testNode.distances["S"] > 0 || testNode.distances["E"] > 0)
                            {
                                // Diagonal one away
                                currentNode.distances["SE"] = 1;
                                incrementFromLast = false;
                            }

                        }

                        if (incrementFromLast)
                        {
                            // Increment from last
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c + 1, r - 1));

                            int jumpDistance = testNode.distances["SE"];

                            if (jumpDistance > 0)
                            {
                                currentNode.distances["SE"] = 1 + jumpDistance;
                            }
                            else
                            {
                                currentNode.distances["SE"] = -1 + jumpDistance;
                            }
                        }
                    }
                }
            }
        }

        private void sweepDownDiagonally()
        {
            for (int r = grid.getHeight() - 1; r >= 0; --r)
            {
                for (int c = 0; c < grid.getWidth(); ++c)
                {
                    var currentNode = this.NodeRecords.GetNodeRecord(GetNode(c, r));

                    if (currentNode.isWalkable)
                    {
                        bool incrementFromLast = true;

                        // NORTHWEST
                        if (r == grid.getHeight() || c == 0 || (!GetNode(c, r + 1).isWalkable || !GetNode(c - 1, r).isWalkable || !GetNode(c - 1, r + 1).isWalkable))
                        {
                            // Wall one away
                            currentNode.distances["NW"] = 0;
                            incrementFromLast = false;
                        }
                        else if (GetNode(c, r + 1).isWalkable && GetNode(c - 1, r).isWalkable)
                        {
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c - 1, r + 1));

                            if (testNode.distances["N"] > 0 || testNode.distances["W"] > 0)
                            {
                                // Diagonal one away
                                currentNode.distances["NW"] = 1;
                                incrementFromLast = false;
                            }

                        }
                        if (incrementFromLast)
                        {
                            // Increment from last
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c - 1, r + 1));

                            int jumpDistance = testNode.distances["NW"];

                            if (jumpDistance > 0)
                            {
                                currentNode.distances["NW"] = 1 + jumpDistance;
                            }
                            else
                            {
                                currentNode.distances["NW"] = -1 + jumpDistance;
                            }
                        }

                        incrementFromLast = true;


                        // NORTHEAST
                        if (r == grid.getHeight() || c == 0 || (!GetNode(c, r + 1).isWalkable || !GetNode(c + 1, r).isWalkable || !GetNode(c + 1, r + 1).isWalkable))
                        {
                            // Wall one away
                            currentNode.distances["NE"] = 0;
                            incrementFromLast = false;
                        }
                        else if (GetNode(c, r + 1).isWalkable && GetNode(c + 1, r).isWalkable)
                        {
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c + 1, r + 1));

                            if (testNode.distances["N"] > 0 || testNode.distances["E"] > 0)
                            {
                                // Diagonal one away
                                currentNode.distances["NE"] = 1;
                                incrementFromLast = false;
                            }

                        }
                        if (incrementFromLast)
                        {
                            // Increment from last
                            NodeRecord testNode = this.NodeRecords.GetNodeRecord(GetNode(c + 1, r + 1));

                            int jumpDistance = testNode.distances["NE"];

                            if (jumpDistance > 0)
                            {
                                currentNode.distances["NE"] = 1 + jumpDistance;
                            }
                            else
                            {
                                currentNode.distances["NE"] = -1 + jumpDistance;
                            }
                        }
                    }
                }
            }
        }

        override public void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
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
            this.MaxClosedNodes = 0;
            this.MaxNodeProcessingTime = int.MinValue;
            this.MinNodeProcessingTime = int.MaxValue;
            this.AllNodesProcessingTime = new List<float>();
            this.Fill = 0;

            var initialNode = this.NodeRecords.GetNodeRecord(StartNode);

            this.NodeRecords.Initialize();
            initialNode.gCost = 0;
            initialNode.hCost = this.Heuristic.H(this.StartNode, this.GoalNode);

            initialNode.CalculateFCost();

            this.NodeRecords.AddToOpen(initialNode);
        }

        private float DiagonalDistance(NodeRecord a, NodeRecord b)
        {
            var x = (b.x - a.x);
            var y = (b.y - a.y);
            return Mathf.Sqrt((x * x + y * y));
        }

        private int rowDistance(NodeRecord a, NodeRecord b)
        {
            return Mathf.Abs(b.y - a.y);
        }

        private int colDistance(NodeRecord a, NodeRecord b)
        {
            return Mathf.Abs(b.x - a.x);
        }

        private bool isCardinal(string direction)
        {
            switch (direction)
            {
                case "N":
                    return true;
                case "S":
                    return true;
                case "E":
                    return true;
                case "W":
                    return true;
                default:
                    return false;
            }
        }

        private bool inGeneralDirection(NodeRecord start, string direction)
        {
            switch (direction)
            {
                case "NE":
                    if ((this.GoalNode.x > start.x && this.GoalNode.y > start.y))
                    {

                        return true;
                    }
                    break;
                case "NW":
                    if (this.GoalNode.x < start.x && this.GoalNode.y > start.y)
                    {
                        return true;
                    }
                    break;
                case "SW":
                    if (this.GoalNode.x < start.x && this.GoalNode.y < start.y)
                    {
                        return true;
                    }
                    break;
                case "SE":
                    if (this.GoalNode.x > start.x && this.GoalNode.y < start.y)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

        private bool inExactDirection(NodeRecord node, string direction)
        {
            switch (direction)
            {
                case "N":
                    if (this.GoalNode.x == node.x && this.GoalNode.y > node.y)
                        return true;
                    break;
                case "S":
                    if (this.GoalNode.x == node.x && this.GoalNode.y < node.y)
                        return true;
                    break;
                case "E":
                    if (this.GoalNode.x > node.x && this.GoalNode.y == node.y)
                        return true;
                    break;
                case "W":
                    if (this.GoalNode.x < node.x && this.GoalNode.y == node.y)
                        return true;
                    break;
                default:
                    return false;
            }

            return false;
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
            this.grid.SetGridObject(currentNode.x, currentNode.y, currentNode);

            float givenCost = 0.0f;

            this.TotalExploredNodes++;  // Increment total number of explored nodes counter

            // Check if the current node is the goal node
            if (currentNode.x == this.GoalPositionX && currentNode.y == this.GoalPositionY)
            {
                this.AllNodesProcessingTime.Add(Time.realtimeSinceStartup - currentTime);
                solution = this.CalculatePath(currentNode);
                return true;
            }

            var parentNode = currentNode.parent;

            // Check which direction we're coming from
            string[] possibleDirections;
            if (parentNode == null)
            { // Initially we can go in any direction
                possibleDirections = new string[] { "N", "S", "E", "W", "SW", "NW", "SE", "NE" };
            }
            else
            {
                possibleDirections = this.ValidLookupTable[currentNode.travelingDirection];
            }

            // Iterate over all possible traveling directions
            foreach (var direction in possibleDirections)
            {
                NodeRecord newSuccessor = null;

                if (isCardinal(direction) &&
                    inExactDirection(currentNode, direction) &&
                    DiagonalDistance(currentNode, this.GoalNode) <= Mathf.Abs(currentNode.distances[direction])
                   )
                {
                    // Goal is closer than wall distance or closer than (or equal to) Jump Point Distance
                    newSuccessor = this.GoalNode;
                    givenCost = currentNode.gCost + (MOVE_STRAIGHT_COST * DiagonalDistance(currentNode, this.GoalNode));
                }
                else if ( //Diagonals
                       !isCardinal(direction) &&
                       inGeneralDirection(currentNode, direction) &&
                       (
                        (rowDistance(currentNode, this.GoalNode) <= Mathf.Abs(currentNode.distances[direction])) ||
                        (colDistance(currentNode, this.GoalNode) <= Mathf.Abs(currentNode.distances[direction]))
                       )
                     )
                {
                    // Goal is closer or equal in either row or column than wall or jump point distance
                    // Create a target jump point
                    int minDiff = Mathf.Min(rowDistance(currentNode, this.GoalNode), colDistance(currentNode, this.GoalNode));

                    var newX = currentNode.x;
                    var newY = currentNode.y;

                    switch (direction)
                    {
                        case "NE":
                            newX += minDiff;
                            newY += minDiff;
                            break;
                        case "NW":
                            newX -= minDiff;
                            newY += minDiff;
                            break;
                        case "SE":
                            newX += minDiff;
                            newY -= minDiff;
                            break;
                        case "SW":
                            newX -= minDiff;
                            newY -= minDiff;
                            break;
                    }

                    newSuccessor = this.NodeRecords.GetNodeRecord(GetNode(newX, newY));
                    givenCost = currentNode.gCost + (MOVE_DIAGONAL_COST * minDiff);

                }
                else if (currentNode.distances[direction] > 0)
                {
                    // Jump point in this direction
                    var jumpPointX = currentNode.x;
                    var jumpPointY = currentNode.y;
                    switch (direction)
                    {
                        case "N":
                            jumpPointY += currentNode.distances[direction];
                            break;
                        case "S":
                            jumpPointY -= currentNode.distances[direction];
                            break;
                        case "W":
                            jumpPointX -= currentNode.distances[direction];
                            break;
                        case "E":
                            jumpPointX += currentNode.distances[direction];
                            break;
                        case "NE":
                            jumpPointX += currentNode.distances[direction];
                            jumpPointY += currentNode.distances[direction];
                            break;
                        case "NW":
                            jumpPointX -= currentNode.distances[direction];
                            jumpPointY += currentNode.distances[direction];
                            break;
                        case "SE":
                            jumpPointX += currentNode.distances[direction];
                            jumpPointY -= currentNode.distances[direction];
                            break;
                        case "SW":
                            jumpPointX -= currentNode.distances[direction];
                            jumpPointY -= currentNode.distances[direction];
                            break;
                    }
                    newSuccessor = this.NodeRecords.GetNodeRecord(GetNode(jumpPointX, jumpPointY));

                    givenCost = DiagonalDistance(currentNode, newSuccessor);
                    if (isCardinal(direction))
                    {
                        givenCost *= MOVE_STRAIGHT_COST;
                    }
                    else
                    {
                        givenCost *= MOVE_DIAGONAL_COST;
                    }

                    givenCost += currentNode.gCost;
                }

                if (newSuccessor != null)
                {
                    this.ProcessSuccessorNode(currentNode, newSuccessor, direction, givenCost);
                }
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

        protected void ProcessSuccessorNode(NodeRecord parentNode, NodeRecord neighbourNode, string direction, float givenCost)
        {
            //this is where you process a child node 
            var child = new NodeRecord(neighbourNode.x, neighbourNode.y)
            {
                parent = parentNode,
                gCost = givenCost,
                //gCost = parentNode.gCost + CalculateDistanceCost(parentNode, neighbourNode),
                hCost = this.Heuristic.H(neighbourNode, this.GoalNode),
                NodeIndex = neighbourNode.NodeIndex,
                directions = neighbourNode.directions,
                distances = neighbourNode.distances,
                travelingDirection = direction
                //solveTies = this.TieBreaking
            };

            child.CalculateFCost();

            var node = this.NodeRecords.GetNodeRecord(child);

            //if (node.status != NodeStatus.Open && node.status != NodeStatus.Closed)
            //{
            //    child.status = NodeStatus.Open;
            //    this.NodeRecords.AddToOpen(child);
            //    this.grid.SetGridObject(child.x, child.y, child);
            //}
            //else if (givenCost < node.gCost)
            //{
            //    child.status = NodeStatus.Open;
            //    this.NodeRecords.Replace(node, child);
            //    this.grid.SetGridObject(child.x, child.y, child);
            //}

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
                    this.NodeRecords.AddToOpen(child);

                    child.status = NodeStatus.Open;
                    this.grid.SetGridObject(child.x, child.y, child);
                }

                return;
            }


            // Child is neither in closed nor open
            child.status = NodeStatus.Open;
            this.NodeRecords.AddToOpen(child);
            this.grid.SetGridObject(child.x, child.y, child);
        }



        override public List<NodeRecord> CalculatePath(NodeRecord endNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();
            path.Add(endNode);
            NodeRecord currentNode = endNode;

            uint i = 1;

            //Go through the list of nodes from the end to the beggining
            while (currentNode.parent != null)
            {
                i++;

                NodeRecord intermediateNode = currentNode;
                while (intermediateNode.x != currentNode.parent.x || intermediateNode.y != currentNode.parent.y)
                {
                    int intermediateNodeX = intermediateNode.x;
                    int intermediateNodeY = intermediateNode.y;
                    switch (currentNode.travelingDirection)
                    {
                        case "N":
                            intermediateNodeY -= 1;
                            break;
                        case "S":
                            intermediateNodeY += 1;
                            break;
                        case "W":
                            intermediateNodeX += 1;
                            break;
                        case "E":
                            intermediateNodeX -= 1;
                            break;
                        case "NE":
                            intermediateNodeX -= 1;
                            intermediateNodeY -= 1;
                            break;
                        case "NW":
                            intermediateNodeX += 1;
                            intermediateNodeY -= 1;
                            break;
                        case "SE":
                            intermediateNodeX -= 1;
                            intermediateNodeY += 1;
                            break;
                        case "SW":
                            intermediateNodeX += 1;
                            intermediateNodeY += 1;
                            break;
                    }

                    intermediateNode = this.NodeRecords.GetNodeRecord(GetNode(intermediateNodeX, intermediateNodeY));
                    path.Add(intermediateNode);

                }
                //path.Add(currentNode.parent);
                currentNode = currentNode.parent;

            }

            this.Fill = this.TotalExploredNodes - i;
            //the list is reversed
            path.Reverse();
            return path;
        }

    }

}
