using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Graph
{
    public static class GraphNodeHelper
    {
        /// <summary>
        /// True if this node is non-null AND, if it is a Unity Object (like NavNode),
        /// has not been destroyed. Use this instead of a plain `== null` check anywhere
        /// an IGraphNode might be backed by a destroyed MonoBehaviour.
        /// </summary>
        public static bool IsAlive(this IGraphNode node)
        {
            if (node is null) return false;
            if (node is Object unityObj) return unityObj != null; // Unity's overloaded null check
            return true; // non-Unity-Object implementations (e.g. WaypointGraphNode) are always "alive"
        }
    }
}