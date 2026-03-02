using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [field: SerializeField]
    public bool IsStart { get; set; } = false;
    [field: SerializeField]
    public int StartingLives { get; set; } = 0;

    public bool HasBeenCaptured { get; set; }
    public int Lives { get; set; }
    public int Score { get; set; }

    void Start()
    {
        if (IsStart) CheckpointManager.Instance?.SetStartingCheckpoint(this);
        else
        {
            HasBeenCaptured = false;
            Lives = 0;
        }
        Score = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CheckpointManager.Instance?.CaptureCheckpoint(this);
    }
}
