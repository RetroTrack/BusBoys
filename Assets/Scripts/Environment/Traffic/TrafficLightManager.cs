using System.Collections.Generic;
using UnityEngine;

namespace BusBoys.Assets.Scripts.Environment.Traffic
{
    public class TrafficLightManager : MonoBehaviour
    {
        [System.Serializable]
        public class TrafficLightGroup
        {
            public List<TrafficLight> lights;
        }

        [Header("Traffic Light Groups")]
        [SerializeField] private List<TrafficLightGroup> groups;

        [Header("Timings")]
        [SerializeField] private float greenTime = 10f;
        [SerializeField] private float yellowTime = 2f;
        [SerializeField] private float redTime = 1.5f;

        private int currentIndex = 0;
        private float timer;

        private enum State
        {
            Green,
            Yellow,
            Red
        }

        private State state;

        private void Start()
        {
            SetGreen();
        }

        //Update the light state after the set time.
        private void Update()
        {
            timer += Time.deltaTime;

            switch (state)
            {
                case State.Green:
                    if (timer >= greenTime)
                        SetYellow();
                    break;

                case State.Yellow:
                    if (timer >= yellowTime)
                        SetRed();
                    break;

                case State.Red:
                    if (timer >= redTime)
                    {
                        currentIndex = (currentIndex + 1) % groups.Count;
                        SetGreen();
                    }
                    break;
            }
        }

        //Set the traffic light.
        private void SetGreen()
        {
            timer = 0f;
            state = State.Green;

            SetAllRedLights();

            var group = groups[currentIndex];
            if (group?.lights == null) return;

            foreach (var l in group.lights)
                if (l != null)
                    l.ChangeState(TrafficLight.TrafficLightState.Green);
        }

        //Set traffic light to yellow
        private void SetYellow()
        {
            timer = 0f;
            state = State.Yellow;

            var group = groups[currentIndex];
            if (group?.lights == null) return;

            foreach (var l in group.lights)
                if (l != null)
                    l.ChangeState(TrafficLight.TrafficLightState.Yellow);
        }

        //Set the taffic light to red
        private void SetRed()
        {
            timer = 0f;
            state = State.Red;

            SetAllRedLights();
        }

        //Set all the light to red.
        private void SetAllRedLights()
        {
            foreach (var group in groups)
            {
                if (group?.lights == null) continue;

                foreach (var l in group.lights)
                {
                    if (l != null)
                        l.ChangeState(TrafficLight.TrafficLightState.Red);
                }
            }
        }
    }
}