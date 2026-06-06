using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack}
    public State currentState = State.Patrol;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float patrolSpeed = 9f;
    private int waypointIndex = 0;

    [Header("Detection")]
    public Transform player;
    public float sightRange = 15f;
    public float sightAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Combat")]
    public float attackRange = 2.5f;
    public float chaseSpeed = 5f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextWaypoint();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
        }
    }

    void HandlePatrol()
    {
        agent.speed = patrolSpeed;

        if (CanSeePlayer())
        {
            currentState = State.Chase;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.SetDestination(waypoints[waypointIndex].position);
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void HandleChase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);

        if(dist <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (!CanSeePlayer())
        {
            currentState = State.Patrol;
            GoToNextWaypoint();
        }
    }

    void HandleAttack()
    {
        agent.ResetPath();
        transform.LookAt(player);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange)
            currentState = State.Chase;
    }

    bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float dist =toPlayer.magnitude;

        if (dist > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > sightAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, toPlayer.normalized, dist, obstacleMask))
            return false;

        return true;
    }
}