using BusBoys.Assets.Scripts.Core.Graph;
using System.Collections.Generic;
using System.Linq;
using BusBoys.Assets.Scripts.ML.Rewards;
using UnityEngine;

namespace BusBoys
{
    public class TrafficLightTrigger : MonoBehaviour
    {
        [SerializeField] private AgentRewardProvider rewardProvider;
        [SerializeField] private GameObject environment;

        void Start()
        {
            if (environment == null)
            {
                Debug.LogError("Environment reference is not set in the inspector.");
                return;
            }
            Initialize(environment);
        }

        public void Initialize(GameObject env)
        {
            rewardProvider = env.GetComponentInChildren<AgentRewardProvider>();
        }
        private void OnTriggerEnter(Collider other)
        {
            rewardProvider.AddReward(rewardProvider.rewardConfig.drivingThroughRedLightPenalty, "Driven through red light");
            Debug.Log($"Bus drove through traffic light! Hit by: {other.gameObject.name}");
        }
    }
}