using BusBoys.Assets.Scripts.Core.Graph;
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
        [SerializeField] private Rigidbody vehicleRigidbody;

        public void Collect(VectorSensor sensor)
        {
            CollectPathNode(sensor, offset: 0);  // current target node
            CollectPathNode(sensor, offset: 1);  // lookahead node
            CollectVelocity(sensor);
        }

        /// <summary>
        /// Adds direction + normalised distance for the path node at
        /// (CurrentPathIndex + offset).  Emits zeros when the node doesn't exist.
        /// </summary>
        private void CollectPathNode(VectorSensor sensor, int offset)
        {
            IGraphNode node = routeNavigator.PeekPathNode(offset);

            if (node.IsAlive())
            {
                Vector3 toNode = node.Position - vehicleRigidbody.position;

                Vector3 localDir =
                    vehicleRigidbody.transform.InverseTransformDirection(toNode.normalized);

                sensor.AddObservation(localDir.x);
                sensor.AddObservation(localDir.y);
                sensor.AddObservation(localDir.z);
                sensor.AddObservation(
                    Mathf.Clamp01(toNode.magnitude / navigationTracker.MaxNodeDistance));
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        private void CollectVelocity(VectorSensor sensor)
        {
            Vector3 localVelocity =
                vehicleRigidbody.transform.InverseTransformDirection(
                    vehicleRigidbody.linearVelocity);

            sensor.AddObservation(localVelocity.x);
            sensor.AddObservation(localVelocity.y);
            sensor.AddObservation(localVelocity.z);
        }
    }
}