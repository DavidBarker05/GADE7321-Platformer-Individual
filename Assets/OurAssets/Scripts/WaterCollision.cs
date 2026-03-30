using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

[RequireComponent(typeof(Rigidbody))]
public class WaterCollision : DeathBarrier
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
    Transform[] collisionPoints;
    TransformAccessArray collisionAccessArray;
    Vector3[] modifiedPoints;

    bool hasGeneratedAllColliders = false;
    bool collidersAreNoLongerInDefaultPosition = false;
    bool hasTriggeredThisFrame = false;

    void OnValidate()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void Awake()
    {
        numVectors = resolution * resolution;
        stride = 3 * sizeof(float);
        generationKernelIndex = waterCompShader.FindKernel("GeneratePoints");
        waveKernelIndex = waterCompShader.FindKernel("UpdateWave");
        InitBuffer();
        initialPoints = new Vector3[numVectors];
        GeneratePoints(ref initialPoints);
        collisionPoints = new Transform[numVectors];
        GenerateCollisionPoints(ref collisionPoints, ref initialPoints);
        collisionAccessArray = new TransformAccessArray(collisionPoints);
        modifiedPoints = new Vector3[numVectors];
        hasGeneratedAllColliders = true;
    }

    void LateUpdate()
    {
        if (!hasGeneratedAllColliders || Time.frameCount % doEveryNthFrame != 0) return;
        transform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        UpdateWave(ref initialPoints, ref modifiedPoints);
        UpdateCollisionPointsPositions(ref collisionAccessArray, ref modifiedPoints);
        hasTriggeredThisFrame = false;
    }

    void OnDisable() => DeleteBuffer();

    void OnEnable()
    {
        if (vectorBuffer == null) InitBuffer();
    }

    void OnDestroy()
    {
        DeleteBuffer();
        if (collisionAccessArray.isCreated) collisionAccessArray.Dispose();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!collidersAreNoLongerInDefaultPosition || hasTriggeredThisFrame) return;
        hasTriggeredThisFrame = true;
        base.OnTriggerEnter(other);
    }

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

    void GenerateCollisionPoints(ref Transform[] transforms, ref Vector3[] points)
    {
        for (int i = 0; i < transforms.Length; ++i)
        {
            GameObject go = new GameObject($"CollisionPoint ({i})");
            go.transform.parent = transform;
            go.transform.localPosition = points[i];
            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.radius = size / resolution / 2;
            sc.isTrigger = true;
            transforms[i] = go.transform;
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

    void UpdateCollisionPointsPositions(ref TransformAccessArray accessArray, ref Vector3[] newPositions)
    {
        NativeArray<Vector3> nativeVectors = new NativeArray<Vector3>(newPositions, Allocator.TempJob);
        NativeArray<float3> nativeFloats = nativeVectors.Reinterpret<float3>();
        UpdateCollidersJob job = new UpdateCollidersJob()
        {
            newPositionsArray = nativeFloats
        };
        JobHandle jobHandle = job.Schedule(accessArray);
        jobHandle.Complete();
        if (nativeVectors.IsCreated) nativeVectors.Dispose();
        if (!collidersAreNoLongerInDefaultPosition) collidersAreNoLongerInDefaultPosition = true;
    }
}

[BurstCompile]
public struct UpdateCollidersJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float3> newPositionsArray;

    public void Execute(int index, TransformAccess transform)
    {
        transform.localPosition = newPositionsArray[index];
    }
}
