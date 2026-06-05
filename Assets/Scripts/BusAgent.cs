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
    [Tooltip("Distance at which the agent considers a bus stop reached")]
    [SerializeField] private float stopReachedDistance = 2f;
    [SerializeField] private float wrongDirectionAngleThreshold = 120f; // degrees
    [SerializeField] private float wrongDirectionCheckDistance = 5f; // only check if node is within this distance
    [SerializeField] private float recalculateCooldown = 2f; // prevent spam recalculating
    private float lastRecalculateTime = -999f;

    [Header("Reward Settings")]
    [SerializeField] private float rewardForReachingStop = 10f;
    [SerializeField] private float rewardForReachingNode = 0.3f;
    [SerializeField] private float recalculationPenalty = -2f;
    [SerializeField] private float collisionPenalty = -10f;
    [SerializeField] private float ghostDrivingPenalty = -0.1f;
    [SerializeField] private float drivingThroughRedLightPenalty = -5f;


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
        if (targetPathNode == null)
        {
            if (!routeManager.HasReachedEndOfPath())
                targetPathNode = routeManager.GetNextPathNode();
            return;
        }

        Vector3 toNode = targetPathNode.transform.position - transform.position;
        float distToNode = toNode.magnitude;

        // Advance to next node if close enough
        if (distToNode <= routeManager.GetNodeReachedDistance())
        {
            AddReward(rewardForReachingNode);
            targetPathNode = routeManager.GetNextPathNode();
            return;
        }

        // Wrong direction check
        float angle = Vector3.Angle(transform.forward, toNode);
        bool isFacingWrongWay = angle > wrongDirectionAngleThreshold;
        bool cooldownExpired = Time.time - lastRecalculateTime > recalculateCooldown;

        if (isFacingWrongWay && cooldownExpired)
        {
            Debug.Log($"Bus facing wrong way ({angle:F0}°), recalculating...");
            lastRecalculateTime = Time.time;
            AddReward(recalculationPenalty);

            routeManager.PathfindFromPosition(transform.position, transform.forward);
            targetPathNode = routeManager.GetNextPathNode();
            return;
        }

        // Check if bus has reached the bus stop
        Transform nextStop = routeManager.PeekNextBusStop();
        if (nextStop != null)
        {
            float distToStop = Vector3.Distance(transform.position, nextStop.position);
            if (distToStop <= stopReachedDistance)
            {
                Debug.Log("Arrived at stop, pathfinding to next...");
                routeManager.ArriveAtStop(transform.position, transform.forward);
                targetPathNode = routeManager.GetNextPathNode();
                AddReward(rewardForReachingStop);
                return;
            }
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

        // Pathfind from bus position instead of consuming a node immediately
        routeManager.PathfindFromPosition(transform.position, transform.forward);
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
