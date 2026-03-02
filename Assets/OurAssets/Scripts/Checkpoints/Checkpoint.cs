using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    bool isStart = false;
    [SerializeField]
    int startingLives = 0;

    public bool HasBeenCaptured { get; private set; }
    public int Lives { get; private set; }
    public int Score { get; set; }

    void Start()
    {
        if (isStart)
        {
            if (!CheckpointManager.Instance.CheckpointStack.IsEmpty && CheckpointManager.Instance.CheckpointStack.Peek() != this)
            {
                isStart = false;
                startingLives = 0;
            }
            else CheckpointManager.Instance.CheckpointStack.Push(this);
        }
        HasBeenCaptured = isStart;
        Lives = startingLives;
        Score = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (HasBeenCaptured) return;
        HasBeenCaptured = true;
        if (!CheckpointManager.Instance.CheckpointStack.IsEmpty)
        {
            Checkpoint last = CheckpointManager.Instance.CheckpointStack.Peek();
            Lives = last.Lives;
            Score = last.Score;
            CheckpointManager.Instance.CheckpointStack.Pop();
        }
        CheckpointManager.Instance.CheckpointStack.Push(this);
    }

    public void LoseLife()
    {
        --Lives;
        if (Lives == 0)
        {
            // Death Screen
        }
        else
        {
            // Respawn
        }
    }
}
