using UnityEngine;

namespace BusBoys
{
    public class Crossing : MonoBehaviour
    {
        [SerializeField] Transform StartPoint;
        [SerializeField] Transform EndPoint;

        [SerializeField] Transform passerby;
        float passerbySpeed = 4.2f;
        float passerbyOdds = 0.2f; //20%
        float SetTime = 60;//elke minuut
        float passerbyTimer;

        bool crossing = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            passerby.gameObject.SetActive(false);
            passerbyTimer = SetTime;
        }

        // Update is called once per frame
        void Update()
        {   if (crossing == false)
            {
                passerbyTimer -= Time.deltaTime;
                if (passerbyTimer < 0)
                {
                    passerbyTimer = SetTime;
                    float chance = Random.value;
                    if (chance <= passerbyOdds)
                    {
                        crossing = true;
                        passerby.gameObject.SetActive(true);
                        Vector3 startPos = StartPoint.position;
                        startPos.y = 0f;
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
