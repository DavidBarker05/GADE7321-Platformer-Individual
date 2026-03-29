using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure attached to object with collider
public class Collectable : MonoBehaviour
{
    void OnValidate() => GetComponent<Collider>().isTrigger = true; // Ensure collider is a trigger

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CollectableManager.Instance?.CollectCollectable();
        Destroy(gameObject);
    }
}
