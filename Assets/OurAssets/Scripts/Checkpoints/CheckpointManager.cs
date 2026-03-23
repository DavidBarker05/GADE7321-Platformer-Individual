using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    static CheckpointManager _instance;
    public static CheckpointManager Instance
    {
        get
        {
            // Lazy instantiation
            if (!_instance)
            {
                GameObject go = new GameObject("CheckpointManager");
                _instance = go.AddComponent<CheckpointManager>();
            }
            return _instance;
        }
    }

    readonly StackADT<Checkpoint> checkpointStack = new StackADT<Checkpoint>();

    PlayerRespawner _player;
    PlayerRespawner Player
    {
        get
        {
            // This ensures that if a player exists they will be used. We don't want
            // to assign the player in awake since checkpoint manager awake might
            // happen before player respawner gets created. We also don't want to
            // assign the player in start since checkpoint start might happen before
            // checkpoint manager start and then player won't be assigned when trying
            // to spawn the player at the starting checkpoint
            if (!_player) _player = FindFirstObjectByType<PlayerRespawner>();
            return _player;
        }
    }

    void Awake()
    {
        if (_instance && _instance != this) Destroy(gameObject);
        else _instance = this;
    }

    public void SetStartingCheckpoint(Checkpoint checkpoint)
    {
        if (checkpointStack.IsEmpty)
        {
            CaptureCheckpoint(checkpoint); // Capture the checkpoint
            Transform startingPoint = checkpoint.RespawnPoint;
            Player.Respawn(startingPoint); // Spawn player at start
        }
        else checkpoint.HasBeenCaptured = false; // Set HasBeenCaptured to false so that it can still be captured
    }

    public void CaptureCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint.HasBeenCaptured) return; // If captured already then don't capture again
        checkpoint.HasBeenCaptured = true; // Set captured to true (also does this for the starting checkpoint)
        if (!checkpointStack.IsEmpty) // If a checkpoint is already on the stack
        {
            Checkpoint last = checkpointStack.Pop(); // Remove from the stack and get its data
            checkpoint.Lives = last.Lives;
            checkpoint.Score = last.Score;
        }
        checkpointStack.Push(checkpoint); // Add the new checkpoint to the stack
    }

    public void LoseLife()
    {
        if (checkpointStack.IsEmpty) return; // If no checkpoints then do nothing
        if (--checkpointStack.Peek().Lives > 0) // Decrement the lives and if more than 0 respawn
        {
            Transform respawnPoint = checkpointStack.Peek().RespawnPoint;
            Player.Respawn(respawnPoint);
        }
        else // Else die
        {
            // Death Screen
        }
    }
}
