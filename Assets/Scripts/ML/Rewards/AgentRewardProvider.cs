using BusBoys.Assets.Scripts.Configs;
using System;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Rewards
{
    public class AgentRewardProvider : MonoBehaviour
    {
        [SerializeField] public RewardConfig rewardConfig;

        public event Action<float> OnAgentRewarded;
        public event Action<float> OnAgentRewardSet;
        public event Action OnAgentEpisodeStopped;
        public void AddReward(float reward) => OnAgentRewarded?.Invoke(reward);
        public void SetReward(float reward) => OnAgentRewardSet?.Invoke(reward);
        public void EndEpisode() => OnAgentEpisodeStopped?.Invoke();
    }
}
