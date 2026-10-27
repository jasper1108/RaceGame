using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Collects all child transforms in a race track and visualizes the path with gizmos.
public class WaypointContainer : MonoBehaviour
{
    public List<Transform> waypoints;
    public float forwardGizmoLength = 2f;

    // Stores every child waypoint so the AI and player can follow the route.
    void Awake()
    {
        foreach(Transform tr in gameObject.GetComponentsInChildren<Transform>())
        {
            waypoints.Add(tr);
        }
        waypoints.Remove(waypoints[0]);
    }

    // Draws a small directional arrow at each waypoint in the editor.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (Transform waypoint in GetComponentsInChildren<Transform>())
        {
            if (waypoint == transform)
            {
                continue;
            }

            Vector3 forward = waypoint.forward * forwardGizmoLength;
            Vector3 tip = waypoint.position + forward;
            float arrowSize = forwardGizmoLength * 0.2f;
            Vector3 arrowBase = tip - waypoint.forward * arrowSize;

            Gizmos.DrawLine(waypoint.position, tip);
            Gizmos.DrawLine(tip, arrowBase + waypoint.right * arrowSize * 0.5f);
            Gizmos.DrawLine(tip, arrowBase - waypoint.right * arrowSize * 0.5f);
        }
    }
}