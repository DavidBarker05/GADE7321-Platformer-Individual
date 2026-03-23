using UnityEngine;

[RequireComponent(typeof(Collider), typeof(DialogueHolder))]
public class DialogueTrigger : MonoBehaviour
{
    Collider _collider;
    DialogueHolder holder;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (!_collider.isTrigger) _collider.isTrigger = true; // Make sure collider is a trigger
        holder = GetComponent<DialogueHolder>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) holder.StartDialogue();
    }
}
