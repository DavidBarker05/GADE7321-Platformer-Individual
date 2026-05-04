using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure attached to object with collider
public class Collectable : MonoBehaviour
{
    [SerializeField, Min(0)]
    int scoreGiven = 100;

    void OnValidate() => GetComponent<Collider>().isTrigger = true; // Ensure collider is a trigger

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CollectableManager.Instance?.CollectCollectable();
        CheckpointManager.Instance.AddScore(scoreGiven); // Now give score
        Destroy(gameObject);
    }
}
