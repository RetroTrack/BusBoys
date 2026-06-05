using System.Collections.Generic;
using UnityEngine;

public class BusRouteManager : MonoBehaviour
{
    public List<Transform> busStops = new List<Transform>();
    [SerializeField] RoadGraph roadGraph;

    int currentStopIndex = 0;

    // The full sequence of RoadNodes to follow to reach the next stop
    public List<RoadNode> currentPath { get; private set; } = new List<RoadNode>();
    public int CurrentPathIndex { get; private set; } = 0;

    public float GetNodeReachedDistance()
    {
        if (currentPath == null || CurrentPathIndex >= currentPath.Count)
            return 0f;
        RoadNode nextNode = currentPath[CurrentPathIndex];
        return nextNode.nodeReachedDistance;
    }

    // Call this to continue to the next node in the path, returns null if at the end
    public RoadNode GetNextPathNode()
    {
        if (currentPath == null || CurrentPathIndex >= currentPath.Count)
            return null;

        return currentPath[CurrentPathIndex++];
    }

    public bool HasReachedEndOfPath()
    {
        return currentPath == null || CurrentPathIndex >= currentPath.Count;
    }

    // Call when bus arrives at a stop to update the current stop and pathfind to the next one
    public void ArriveAtStop(Vector3 busPosition, Vector3 busFacing)
    {
        Debug.Log("Arrived at stop: " + busStops[currentStopIndex]);
        currentStopIndex = (currentStopIndex + 1) % busStops.Count;

        // Pathfind from where the bus actually is, not from the stop position
        PathfindFromPosition(busPosition, busFacing);
    }

    // Allows pathfinding from an vector position (like the bus's position) to the current stop
    public void PathfindFromPosition(Vector3 fromPosition, Vector3? facingDirection = null)
    {
        if (busStops.Count == 0 || roadGraph == null) return;

        Transform to = busStops[currentStopIndex];

        RoadNode startNode = FindClosestNode(fromPosition);
        RoadNode goalNode = FindClosestNode(to.position);

        if (startNode == null || goalNode == null)
        {
            Debug.LogWarning("Could not find road nodes near start or stop.");
            return;
        }

        currentPath = roadGraph.FindPath(startNode, goalNode, facingDirection);
        CurrentPathIndex = 0;
    }

    RoadNode FindClosestNode(Vector3 position)
    {
        RoadNode closest = null;
        float bestDist = float.MaxValue;

        foreach (var node in FindObjectsByType<RoadNode>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    // Helper methods to get distance and direction to a stop from the current stop, useful for bus agent training

    // Returns the vector from the current stop to the target stop
    public Vector3 GetDistanceToStop(Transform targetStop)
    {
        if (busStops.Count == 0) return Vector3.zero;
        Transform currentStop = busStops[currentStopIndex];
        return targetStop.position - currentStop.position;
    }
    // Returns the normalized direction vector from the current stop to the target stop
    public Vector3 GetDirectionToStop(Transform targetStop)
    {
        if (busStops.Count == 0) return Vector3.zero;
        Transform currentStop = busStops[currentStopIndex];
        return (targetStop.position - currentStop.position).normalized;
    }
    // Returns the next stop in the route and advances the stop index, returns null if no stops, loops back to the first stop after reaching the end
    public Transform GetNextBusStop()
    {
        if (busStops.Count == 0) return null;
        if (currentStopIndex >= busStops.Count) currentStopIndex = 0;
        Transform nextStop = busStops[currentStopIndex];
        currentStopIndex = (currentStopIndex + 1) % busStops.Count;
        return nextStop;
    }
    // Returns the next stop in the route without advancing the stop index, returns null if no stops
    public Transform PeekNextBusStop()
    {
        if (busStops.Count == 0) return null;
        int peekIndex = currentStopIndex % busStops.Count;
        return busStops[peekIndex];
    }
}