using BusBoys.Assets.Scripts.Vehicles.Bus;
using System.Collections.Generic;
using UnityEngine;

public class BatteryRenderer : MonoBehaviour
{
    [Header("Battery Percentage Images")]
    [SerializeField] private List<Texture2D> batteryImages; // List to hold the battery percentage images

    [Header("Bus Battery Reference")]
    [SerializeField] private BusBattery busBattery;
    public MeshRenderer targetMesh;

    public void Update()
    {
        if (busBattery == null || targetMesh == null)
        {
            Debug.LogWarning("BusBattery or Target Mesh is not assigned.");
            return;
        }

        if (batteryImages.Count == 0)
        {
            Debug.LogWarning("Battery images list is empty. Please assign the battery percentage images.");
            return;
        }

        int index = Mathf.Clamp(Mathf.FloorToInt((busBattery.batteryPercentage / 100f) * batteryImages.Count), 0, batteryImages.Count - 1);
        targetMesh.material.mainTexture = batteryImages[index];
    }

}