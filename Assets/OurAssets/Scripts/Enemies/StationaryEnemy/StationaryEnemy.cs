using UnityEngine;

public class StationaryEnemy : BaseEnemy
{
	[SerializeField, Min(5f)]
	float m_ActivationDistance = 10f;
	[SerializeField, Range(0f, 180f)]
	float m_AttackFOV = 90f;
	[SerializeField, Min(0.1f)]
	float m_MaxTurnAngle = 1f;
    [SerializeField]
    ProjectileData m_Projectile;
    [SerializeField]
    Transform m_ProjectileSpawnTransform;
	[SerializeField]
	StationaryEnemyAnimationLinker m_AnimationLinker;

	Vector3 m_StartingForward;
	PlayerMovement m_Player;

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, m_ActivationDistance);
		Vector3 offsetPos = transform.position + Vector3.up * 0.5f;
		Vector3 rightLine = new Vector3(Mathf.Sin((transform.eulerAngles.y + m_AttackFOV / 2f) * Mathf.Deg2Rad), 0f, Mathf.Cos((transform.eulerAngles.y + m_AttackFOV / 2f) * Mathf.Deg2Rad));
		Vector3 leftLine = new Vector3(Mathf.Sin((transform.eulerAngles.y + -m_AttackFOV / 2f) * Mathf.Deg2Rad), 0f, Mathf.Cos((transform.eulerAngles.y + -m_AttackFOV / 2f) * Mathf.Deg2Rad));
		Debug.DrawLine(offsetPos, offsetPos + rightLine * m_ActivationDistance, Color.red);
		Debug.DrawLine(offsetPos, offsetPos + leftLine * m_ActivationDistance, Color.red);
	}
#endif

	void Awake() => m_StartingForward = transform.forward;

	void Start() => m_Player = FindFirstObjectByType<PlayerMovement>();

	void Update()
	{
		if (!Enabled)
		{
			m_AnimationLinker.DoAttack = false;
			return;
		}
		Vector3 playerPos = m_Player.transform.position;
		if (Vector3.Distance(transform.position, playerPos) > m_ActivationDistance)
		{
			m_AnimationLinker.DoAttack = false;
			return;
		}
		playerPos.y = transform.position.y;
		Vector3 toPlayer = (playerPos - transform.position).normalized;
		float cos = Vector3.Dot(m_StartingForward, (playerPos - transform.position).normalized);
		float angle = Mathf.Acos(cos) * Mathf.Rad2Deg;
		Quaternion target = Quaternion.LookRotation(angle > m_AttackFOV / 2f ? m_StartingForward : toPlayer);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, target, m_MaxTurnAngle);
		m_AnimationLinker.DoAttack = angle <= m_AttackFOV / 2f;
	}

	public void Shoot() => ProjectileFactory.Instance.Create(m_Projectile, m_ProjectileSpawnTransform).Activate();
}
