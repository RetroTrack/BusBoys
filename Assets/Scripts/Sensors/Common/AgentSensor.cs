using UnityEngine;

namespace BusBoys.Assets.Scripts.Sensors.Common
{
    public abstract class AgentSensor : MonoBehaviour
    {
        public abstract float[] Observations { get; }
        public abstract float UpdateInterval { get; set; }

    }
}
