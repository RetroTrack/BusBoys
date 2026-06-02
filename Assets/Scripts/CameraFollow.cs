using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // The target the camera will follow
    [SerializeField] private Vector3 offset; // The offset from the target's position
    [SerializeField] private bool followRotation = false; // Whether the camera should also follow the target's rotation

    // Update is called once per frame
    void Update()
    {

        if (followRotation)
        {
            transform.position = target.position + target.rotation * offset;
            transform.LookAt(target.position);
        }
        else
        {
            transform.position = target.position + offset;
        }
    }
}
