using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    [SerializeField]
    Transform playerTransform;
    [SerializeField, Min(2)]
    int resolution = 2;
    [SerializeField, Min(0f)]
    float size = 10f;
    [SerializeField]
    ComputeShader waterCompShader;
    [SerializeField, Range(1, 10)]
    int doEveryNthFrame = 1;

    ComputeBuffer vectorBuffer;
    Vector3[] vectorBufferData;

    int numVectors;
    int stride;

    int generationKernelIndex;
    int waveKernelIndex;

    Vector3[] initialPoints;
    SphereCollider[] collisionPoints;
    Vector3[] modifiedPoints;

    void Awake()
    {
        numVectors = resolution * resolution;
        stride = 3 * sizeof(float);
        generationKernelIndex = waterCompShader.FindKernel("GeneratePoints");
        waveKernelIndex = waterCompShader.FindKernel("UpdateWave");
        InitBuffer();
        initialPoints = new Vector3[numVectors];
        GeneratePoints(ref initialPoints);
        collisionPoints = new SphereCollider[numVectors];
        GenerateCollisionPoints(ref collisionPoints, ref initialPoints);
        modifiedPoints = new Vector3[numVectors];
    }

    void LateUpdate()
    {
        if (Time.frameCount % doEveryNthFrame != 0) return;
        transform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        UpdateWave(ref initialPoints, ref modifiedPoints);
        UpdateCollisionPointsPositions(ref collisionPoints, ref modifiedPoints);
    }

    void OnDisable() => DeleteBuffer();

    void OnEnable()
    {
        if (vectorBuffer == null) InitBuffer();
    }

    void OnDestroy() => DeleteBuffer();

    void InitBuffer()
    {
        vectorBuffer?.Release();
        vectorBuffer = new ComputeBuffer(numVectors, stride);
        vectorBufferData = new Vector3[numVectors];
        waterCompShader.SetBuffer(generationKernelIndex, "VectorBuffer", vectorBuffer);
        waterCompShader.SetBuffer(waveKernelIndex, "VectorBuffer", vectorBuffer);
        vectorBuffer.SetData(vectorBufferData);
        waterCompShader.SetInt("VectorCount", numVectors);
    }

    void DeleteBuffer()
    {
        vectorBuffer?.Release();
        vectorBuffer = null;
    }

    void GeneratePoints(ref Vector3[] points)
    {
        waterCompShader.SetFloat("Size", size);
        int threadGroups = Mathf.CeilToInt(resolution / 8f);
        vectorBufferData = new Vector3[numVectors];
        vectorBuffer.SetData(vectorBufferData);
        waterCompShader.Dispatch(generationKernelIndex, threadGroups, threadGroups, 1);
        vectorBuffer.GetData(vectorBufferData);
        vectorBufferData.CopyTo(points, 0);
    }

    void GenerateCollisionPoints(ref SphereCollider[] colliders, ref Vector3[] points)
    {
        for (int i = 0; i < colliders.Length; ++i)
        {
            colliders[i] = gameObject.AddComponent<SphereCollider>();
            colliders[i].center = points[i];
            colliders[i].radius = size / resolution / 2;
            colliders[i].isTrigger = true;
        }
    }

    void UpdateWave(ref Vector3[] initialPoints, ref Vector3[] resultantPoints)
    {
        initialPoints.CopyTo(vectorBufferData, 0);
        vectorBuffer.SetData(vectorBufferData);
        waterCompShader.SetVector("WorldOffset", transform.position);
        waterCompShader.SetFloat("WaveAmplitude", WaterWaveManager.Instance.WaveAmplitude);
        waterCompShader.SetFloat("WaveSpeed", WaterWaveManager.Instance.WaveSpeed);
        waterCompShader.SetFloat("WaveFrequency", WaterWaveManager.Instance.WaveFrequency);
        waterCompShader.SetFloat("WaterTime", WaterWaveManager.Instance.WaterTime);
        int threadGroups = Mathf.CeilToInt(numVectors / 64f);
        waterCompShader.Dispatch(waveKernelIndex, threadGroups, 1, 1);
        vectorBuffer.GetData(vectorBufferData);
        vectorBufferData.CopyTo(resultantPoints, 0);
    }

    void UpdateCollisionPointsPositions(ref SphereCollider[] colliders, ref Vector3[] newPositions)
    {
        for (int i = 0; i < colliders.Length; ++i)
        {
            colliders[i].center = newPositions[i];
        }
    }
}
