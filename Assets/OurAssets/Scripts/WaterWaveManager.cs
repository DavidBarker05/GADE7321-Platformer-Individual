using UnityEngine;

public class WaterWaveManager : MonoBehaviour
{
    static WaterWaveManager _instance;
    public static WaterWaveManager Instance
    {
        get
        {
            // Lazy instantiation
            if (!_instance)
            {
                GameObject go = new GameObject("WaterWaveManager");
                _instance = go.AddComponent<WaterWaveManager>();
            }
            return _instance;
        }
    }

    [field: SerializeField, Range(0f, 1f)]
    public float WaveAmplitude { get; private set; } = 0.25f;
    [field: SerializeField, Range(0f, 10f)]
    public float WaveSpeed { get; private set; } = 1f;
    [field: SerializeField, Range(0f, 10f)]
    public float WaveFrequency { get; private set; } = 1f;
    [SerializeField]
    Material[] waterMaterials;

    public float WaterTime { get; private set; } = 0f;

    void Awake()
    {
        if (_instance && _instance != this) Destroy(gameObject);
        else _instance = this;
    }

    void OnDisable()
    {
        foreach (Material m in waterMaterials)
        {
            m.SetFloat("_GlobalWaterTime", 0f);
        }
    }

    void OnDestroy()
    {
        foreach (Material m in waterMaterials)
        {
            m.SetFloat("_GlobalWaterTime", 0f);
        }
    }

    void Update()
    {
        WaterTime += Time.deltaTime;
        foreach (Material m in waterMaterials)
        {
            m.SetFloat("_WaveAmplitude", WaveAmplitude);
            m.SetFloat("_WaveSpeed", WaveSpeed);
            m.SetFloat("_WaveFrequency", WaveFrequency);
            m.SetFloat("_GlobalWaterTime", WaterTime);
        }
    }

    public float GetWaterHeight(Vector3 position) => Mathf.Sin(position.x * WaveFrequency + position.z * WaveFrequency + WaterTime * WaveSpeed) * WaveAmplitude;
}
