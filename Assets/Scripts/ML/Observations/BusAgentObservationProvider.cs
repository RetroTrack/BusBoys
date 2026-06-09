using BusBoys.Assets.Scripts.Sensors.Camera;
using BusBoys.Assets.Scripts.Sensors.Common;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.UI.DefaultControls;

namespace BusBoys.Assets.Scripts.ML.Observations
{
    public class BusAgentObservationProvider : AgentObservationProvider
    {
        [SerializeField] List<IObservationSource> sources;

        public override void CollectObservations(VectorSensor sensor)
        {
            foreach (var source in sources)
            {
                source.Collect(sensor);
            }







            //// Lidar (TODO: make more like a parking sensor (detect in angle instead of single ray) (10 sensors)
            //foreach (var hit in controller.lidarSensor.hits)
            //{
            //    sensor.AddObservation(hit.normalizedDistance);
            //}

            //// Stoplight: red, yellow, green or null (3 sensors)
            //TrafficLightDetector.StoplightState state = controller.trafficLightDetector.CurrentStopLightState;
            //sensor.AddObservation(state == TrafficLightDetector.StoplightState.Red ? 1f : 0f);
            //sensor.AddObservation(state == TrafficLightDetector.StoplightState.Yellow ? 1f : 0f);
            //sensor.AddObservation(state == TrafficLightDetector.StoplightState.Green ? 1f : 0f);

            //// Next path node: direction + distance (3 sensors)
            //if (targetPathNode != null)
            //{
            //    Vector3 toNode = targetPathNode.Position - transform.position;
            //    // Direction in local space so it's relative to the bus's heading
            //    Vector3 localDir = transform.InverseTransformDirection(toNode.normalized);

            //    sensor.AddObservation(localDir.x);          // left/right component
            //    sensor.AddObservation(localDir.z);          // forward/back component            
            //    sensor.AddObservation(Mathf.Clamp01(toNode.magnitude / maxNodeDistance)); // Normalise distance
            //}
            //else
            //{
            //    sensor.AddObservation(0f);
            //    sensor.AddObservation(0f);
            //    sensor.AddObservation(0f);
            //}

            //// Next bus stop: direction + distance, same as above but for next stop instead of node (3 sensors)
            //Transform nextStop = routeManager.PeekNextBusStop();
            //if (nextStop != null)
            //{
            //    Vector3 toStop = nextStop.position - transform.position;
            //    Vector3 localStopDir = transform.InverseTransformDirection(toStop.normalized);
            //    sensor.AddObservation(localStopDir.x);
            //    sensor.AddObservation(localStopDir.z);
            //    sensor.AddObservation(Mathf.Clamp01(toStop.magnitude / maxStopDistance));
            //}
            //else
            //{
            //    sensor.AddObservation(0f);
            //    sensor.AddObservation(0f);
            //    sensor.AddObservation(0f);
            //}





            // Afstand vanaf het midden van de weg (min is links, plus is rechts)
        }
    }
}
