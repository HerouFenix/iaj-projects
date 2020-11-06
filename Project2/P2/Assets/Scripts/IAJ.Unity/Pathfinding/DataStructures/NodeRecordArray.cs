using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

            for (int i = 0; i < nodes.Count; i++)
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
                return this.NodeRecords[node.NodeIndex];
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
                this.NodeRecords[i].parent = null;
                this.NodeRecords[i].gCost = int.MaxValue;
                this.NodeRecords[i].fCost = int.MaxValue;
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
            nodeRecord.status = NodeStatus.Open;
            this.NodeRecords[nodeRecord.NodeIndex] = nodeRecord;

            Open.AddToOpen(nodeRecord);
        }

        public void AddToOpen(int index, float gCost, float hCost, float fCost, int parentIndex)
        {
            var nodeToUpdate = this.NodeRecords[index];
            nodeToUpdate.status = NodeStatus.Open;
            nodeToUpdate.gCost = gCost;
            nodeToUpdate.hCost = hCost;
            nodeToUpdate.fCost = fCost;
            nodeToUpdate.parent = this.NodeRecords[parentIndex];

            //nodeRecord.status = NodeStatus.Open;
            //this.NodeRecords[nodeRecord.NodeIndex] = nodeRecord;

            Open.AddToOpen(this.NodeRecords[index]);
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            //nodeRecord.status = NodeStatus.Closed;
            //this.NodeRecords[nodeRecord.NodeIndex] = nodeRecord;

            var nodeToUpdate = this.NodeRecords[nodeRecord.NodeIndex];
            nodeToUpdate.status = NodeStatus.Closed;
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

            var nodeToUpdate = this.NodeRecords[node.NodeIndex];
            nodeToUpdate.status = NodeStatus.Closed;

            return this.NodeRecords[node.NodeIndex];
        }

        public NodeRecord PeekBest()
        {
            return Open.PeekBest();
        }

        public NodeRecord GetBestAndRemoveTieBreaking()
        {
            NodeRecord node = Open.GetBestAndRemove();

            var nodeToUpdate = this.NodeRecords[node.NodeIndex];
            nodeToUpdate.status = NodeStatus.Closed;

            return this.NodeRecords[node.NodeIndex];
        }

        public NodeRecord PeekBestTieBreaking()
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
            foreach (var node in this.NodeRecords)
            {
                if (node.status == NodeStatus.Closed)
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

        public int CountClosed()
        {
            int i = 0;
            foreach (NodeRecord node in this.NodeRecords)
            {
                if (node.status == NodeStatus.Closed)
                {
                    i++;
                }
            }
            return i;
        }
    }
}
