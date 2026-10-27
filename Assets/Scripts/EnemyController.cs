using System.Collections.Generic;
using UnityEngine;

/// Drives the AI car through the course by steering toward the next waypoint and braking when needed.
public class EnemyController : MonoBehaviour
{
    public WaypointContainer waypointContainer;
    public LapController lapController;
    public CarController carController;
    private BrakingPoints brakingPoints;
    public GameObject brakeLight;
    public List<Transform> waypoints;
    public int currentWaypoint, lastWaypointHit;
    public float waypointRange;
    public float gasInput, currentAngle, startDelay;
    public float topSpeed = 150f, maximumAngle = 35f;
    public bool brakingZone;
    float wait = 10f;

    // Cache the car reference and the waypoint list for path following.
    void Start()
    {
        carController = GetComponent<CarController>();
        waypoints = waypointContainer.waypoints;
    }

    // Follow the race path and react to braking zones after the countdown ends.
    void Update()
    {
        if (lapController != null && lapController.IsRacerFinished(carController))
        {
            return;
        }

        if (!lapController.startTimer)
        {
            startDelay -= Time.deltaTime;
            if (startDelay <= 0f)
            {
                if (Vector3.Distance(waypoints[currentWaypoint].position, transform.position) < waypointRange)
                {
                    lastWaypointHit = currentWaypoint;
                    currentWaypoint++;
                    if (currentWaypoint == waypoints.Count)
                        currentWaypoint = 0;
                }
                Vector3 fwd = transform.TransformDirection(Vector3.forward);
                currentAngle = Vector3.SignedAngle(fwd, waypoints[currentWaypoint].position - transform.position, Vector3.up);
                carController.SetInput(1,currentAngle, 0, 0);
                gasInput = Mathf.Clamp01(maximumAngle - Mathf.Abs(carController.currentSpeed * 0.01f * currentAngle) / maximumAngle);
                if (brakingZone)
                {
                    brakeLight.SetActive(true);
                    float brakeInput = brakingPoints != null ? brakingPoints.brakingPower : 1f;
                    carController.SetInput(0, currentAngle, brakeInput, 0);
                }
                else
                {   
                    brakeLight.SetActive(false);
                    carController.SetInput(1, currentAngle, 0, 0);
                }
                if (brakingZone && carController.currentSpeed < 30f)
                {
                    brakingZone = false;
                }
                if (carController.currentSpeed <= 10f)
                {
                    wait -= Time.deltaTime;
                    if (wait <= 0f)
                    {
                        int lastWaypoint = Mathf.Clamp(lastWaypointHit, 0, waypoints.Count - 1);
                        Transform destination = waypoints[lastWaypoint];
                        transform.SetPositionAndRotation(destination.position, destination.rotation);
                        currentWaypoint = lastWaypoint;
                        carController.SetInput(0, 0, 0, 0);
                        wait = 10f;
                    }
                }
                else
                {
                    wait = 10f;
                }
            }
        }
        Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position);
    }

    // Triggered when the AI vehicle enters a braking zone or hazard trigger.
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("AI"))
        {
            brakingZone = true;
        }
        else
        {
            BrakingPoints point = other.GetComponent<BrakingPoints>();
            if (point != null)
            {
                brakingPoints = point;
                brakingZone = true;
            }
        }
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

    // Clears the braking state once the AI leaves the trigger area.
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("AI"))
        {
            brakingZone = false;
        }
        else
        {
            BrakingPoints point = other.GetComponent<BrakingPoints>();
            if (point != null)
            {
                brakingPoints = null;
                brakingZone = false;
            }
        }
    }
}
