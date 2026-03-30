using UnityEngine;

public class DeathBarrier : MonoBehaviour
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CheckpointManager.Instance.LoseLife();
    }
}
