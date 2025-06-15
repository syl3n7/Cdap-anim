using UnityEngine;
using UnityEngine.AI;

public class CarPath : MonoBehaviour
{
    public Transform[] waypoints;
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;

    public bool CarBoss;
    private bool startMoving = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (CarBoss)
        {
            agent.isStopped = true;
            startMoving = false;
        }
        else
        {
            startMoving = true;
            agent.destination = waypoints[currentWaypointIndex].position;
        }
    }

    void Update()
    {
        if (startMoving)
        {
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 2f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

                agent.destination = waypoints[currentWaypointIndex].position;
            }
        }

    }

    public void StartMoving()
    {
        if (CarBoss)
        {
            startMoving = true;
            agent.isStopped = false;
            agent.destination = waypoints[currentWaypointIndex].position;
        }
    }
}
