using UnityEngine;

public class StationaryEnemy : BaseEnemy
{
    [SerializeField, Range(1f / 10f, 1f / 0.1f)] // Min 1 projectile every 10 seconds, Max 1 projectile every 0.1 seconds
    float m_FireRate = 0.5f;
    [SerializeField]
    ProjectileData m_Projectile;
    [SerializeField]
    Transform m_ProjectileSpawnTransform;

    float m_CurrentTime = 0f;

	void FixedUpdate()
	{
        m_CurrentTime += Time.fixedDeltaTime;
        if (m_CurrentTime < 1f / m_FireRate) return;
        ProjectileFactory.Instance.SpawnProjectile(m_Projectile, m_ProjectileSpawnTransform).Activate();
        m_CurrentTime = 0f;
	}
}
