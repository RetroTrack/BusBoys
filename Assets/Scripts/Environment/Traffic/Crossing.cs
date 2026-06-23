using UnityEngine;

namespace BusBoys
{
    public class Crossing : MonoBehaviour
    {
        [SerializeField] Transform StartPoint;
        [SerializeField] Transform EndPoint;

        [SerializeField] Transform passerby;
        [SerializeField] float passerbySpeed = 2.6f;
        public float passerbyOdds = 0.2f; //20%
        float SetTime = 30;
        float passerbyTimer;
        float setOffsetTime;
        bool crossing = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            setOffsetTime = Random.Range(5f, 120f); //5 tot 120 s
            passerby.gameObject.SetActive(false);
            passerbyTimer = setOffsetTime;
        }

        // Update is called once per frame
        //Randomly spawns and moves pedestrians accross the crossing prefab.
        void Update()
        {   
            if (crossing == false)
            {
                passerbyTimer -= Time.deltaTime;
                if (passerbyTimer < 0)
                {
                    passerbyTimer = SetTime;
                    float chance = Random.value; //generated value 0-1
                    if (chance <= passerbyOdds)
                    {
                        crossing = true;
                        passerby.gameObject.SetActive(true);
                        Vector3 startPos = StartPoint.position;
                        startPos.y += 0.5f;
                        passerby.position = startPos;
                    }
                }
            }
            else if (crossing)
            {
                passerby.position = Vector3.MoveTowards(
                passerby.position,
                EndPoint.position,
                passerbySpeed * Time.deltaTime);

                if (Vector3.Distance(passerby.position, EndPoint.position) < 0.1f) 
                {
                    crossing = false;
                    passerby.gameObject.SetActive(false);
                }
            }
        }
    }
}
