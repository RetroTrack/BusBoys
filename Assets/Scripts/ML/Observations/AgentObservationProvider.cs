using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Observations
{
    public abstract class AgentObservationProvider : MonoBehaviour
    {
        public abstract void CollectObservations(VectorSensor sensor);
    }
}
