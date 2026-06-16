using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : BaseEnemy // Extend BaseEnemy so can damage player and be spawned in
{
    public enum State { Patrol, Chase, Attack }
    public State currentState = State.Patrol;

    [Header("Patrol")]
    public float patrolSpeed = 9f;
    private int nodeIndex = 0; // David - renamed from waypointIndex to nodeIndex

    [Header("Detection")]
    public Transform player;
    public float sightRange = 15f;
    public float sightAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Combat")]
    public float attackRange = 2.5f;
    public float chaseSpeed = 12f;

    private NavMeshAgent agent;
    private GraphADT<Transform> patrolPoints; // David added
    private GraphADTNode<Transform>[] path; // David added

#if UNITY_EDITOR
    // David - Draw some gizmos to help see what the boss can do/is doing
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Vector3 offsetPos = transform.position + Vector3.up * 0.5f;
        Vector3 rightLine = new Vector3(Mathf.Sin((transform.eulerAngles.y + sightAngle / 2f) * Mathf.Deg2Rad), 0f, Mathf.Cos((transform.eulerAngles.y + sightAngle / 2f) * Mathf.Deg2Rad));
        Vector3 leftLine = new Vector3(Mathf.Sin((transform.eulerAngles.y + -sightAngle / 2f) * Mathf.Deg2Rad), 0f, Mathf.Cos((transform.eulerAngles.y + -sightAngle / 2f) * Mathf.Deg2Rad));
        Debug.DrawLine(offsetPos, offsetPos + rightLine * sightRange, Color.green);
        Debug.DrawLine(offsetPos, offsetPos + leftLine * sightRange, Color.green);
        if (currentState != State.Patrol)
        {
            Vector3 playerPos = player.transform.position;
            playerPos.y = transform.position.y;
            Debug.DrawLine(transform.position + Vector3.up * 0.75f, playerPos + Vector3.up * 0.75f, Color.red);
        }
        if (patrolPoints == null || patrolPoints.IsEmpty) return;
        foreach (GraphADTNode<Transform> node in patrolPoints.Nodes)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(node.Value.position + Vector3.up * 0.25f, 0.25f);
            foreach (GraphADTNode<Transform> connection in node.Connections)
                Debug.DrawLine(node.Value.position + Vector3.up * 0.25f, connection.Value.position + Vector3.up * 0.25f, Color.yellow);
        }
        if (path == null && path.Length == 0 || currentState != State.Patrol) return;
        Debug.DrawLine(transform.position + Vector3.up, path[nodeIndex].Value.position + Vector3.up * 0.25f, Color.cyan);
        Debug.DrawLine(transform.position + Vector3.up, path[^1].Value.position + Vector3.up * 0.25f, Color.magenta);
    }
#endif

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // David - Moved to awake instead of start
    }

    void Update()
    {
        if (!Enabled) return; // David added
        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
        }
    }

    // David - Set the graph for the enemy to follow on spawn as well as where it starts
    public void SetPatrolPoints(GraphADT<Transform> graph)
    {
        patrolPoints = graph;
    }

    // David - Get path to patrol
    GraphADTNode<Transform>[] GetPath(GraphADTNode<Transform> start)
    {
        GraphADTNode<Transform>[] nodes = patrolPoints.Nodes; // David - Faster index than constantly accessing from patrol points
        GraphADTNode<Transform> end;
        do
        {
            end = nodes[Random.Range(0, nodes.Length)];
        } while (end == start); // David - Make sure end isn't start since randomly chosen
        nodeIndex = 0; // David - Reset index
        // David - If fps < 50fps do depth-first search because it finds the first path rather than the shortest
        // path, so that means it doesn't search through the whole graph like breadth-first search so it is faster
        // and better for performance. If fps >= 50fps breadth-first search is fine since framerate is manageable
        if (Time.unscaledDeltaTime < 1f / 50f + Mathf.Epsilon) return patrolPoints.GetPathDFS(start, end);
        else return patrolPoints.GetPathBFS(start, end);
    }

    void HandlePatrol()
    {
        path ??= GetPath(patrolPoints.Nodes[0]); // David - If no path (i.e. start of game) get path from first node
        agent.speed = patrolSpeed;

        if (CanSeePlayer())
        {
            currentState = State.Chase;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (path == null) return;
        if (nodeIndex == path.Length) path = GetPath(path[^1]); // David - If at end get new path
        agent.SetDestination(path[++nodeIndex].Value.position); // David - Go to next waypoint
    }

    void HandleChase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
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
        float dist = toPlayer.magnitude;

        if (dist > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > sightAngle * 0.5f) return false;

        return !Physics.Raycast(transform.position + Vector3.up, toPlayer.normalized, dist, obstacleMask);
    }
}