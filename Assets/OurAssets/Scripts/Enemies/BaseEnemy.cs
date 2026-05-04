using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
	protected virtual void OnCollisionEnter(Collision collision) => CheckForPlayerCollision(collision.gameObject);

	protected virtual void OnTriggerEnter(Collider other) => CheckForPlayerCollision(other.gameObject);

	/// <summary>
	/// I don't recommend overriding this unless you really have to
	/// </summary>
	protected virtual void CheckForPlayerCollision(GameObject go)
	{
		if (go.CompareTag("Player") || gameObject.layer == LayerMask.NameToLayer("Player"))
			CheckpointManager.Instance.LoseLife();
	}
}
