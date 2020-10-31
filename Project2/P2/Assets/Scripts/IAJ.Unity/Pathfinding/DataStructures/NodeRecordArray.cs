using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class NodeRecordArray : IOpenSet, IClosedSet
    {
        private NodeRecord[] NodeRecords { get; set; }
        private List<NodeRecord> SpecialCaseNodes { get; set; } 
        private NodePriorityHeap Open { get; set; }

        public NodeRecordArray(List<NodeRecord> nodes)
        {
            //this method creates and initializes the NodeRecordArray for all nodes in the Navigation Graph
            this.NodeRecords = new NodeRecord[nodes.Count];
            
            for(int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                this.NodeRecords[i] = node;
            }

            this.SpecialCaseNodes = new List<NodeRecord>();

            this.Open = new NodePriorityHeap();
        }

        public NodeRecord GetNodeRecord(NodeRecord node)
        {
            //do not change this method
            //here we have the "special case" node handling
            if (node.NodeIndex == -1)
            {
                for (int i = 0; i < this.SpecialCaseNodes.Count; i++)
                {
                    if (node == this.SpecialCaseNodes[i])
                    {
                        return this.SpecialCaseNodes[i];
                    }
                }
                return null;
            }
            else
            {
                return  this.NodeRecords[node.NodeIndex];
            }
        }

        public void AddSpecialCaseNode(NodeRecord node)
        {
            this.SpecialCaseNodes.Add(node);
        }

        public void Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

            this.SpecialCaseNodes.Clear();
        }

        void IOpenSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

            this.SpecialCaseNodes.Clear();
        }

        void IClosedSet.Initialize()
        {
            return;
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            var nodeToUpdate = this.NodeRecords.SingleOrDefault(x => x.NodeIndex == nodeRecord.NodeIndex);
            nodeToUpdate.status = NodeStatus.Open;
            nodeToUpdate.gCost = nodeRecord.gCost;
            nodeToUpdate.hCost = nodeRecord.hCost;
            nodeToUpdate.fCost = nodeRecord.fCost;
            nodeToUpdate.parent = nodeRecord.parent;

            Open.AddToOpen(nodeRecord);
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            var nodeToUpdate = this.NodeRecords.SingleOrDefault(x => x.NodeIndex == nodeRecord.NodeIndex);
            nodeToUpdate.status = NodeStatus.Closed;
            nodeToUpdate.gCost = nodeRecord.gCost;
            nodeToUpdate.hCost = nodeRecord.hCost;
            nodeToUpdate.fCost = nodeRecord.fCost;
            nodeToUpdate.parent = nodeRecord.parent;

        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            NodeRecord node = this.NodeRecords[nodeRecord.NodeIndex];
            if (node.status == NodeStatus.Open)
            {
                return node;
            }

            return null;
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            NodeRecord node = this.NodeRecords[nodeRecord.NodeIndex];
            if (node.status == NodeStatus.Closed)
            {
                return node;
            }

            return null;
        }

        public NodeRecord GetBestAndRemove()
        {
            NodeRecord node = Open.GetBestAndRemove();

            var nodeToUpdate = this.NodeRecords.SingleOrDefault(x => x.NodeIndex == node.NodeIndex);
            nodeToUpdate.status = NodeStatus.Closed;

            return this.NodeRecords[node.NodeIndex];
        }

        public NodeRecord PeekBest()
        {
            return Open.PeekBest();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            this.NodeRecords[nodeToBeReplaced.NodeIndex] = nodeToReplace;

            this.Open.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            var nodeToUpdate = this.NodeRecords.SingleOrDefault(x => x.NodeIndex == nodeRecord.NodeIndex);
            nodeToUpdate.status = NodeStatus.Closed;

            this.Open.RemoveFromOpen(nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            var nodeToUpdate = this.NodeRecords.SingleOrDefault(x => x.NodeIndex == nodeRecord.NodeIndex);
            nodeToUpdate.status = NodeStatus.Open;
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
            return this.Open.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            List<NodeRecord> closedNodes = new List<NodeRecord>();
            foreach(var node in this.NodeRecords)
            {
                if(node.status == NodeStatus.Closed)
                {
                    closedNodes.Add(node);
                }
            }
            return closedNodes;
        }

        public int CountOpen()
        {
            return this.Open.CountOpen();
        }
    }
}
