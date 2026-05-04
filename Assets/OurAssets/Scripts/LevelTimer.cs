using UnityEngine;

// Add score based on how many seconds were left in the level
public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance { get; private set; }

    [SerializeField, Range(120f, 1200f)] // 2min-20min (I doubt we need 20 but who knows)
    float m_LevelTime = 300f; // 5min by default

    public int CurrentTime => Mathf.CeilToInt(m_CurrentTime);
    public int BonusScore => Mathf.Max(Mathf.CeilToInt(m_LevelTime - m_CurrentTime), 0);

    float m_CurrentTime;

	void Awake()
	{
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
	}

	void Start() => m_CurrentTime = m_LevelTime;

    void Update() => m_CurrentTime -= Time.deltaTime; // Thankfully dialogue freezes time at the start so this won't be affected

}
