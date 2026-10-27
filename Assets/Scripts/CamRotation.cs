using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

/// Rotates and drifts the camera toward a tracked target for a cinematic follow effect.
public class CamRotation : MonoBehaviour
{
    public Transform target;
    public float speed;

    // Keeps the camera pointed at the target while moving sideways for the orbit effect.
    void Update()
    {
        Vector3 direction = target.position - transform.position;
        
        Quaternion rotate = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotate, speed * Time.deltaTime);

        transform.position -= transform.right * speed * Time.deltaTime; 
        
    }
}
