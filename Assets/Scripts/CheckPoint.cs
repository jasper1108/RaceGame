using Unity.VisualScripting;
using UnityEngine;

/// Marks a checkpoint on the track and reports it to the lap controller when a racer passes through.
public class CheckPoint : MonoBehaviour
{
    public LapController lapController;

    // Notifies the race manager that a vehicle crossed this checkpoint.
    void OnTriggerEnter(Collider other)
    {
        CarController racer = other.GetComponentInParent<CarController>();
        if (racer != null && lapController != null)
        {
            lapController.RegisterCheckpoint(racer);
        }
    }
}
