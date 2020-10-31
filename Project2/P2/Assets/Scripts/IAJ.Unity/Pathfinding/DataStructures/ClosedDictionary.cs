using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class ClosedDictionary : IClosedSet
    {
        private Dictionary<Vector2,NodeRecord> NodeRecords { get; set; }

        public ClosedDictionary()
        {
            this.NodeRecords = new Dictionary<Vector2, NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }

        public int CountClosed()
        {
            return this.NodeRecords.Count;
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            Vector2 key = new Vector2(nodeRecord.x, nodeRecord.y);
            this.NodeRecords.Add(key,nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(new Vector2(nodeRecord.x, nodeRecord.y));
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            Vector2 key = new Vector2(nodeRecord.x, nodeRecord.y);
            if(!this.NodeRecords.ContainsKey(key))
            {
                return null;
            }

            return this.NodeRecords[key];
        }

        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords.Values.ToList();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            this.NodeRecords[new Vector2(nodeToBeReplaced.x, nodeToBeReplaced.y)] = nodeToReplace;
        }
    }
}
