using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OptionalCollectable : MonoBehaviour
{
    [SerializeField, Min(0)]
    int scoreBonus = 10;

    void OnValidate()
    {
        if (GetComponent<Collider>() is MeshCollider meshCollider) meshCollider.convex = true;
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CheckpointManager.Instance.AddScore(scoreBonus);
        Destroy(gameObject);
    }
}
