using UnityEngine;

/// Acts as a brake-zone trigger for AI vehicles. When entered, the car is forced to reduce speed.
public class BrakingPoints : MonoBehaviour
{
    public float brakingPower = 10f;

    // Activates the AI braking behavior when the trigger is entered.
    private void OnTriggerEnter(Collider other)
    {
        EnemyController car = other.GetComponent<EnemyController>();
        if (car != null)
        {
            car.brakingZone = true;

        }
    }

    // Clears the braking state when the AI leaves the trigger area.
    private void OnTriggerExit(Collider other)
    {
        EnemyController car = other.GetComponent<EnemyController>();
        if (car != null)
        {
            car.brakingZone = false;
        }
    }
}