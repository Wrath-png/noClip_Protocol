using System.Collections.Generic;
using UnityEngine;

public class PatrolRouteMono : MonoBehaviour {
    public List<Transform> waypoints;
    public float distanceFromStart;
    public Transform[] GetWaypoints() => waypoints.ToArray();

    public void CalculateDistanceFrom(Transform playerSpawn)
    {
        if (waypoints != null && waypoints.Count > 0 && playerSpawn != null)
        {
            distanceFromStart = Vector3.Distance(waypoints[0].position, playerSpawn.position);
        }
        else
        {
            distanceFromStart = 0f;
        }
    }
}