using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Observations
{
    public class BusAgentObservationProvider : AgentObservationProvider
    {
        [SerializeField] List<MonoBehaviour> sources;

        private readonly List<IObservationSource> observationSources = new();

        private void Awake()
        {
            observationSources.Clear();

            foreach (var source in sources)
            {
                if (source is IObservationSource observationSource)
                {
                    observationSources.Add(observationSource);
                }
                else
                {
                    Debug.LogError(
                        $"{source.name} must implement IObservationSource",
                        source);
                }
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            foreach (var source in observationSources)
            {
                source.Collect(sensor);
            }


            // Afstand vanaf het midden van de weg (min is links, plus is rechts)
        }
    }
}
