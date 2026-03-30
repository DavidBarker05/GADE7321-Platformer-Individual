using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensure attached to game object with a collider
public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    bool isStart = false;
    [SerializeField]
    int startingLives = 0;
    [field: SerializeField]
    public Transform RespawnPoint { get; private set; }

    public bool HasBeenCaptured { get; set; }
    public int Lives { get; set; }
    public int Score { get; set; }

    void OnValidate()
    {
        GetComponent<Collider>().isTrigger = true; // Ensure collider is a trigger
        if (!RespawnPoint) RespawnPoint = transform;
    }

    void Start()
    {
        if (isStart)
        {
            CheckpointManager.Instance.SetStartingCheckpoint(this);
            Lives = startingLives;
        }
        else
        {
            HasBeenCaptured = false;
            Lives = 0;
        }
        Score = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CheckpointManager.Instance.CaptureCheckpoint(this);
    }
}
