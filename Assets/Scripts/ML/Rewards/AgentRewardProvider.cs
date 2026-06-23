using BusBoys.Assets.Scripts.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusBoys.Assets.Scripts.ML.Rewards
{
    public class AgentRewardProvider : MonoBehaviour
    {
        [SerializeField] public RewardConfig rewardConfig;

        public event Action<float> OnAgentRewarded;
        public event Action<float> OnAgentRewardSet;
        public event Action OnAgentEpisodeStopped;
        public Dictionary<string, float> RewardReasons { get; private set; } = new();
        public void AddReward(float reward, string reason = "")
        {
            Debug.Log($"Reward added: {(reward > 1f ? "High" : "Low")} ({reason})");
            if (RewardReasons.ContainsKey(reason))
            {
                RewardReasons[reason] += reward;
            }
            else
            {
                RewardReasons.Add(reason, reward);
            }
            OnAgentRewarded?.Invoke(reward);
        }
        public void SetReward(float reward, string reason = "")
        {
            Debug.Log($"Reward set: {(reward > 1f ? "High" : "Low")} ({reason})");

            RewardReasons.Clear();
            RewardReasons.Add(reason, reward);

            OnAgentRewardSet?.Invoke(reward);
        }
        public void EndEpisode() => OnAgentEpisodeStopped?.Invoke();

        public void OnEpisodeBegin()
        {
            Debug.Log($"Episode began, rewards of last episode: {string.Join(", ", RewardReasons.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
            RewardReasons.Clear();
        }
    }
}
