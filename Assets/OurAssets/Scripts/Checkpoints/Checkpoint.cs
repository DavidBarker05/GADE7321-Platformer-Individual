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
    public int StartingLives => startingLives;
    public int Lives { get; set; }
    public int Score { get; set; }

    void OnValidate()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
        if (!RespawnPoint) RespawnPoint = transform;
    }

	void OnEnable()
	{
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider collider in colliders)
		{
			collider.isTrigger = true;
		}
		if (!RespawnPoint) RespawnPoint = transform;
	}

	void Start()
    {
		if (isStart) CheckpointManager.Instance.SetStartingCheckpoint(this);
        else
        {
            HasBeenCaptured = false;
            Lives = 0;
			Score = 0;
		}
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) CheckpointManager.Instance.CaptureCheckpoint(this);
    }

    public void SnapToGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.01f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit)) transform.position = hit.point;
    }
}
