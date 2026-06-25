using Unity.MLAgents.Sensors;

namespace BusBoys.Assets.Scripts.ML.Observations
{
    public interface IObservationSource
    {
        //Interface for collecting.
        void Collect(VectorSensor sensor);
    }
}
