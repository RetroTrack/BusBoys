using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    /// <summary>
    /// Lightweight IGraphNode wrapper to pass through the world position of the node.
    /// </summary>
    public class WaypointGraphNode : IGraphNode
    {
        private Vector3 position;
        public IReadOnlyList<IGraphNode> Neighbors { get; } = new List<IGraphNode>();
        public float NodeReachedDistance => 2f;
        public Vector3 Position => position;

        public WaypointGraphNode(Vector3 position)
        {
            this.position = position;
        }
    }
}
