using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class BusAgent : Agent
{
    [Header("References")]
    [SerializeField] private BusRouteManager routeManager;
    [SerializeField] private BusController controller;

    [Header("Navigation Settings")]
    [Tooltip("Distance at which the agent considers a path node reached")]
    [SerializeField] private float nodeReachedDistance = 3f;
    [Tooltip("Distance at which the agent considers a bus stop reached")]
    [SerializeField] private float stopReachedDistance = 2f;

    [Header("Reward Settings")]
    [SerializeField] private float rewardForReachingStop = 5f;

    InputAction moveAction;
    InputAction brakeAction;



    private RoadNode targetPathNode; // Cached node for observations
    private float maxNodeDistance; // Used for normalising distance to node in observations
    private float maxStopDistance; // Used for normalising distance to stop in observations

    protected override void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        brakeAction = InputSystem.actions.FindAction("Jump");

        moveAction.Enable();
        brakeAction.Enable();
    }

    public void FixedUpdate()
    {
        AdvanceAlongPath();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Lidar (TODO: make more like a parking sensor (detect in angle instead of single ray) (10 sensors)
        foreach (var hit in controller.lidarSensor.hits)
        {
            sensor.AddObservation(hit.normalizedDistance);
        }

        // Stoplight: red, yellow, green or null (3 sensors)
        TrafficLightDetector.StoplightState state = controller.trafficLightDetector.CurrentStopLightState;
        sensor.AddObservation(state == TrafficLightDetector.StoplightState.Red ? 1f : 0f);
        sensor.AddObservation(state == TrafficLightDetector.StoplightState.Yellow ? 1f : 0f);
        sensor.AddObservation(state == TrafficLightDetector.StoplightState.Green ? 1f : 0f);

        // Next path node: direction + distance (3 sensors)
        if (targetPathNode != null)
        {
            Vector3 toNode = targetPathNode.transform.position - transform.position;
            // Direction in local space so it's relative to the bus's heading
            Vector3 localDir = transform.InverseTransformDirection(toNode.normalized);

            sensor.AddObservation(localDir.x);          // left/right component
            sensor.AddObservation(localDir.z);          // forward/back component            
            sensor.AddObservation(Mathf.Clamp01(toNode.magnitude / maxNodeDistance)); // Normalise distance
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // Next bus stop: direction + distance, same as above but for next stop instead of node (3 sensors)
        Transform nextStop = routeManager.PeekNextBusStop();
        if (nextStop != null)
        {
            Vector3 toStop = nextStop.position - transform.position;
            Vector3 localStopDir = transform.InverseTransformDirection(toStop.normalized);
            sensor.AddObservation(localStopDir.x);
            sensor.AddObservation(localStopDir.z);
            sensor.AddObservation(Mathf.Clamp01(toStop.magnitude / maxStopDistance));
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }





        // Afstand vanaf het midden van de weg (min is links, plus is rechts)



    }

    // Checks through path nodes and triggers ArriveAtStop when the bus stop is reached
    private void AdvanceAlongPath()
    {
        // Advance to next road node if close enough
        if (targetPathNode != null)
        {
            float distToNode = Vector3.Distance(transform.position, targetPathNode.transform.position);
            if (distToNode <= nodeReachedDistance)
                targetPathNode = routeManager.GetNextPathNode(); // grab next node in path (or null if at end)

            // Check if the end of the path puts us at the bus stop
            if (!routeManager.HasReachedEndOfPath())
            {
                return;
            }
            Transform nextStop = routeManager.PeekNextBusStop();
            if (nextStop == null)
            {
                return;
            }
            float distToStop = Vector3.Distance(transform.position, nextStop.position);
            if (distToStop > stopReachedDistance)
            {
                return;
            }
            routeManager.ArriveAtStop(); // advances stop index + re-pathfinds
            targetPathNode = routeManager.GetNextPathNode(); // grab first node of new path
            AddReward(rewardForReachingStop); // reward for reaching a stop
        }
    }
    private float CalculateMaxNodeDistance()
    {
        float max = 0f;
        foreach (var node in FindObjectsByType<RoadNode>(FindObjectsSortMode.None))
            foreach (var neighbor in node.neighbors)
                max = Mathf.Max(max, Vector3.Distance(node.transform.position, neighbor.transform.position));

        return max > 0f ? max : 50f; // fallback if graph is empty
    }
    private float CalculateMaxStopDistance()
    {
        var stops = routeManager.busStops; // see below
        if (stops == null || stops.Count < 2) return 200f; // fallback

        float max = 0f;
        for (int i = 0; i < stops.Count; i++)
        {
            int next = (i + 1) % stops.Count;
            max = Mathf.Max(max, Vector3.Distance(stops[i].position, stops[next].position));
        }
        return max;
    }

    public override void OnEpisodeBegin()
    {
        maxNodeDistance = CalculateMaxNodeDistance();
        maxStopDistance = CalculateMaxStopDistance();
        targetPathNode = routeManager.GetNextPathNode();
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
