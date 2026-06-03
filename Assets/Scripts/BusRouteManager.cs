using System.Collections.Generic;
using UnityEngine;

public class BusRouteManager : MonoBehaviour
{
    [SerializeField] List<Transform> busStops = new List<Transform>();
    int currentStopIndex = 0;

    public void ArriveAtStop()
    {
        Debug.Log("Arrived at stop: " + busStops[currentStopIndex]);
        currentStopIndex++;
    }

    public Vector3 GetDistanceToStop(Transform targetStop)
    {
        if (busStops.Count == 0)
            return Vector3.zero;

        Transform currentStop = busStops[currentStopIndex];
        return targetStop.position - currentStop.position;
    }

    public Vector3 GetDirectionToStop(Transform targetStop)
    {
        if (busStops.Count == 0)
            return Vector3.zero;
        Transform currentStop = busStops[currentStopIndex];
        Vector3 direction = targetStop.position - currentStop.position;
        return direction.normalized;
    }

    public Transform GetNextBusStop()
    {
        if (busStops.Count == 0)
            return null;
        if(currentStopIndex >= busStops.Count)
            currentStopIndex = 0; // Reset index if it exceeds the lists
        Transform nextStop = busStops[currentStopIndex];
        currentStopIndex = (currentStopIndex + 1) % busStops.Count; // Loop back to the first stop after reaching the last one
        return nextStop;
    }

}
