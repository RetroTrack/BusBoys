using BusBoys.Assets.Scripts.Core.Pathfinding;
using BusBoys.Assets.Scripts.ML.Navigation;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Observations
{

    public class NavigationObservationSource : MonoBehaviour, IObservationSource
    {
        [SerializeField] private NavigationTracker navigationTracker;
        [SerializeField] private RouteNavigator routeNavigator;

        [SerializeField] private Transform vehicleTransform;

        public void Collect(VectorSensor sensor)
        {
            CollectTargetNode(sensor);
            CollectNextWaypoint(sensor);
        }

        private void CollectTargetNode(VectorSensor sensor)
        {
            if (navigationTracker.TargetPathNode != null)
            {
                Vector3 toNode =
                    navigationTracker.TargetPathNode.Position -
                    vehicleTransform.position;

                Vector3 localDir =
                    vehicleTransform.InverseTransformDirection(
                        toNode.normalized);

                sensor.AddObservation(localDir.x);
                sensor.AddObservation(localDir.z);

                sensor.AddObservation(
                    Mathf.Clamp01(
                        toNode.magnitude / navigationTracker.MaxNodeDistance));
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        private void CollectNextWaypoint(VectorSensor sensor)
        {
            Transform waypoint =
                routeNavigator.PeekCurrentWaypoint();

            if (waypoint != null)
            {
                Vector3 toWaypoint =
                    waypoint.position -
                    vehicleTransform.position;

                Vector3 localDir =
                    vehicleTransform.InverseTransformDirection(
                        toWaypoint.normalized);

                sensor.AddObservation(localDir.x);
                sensor.AddObservation(localDir.z);

                sensor.AddObservation(
                    Mathf.Clamp01(
                        toWaypoint.magnitude / navigationTracker.MaxWaypointSpan));
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
    }
}
