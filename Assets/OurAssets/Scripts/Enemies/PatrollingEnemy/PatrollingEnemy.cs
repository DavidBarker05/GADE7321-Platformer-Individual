using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class PatrollingEnemy : BaseEnemy
{
	NavMeshAgent m_Agent;
	LinkedListADT<Transform> m_PatrolPoints;

	LinkedListADTNode<Transform> m_CurrentDestination;

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (m_PatrolPoints == null || m_PatrolPoints.IsEmpty) return;
		foreach (Transform t in m_PatrolPoints)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(t.position + Vector3.up * 0.25f, 0.25f);
			Transform next = m_PatrolPoints.FindFirst(t).Next.Value;
			Debug.DrawLine(t.position + Vector3.up * 0.25f, next.position + Vector3.up * 0.25f, Color.yellow);
		}
		if (m_CurrentDestination) Debug.DrawLine(transform.position + Vector3.up, m_CurrentDestination.Value.position + Vector3.up * 0.25f, Color.magenta);
	}
#endif

	private void OnValidate() => GetComponent<CapsuleCollider>().isTrigger = true;

	private void OnEnable() => GetComponent<CapsuleCollider>().isTrigger = true;

	void Awake() => m_Agent = GetComponent<NavMeshAgent>();

	void Update()
	{
		if (!Enabled
			|| m_CurrentDestination == null
			|| m_Agent.pathPending
			|| m_Agent.remainingDistance > m_Agent.stoppingDistance) return;
		m_CurrentDestination = m_CurrentDestination.Next;
		if (m_CurrentDestination != null) m_Agent.SetDestination(m_CurrentDestination.Value.position);
	}

	public void SetPatrolPoints(LinkedListADT<Transform> patrolPoints)
	{
		m_PatrolPoints = patrolPoints.IsCircular ? patrolPoints : new LinkedListADT<Transform>(patrolPoints); // Circular by default
		m_CurrentDestination = m_PatrolPoints.Front; // If empty list this will be null and the
	}
}
