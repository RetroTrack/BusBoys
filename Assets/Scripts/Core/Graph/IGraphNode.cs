using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    //Interface definition for IGraphNode. 
    public interface IGraphNode
    {
        Vector3 Position { get; }
        IReadOnlyList<IGraphNode> Neighbors { get; }
        float NodeReachedDistance { get; }
    }
}
