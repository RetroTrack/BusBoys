using UnityEngine;

public class LaadPaal : MonoBehaviour
{
    public float chargeRate = 20f; // % per seconde

    private void OnTriggerStay(Collider other)
    {
        BusController bus = other.GetComponent<BusController>();

        if (bus != null && bus.batteryPercentage < 100)
        {
            bus.batteryPercentage += (chargeRate * Time.deltaTime);
            Debug.Log("Auto laad op!!");
        }
        if (bus != null && bus.batteryPercentage == 100) 
        {
            Debug.Log("Auto is vol!!");
        }
    }
}

//is trigger aanvinken en grootte van de collider aanpassen
//in buscontroller //#region nieuw voor batterij zoeken voor de nieuwe delen die geplakt moeten worden
/*
 *     public float batteryPercentage = 100f;
    private Vector3 lastPosition;
    public float drainPerMeter = 0.001f; // hoeveel % per meter

 en plak dit in accelerate bus 

    if (batteryPercentage <= 0f)
    {
        batteryPercentage = 0f;
        wheelFrontLeft.motorTorque = 0f;
        wheelFrontRight.motorTorque = 0f;
        wheelBackLeft.motorTorque = 0f;
        wheelBackRight.motorTorque = 0f;
        return;
    }

fixed update:
#region nieuw voor batterij
        Vector2 SteeringInput = moveAction.ReadValue<Vector2>();
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, lastPosition); //bewogen afstand bepalem
        if (currentSpeed > 0.5f && SteeringInput.magnitude > 0.1f)
        {
            batteryPercentage -= distance * drainPerMeter;
        }
        lastPosition = currentPosition;
        #endregion
 */