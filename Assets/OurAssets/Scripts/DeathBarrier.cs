using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure attached to an object with a collider
public class DeathBarrier : MonoBehaviour
{
    Collider _collider;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true; // Ensure the collider is a trigger collider
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CheckpointManager.Instance.LoseLife();
    }
}
