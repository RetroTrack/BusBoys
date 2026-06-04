using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class BusAgent : Agent
{
    [SerializeField] private BusRouteManager routeManager;
    [SerializeField] private BusController controller;

    InputAction moveAction;
    InputAction brakeAction;

    protected override void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        brakeAction = InputSystem.actions.FindAction("Jump");

        moveAction.Enable();
        brakeAction.Enable();
    }   
    public override void CollectObservations(VectorSensor sensor)
    {
        // Lidar
        foreach (var hit in controller.lidarSensor.hits)
        {
            sensor.AddObservation(hit.normalizedDistance);
        }
        // Stoplicht: Red yellow green of null
        TrafficLightDetector.StoplightState state = controller.trafficLightDetector.CurrentStopLightState;
        sensor.AddObservation(state == TrafficLightDetector.StoplightState.Red ? 1f : 0f);
        sensor.AddObservation(state == TrafficLightDetector.StoplightState.Yellow ? 1f : 0f);
        sensor.AddObservation(state == TrafficLightDetector.StoplightState.Green ? 1f : 0f);

        // Astar route???????
        // Afstand vanaf het midden van de weg (min is links, plus is rechts)


        // Afstand tot volgende stop
        // Richting tot volgende stop
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float motorInput = actions.ContinuousActions[0];
        float brakeInput = actions.ContinuousActions[1];
        float steerInput = actions.ContinuousActions[2];
        controller.ControlBus(motorInput, brakeInput, steerInput);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        actions[0] = moveInput.y; // Forward/Backward
        actions[1] = brakeAction.ReadValue<float>(); // Brake
        actions[2] = moveInput.x; // Left/Right
    }

}
