using UnityEngine;

// Make sure no collider is attached to this
public class Projectile : MonoBehaviour
{
	public ProjectileData ProjectileData
	{
		get => m_ProjectileData;
		set
		{
			if (m_ProjectileData) throw new System.ArgumentException("Projectile already has ProjectileData");
			m_Mesh = Instantiate(value.Mesh, transform);
			m_ProjectileData = value;
		}
	}

	ProjectileData m_ProjectileData;
	GameObject m_Mesh;
	bool m_bActive;
	float m_CurrentTravelTime;
	Vector3 m_LastPosition;

	void FixedUpdate()
	{
		if (!m_ProjectileData || !m_bActive) return;
		transform.position += (m_ProjectileData.Speed * Time.fixedDeltaTime) * ProjectileHelper.GetMotionDirection(transform, m_ProjectileData.AxisOfMotion);
		Vector3 delta = transform.position - m_LastPosition;
		if (Physics.SphereCast(m_LastPosition, m_ProjectileData.Radius, delta, out RaycastHit hit, delta.magnitude, m_ProjectileData.HittableLayers, m_ProjectileData.QueryTriggerInteraction)) // Does this detect the player? I don't remember
		{
			if (hit.collider.CompareTag("Player") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) // In case the collider doesn't have Player tag then check if the layer is Player
				CheckpointManager.Instance.LoseLife();
			Deactivate(); // Stop existing on contact
		}
		if (!m_bActive) return;
		m_LastPosition = transform.position;
		m_CurrentTravelTime += Time.fixedDeltaTime;
		if (m_CurrentTravelTime >= m_ProjectileData.MaxTravelTime) Deactivate(); // Stop existing after max travel time
	}

	public void Activate()
	{
		if (!m_ProjectileData || m_bActive) return;
		m_bActive = true;
		m_CurrentTravelTime = 0f;
		m_LastPosition = transform.position;
		gameObject.SetActive(true);
	}

	void Deactivate()
	{
		if (!m_ProjectileData || !m_bActive) return;
		m_bActive = false;
		ProjectileFactory.Instance.DeactivateProjectile(this);
		m_CurrentTravelTime = 0f;
		gameObject.SetActive(false);
	}
}
