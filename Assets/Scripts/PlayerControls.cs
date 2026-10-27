using UnityEngine;
using System.Collections.Generic;

/// Reads player input and keeps the human-controlled car aligned with the race track.
public class PlayerControls : MonoBehaviour
{
    public WaypointContainer waypointContainer;
    public CarController carController;
    public LapController lapController;
    public int currentWaypoint, lastWaypointHit;
    public List<Transform> waypoints;
    public float topSpeed, wait = 10f, waypointRange;

    // Cache the car and track waypoint list during startup.
    void Start()
    {
        carController = GetComponent<CarController>();
        waypoints = waypointContainer.waypoints;
    }

    // Handles acceleration, steering, braking, and waypoint tracking while the race is active.
    void Update()
    {
        if (lapController != null && lapController.IsRacerFinished(carController))
        {
            return;
        }

        if (!lapController.startTimer)
        {
        
            float verticalInput = Input.GetAxis("Vertical");
            float currentSpeed = carController.carRigidbody.linearVelocity.magnitude;
            carController.currentAcceleration = carController.acceleration * verticalInput;
            carController.currentTurnAngle = carController.maxTurnAngle * Input.GetAxis("Horizontal");

            if (currentSpeed >= 1f && Input.GetKey(KeyCode.S))
            {
                carController.currentBrakingForce = carController.brakingForce;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                carController.currentBrakingForce = carController.brakingForce;
            }
            else
            {
                carController.currentBrakingForce = 0f;
            }
        }
        if (Vector3.Distance(waypoints[currentWaypoint].position, transform.position) < waypointRange)
        {
            lastWaypointHit = currentWaypoint;
            currentWaypoint++;
            if (currentWaypoint == waypoints.Count)
                currentWaypoint = 0;
        }
        if (carController.currentSpeed <= 10f)
            {
                wait -= Time.deltaTime;
                if (wait <= 0f)
                {
                    int lastWaypoint = Mathf.Clamp(lastWaypointHit, 0, waypoints.Count - 1);
                    Transform destination = waypoints[lastWaypoint];
                    transform.SetPositionAndRotation(destination.position, destination.rotation);
                    carController.SetInput(0, 0, 0, 0);
                    currentWaypoint = lastWaypoint;
                    wait = 10f;
                }
            }
        else
        {
            wait = 10f;
        }
    }

    // Resets the player car if it hits a hazard or obstacle trigger.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OnVehicle"))
        {
            int lastWaypoint = Mathf.Clamp(lastWaypointHit, 0, waypoints.Count - 1);
            Transform destination = waypoints[lastWaypoint];
            transform.SetPositionAndRotation(destination.position, destination.rotation);
            currentWaypoint = lastWaypoint;
            carController.SetInput(0, 0, 0, 0);
            wait = 10f;
        }
    }
}
