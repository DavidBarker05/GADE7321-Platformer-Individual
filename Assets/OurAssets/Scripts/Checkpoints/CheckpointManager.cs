using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    static readonly string TempCheckpointDataFile = Path.Combine(Application.temporaryCachePath, "CheckpointData.txt"); // The place to store data between levels

    [SerializeField]
    DeathScreen deathScreen;

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

    bool isQuit = false;

	void OnApplicationQuit() // Hopefully this gets called when editor stops playing too
	{
        isQuit = true;
        if (File.Exists(TempCheckpointDataFile)) File.Delete(TempCheckpointDataFile); // Destroy temp data on close
	}

	void OnDestroy()
	{
        // Write data to file on destroy (scene change), but not if application closing
        if (!isQuit) File.WriteAllText(TempCheckpointDataFile, $"{checkpointStack.Peek().Lives}\n{checkpointStack.Peek().Score}");
	}

	public void SetStartingCheckpoint(Checkpoint checkpoint)
    {
		checkpoint.Score = 0; // Set score to zero for normal and starting checkpoint, because only if previous data exists the value needs to be something else
        if (checkpointStack.IsEmpty)
        {
            CaptureCheckpoint(checkpoint); // Capture the checkpoint
            checkpoint.Lives = checkpoint.StartingLives;
            if (LevelStartManager.Instance.StartingLevelIndex != SceneManager.GetActiveScene().buildIndex) // Not in the starting scene so we should read the previous lives and score
            {
                if (File.Exists(TempCheckpointDataFile))
                {
                    string contents = File.ReadAllText(TempCheckpointDataFile);
                    string[] lines = contents.Split('\n'); // Split by line
                    if (lines.Length == 2)
                    {
                        checkpoint.Lives = int.Parse(lines[0]);
                        checkpoint.Score = int.Parse(lines[1]);
                    }
                }
            }
            Transform startingPoint = checkpoint.RespawnPoint;
            Player.Respawn(startingPoint); // Spawn player at start
        }
        else
        {
            checkpoint.Lives = 0; // Set lives to 0 because not starting checkpoint
            checkpoint.HasBeenCaptured = false; // Set HasBeenCaptured to false so that it can still be captured
        }
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
        else deathScreen.gameObject.SetActive(true); // Else die
    }

    public void AddScore(int score)
    {
        if (!checkpointStack.IsEmpty && score > 0) checkpointStack.Peek().Score += score;
    }
}
