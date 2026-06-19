using BusBoys.Assets.Scripts.Core.Graph;
using BusBoys.Assets.Scripts.Vehicles.Bus.Electric;
using BusBoys.Assets.Scripts.Vehicles.Common;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BusBoys
{
    public class ChangeCamera : MonoBehaviour
    {
        [SerializeField] private List<Camera> cameras = new();

        private int currentCameraIndex = 0;

        private void Start()
        {
            SetActiveCamera(currentCameraIndex);
        }

        public void SwitchCamera()
        {
            if (cameras.Count <= 1)
                return;

            currentCameraIndex++;

            if (currentCameraIndex >= cameras.Count)
                currentCameraIndex = 0;

            SetActiveCamera(currentCameraIndex);
        }

        private void SetActiveCamera(int index)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].gameObject.SetActive(i == index);
            }
        }
    }



}