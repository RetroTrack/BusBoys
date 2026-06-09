using BusBoys.Assets.Scripts.ML.Navigation;
using BusBoys.Assets.Scripts.ML.Observations;
using BusBoys.Assets.Scripts.ML.Rewards;
using BusBoys.Assets.Scripts.Vehicles.Bus;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BusBoys.Assets.Scripts.ML.Agents
{
    public class BusAgent : Agent
    {
        [Header("References")]
        [SerializeField] private Transform startPosition;
        [Space]
        [SerializeField] private NavigationTracker navigationTracker;
        [SerializeField] private BusController controller;

        [Header("Agent Components")]
        [SerializeField] private AgentObservationProvider observationProvider;
        [SerializeField] private AgentRewardProvider rewardProvider;


        InputAction moveAction;
        InputAction brakeAction;

        protected override void Awake()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            brakeAction = InputSystem.actions.FindAction("Jump");

            moveAction.Enable();
            brakeAction.Enable();
        }

        public void Start()
        {
            if(rewardProvider == null)
            {
                Debug.LogError("Reward provider is not assigned in the inspector, reward will not be calculated.");
                return;
            }

            rewardProvider.OnAgentRewarded += AddReward;
            rewardProvider.OnAgentRewardSet += SetReward;
            rewardProvider.OnAgentEpisodeStopped += EndEpisode;

        }

        public void FixedUpdate()
        {
            navigationTracker.AdvanceAlongPath();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            observationProvider.CollectObservations(sensor);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            float motorInput = actions.ContinuousActions[0];
            float brakeInput = actions.ContinuousActions[1];
            float steerInput = actions.ContinuousActions[2];
            controller.SetInputs(motorInput, brakeInput, steerInput);
        }
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var actions = actionsOut.ContinuousActions;

            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            actions[0] = moveInput.y; // Forward/Backward
            actions[1] = brakeAction.ReadValue<float>(); // Brake
            actions[2] = moveInput.x; // Left/Right
        }
        public override void OnEpisodeBegin()
        {
            controller.transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
            controller.ResetVehicle();
            navigationTracker.BeginEpisode();
        }

    }
}