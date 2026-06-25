using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public static class GraphNodeHelper
    {
        //Check if object is still existing within the scene.
        public static bool IsAlive(this IGraphNode node)
        {
            if (node is null) return false;
            if (node is Object unityObj) return unityObj != null; // Unity's overloaded null check
            return true; // non-Unity-Object implementations (e.g. WaypointGraphNode) are always "alive"
        }
    }
}