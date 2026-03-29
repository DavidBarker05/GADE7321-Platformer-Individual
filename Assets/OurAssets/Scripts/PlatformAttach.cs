using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure attached to game object with collider
public class PlatformAttach : MonoBehaviour
{
    void OnValidate()
    {
        GetComponent<Collider>().isTrigger = true; // Ensure collider is a trigger
        Debug.Assert(transform.parent, "Script needs to be attached to a game object that has a parent"); // Ensure attached to a parent
        if (!transform.parent) return;
        Debug.Assert(transform.parent.gameObject.GetComponent<Collider>(), "Game object's parent must have a collider"); // Ensure parent has a collider
        Debug.Assert(transform.parent.parent, "Game object's parent must also have a parent"); // Ensure parent is attached to a parent
        if (!transform.parent.parent) return;
        Debug.Assert(transform.parent.parent.lossyScale == Vector3.one, "Parent of game object's parent must have no scale applied"); // Ensure parent's parent doesn't have scale
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerMovement>().AttachToPlatform(transform.parent.parent);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerMovement>().DetachFromPlatform(transform.parent.parent);
    }
}
