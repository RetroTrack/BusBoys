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

        private void SetRed()
        {
            timer = 0f;
            state = State.Red;

            SetAllRedLights();
        }

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