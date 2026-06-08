using System;
using UnityEditor.EditorTools;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    [Header("Traffic Light State")]
    [SerializeField] private TrafficLightState currentState = TrafficLightState.Red;

    [Header("Traffic Light Objects")]
    [SerializeField] private MeshRenderer[] redLights;
    [SerializeField] private MeshRenderer[] yellowLights;
    [SerializeField] private MeshRenderer[] greenLights;

    [Header("Traffic Light Materials")]
    [SerializeField] private Material redLightOffMaterial;
    [SerializeField] private Material yellowLightOffMaterial;
    [SerializeField] private Material greenLightOffMaterial;
    [SerializeField] private Material redLightOnMaterial;
    [SerializeField] private Material yellowLightOnMaterial;
    [SerializeField] private Material greenLightOnMaterial;

    public void Start()
    {
        UpdateTrafficLight();
    }

    public void ChangeState(TrafficLightState newState)
    {
        currentState = newState;
        UpdateTrafficLight();
    }

    public TrafficLightState GetCurrentState()
    {
        return currentState;
    }

    private void UpdateTrafficLight()
    {
        foreach (var light in redLights)
        {
            light.material = currentState == TrafficLightState.Red ? redLightOnMaterial : redLightOffMaterial;
        }
        foreach (var light in yellowLights)
        {
            light.material = currentState == TrafficLightState.Yellow ? yellowLightOnMaterial : yellowLightOffMaterial;
        }
        foreach (var light in greenLights)
        {
            light.material = currentState == TrafficLightState.Green ? greenLightOnMaterial : greenLightOffMaterial;
        }
    }

    [ContextMenu("Change Traffic Light State")]
    public void ChangeToNextState()
    {
        switch (currentState)
        {
            case TrafficLightState.Red:
                ChangeState(TrafficLightState.Green);
                break;
            case TrafficLightState.Yellow:
                ChangeState(TrafficLightState.Red);
                break;
            case TrafficLightState.Green:
                ChangeState(TrafficLightState.Yellow);
                break;
        }
    }

    public enum TrafficLightState
    {
        Red,
        Yellow,
        Green
    }
}
