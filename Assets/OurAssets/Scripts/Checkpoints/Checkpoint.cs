using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    bool isStart = false;
    [SerializeField]
    int startingLives = 0;
    [SerializeField]
    Transform respawnPoint;

    public Transform RespawnPoint => respawnPoint ?? transform; // Ignore the unity warning it works fine despite what they say

    public bool HasBeenCaptured { get; set; }
    public int Lives { get; set; }
    public int Score { get; set; }

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
