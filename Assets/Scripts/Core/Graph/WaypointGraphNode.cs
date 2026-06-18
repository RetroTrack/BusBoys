using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    /// <summary>
    /// Lichtgewicht IGraphNode wrapper om een wereldpositie als node door te geven.
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
