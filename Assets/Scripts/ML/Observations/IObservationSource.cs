using Unity.MLAgents.Sensors;

namespace BusBoys.Assets.Scripts.ML.Observations
{
    public interface IObservationSource
    {
        void Collect(VectorSensor sensor);
    }
}
