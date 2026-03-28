using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
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

    Mesh mesh;
    ComputeBuffer vertexBuffer;
    Vector3[] vertexBufferData;
    int kernelIndex;
    Vector3[] baseVertices;
    Vector3[] modifiedVertices;

    void Awake()
    {
        mesh = new Mesh();
        GenerateMesh();
        baseVertices = new Vector3[mesh.vertices.Length];
        mesh.vertices.CopyTo(baseVertices, 0);
        modifiedVertices = new Vector3[baseVertices.Length];
        kernelIndex = waterCompShader.FindKernel("CSMain");
        InitBuffer();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void OnDisable()
    {
        vertexBuffer?.Release();
        vertexBuffer = null;
    }

    void OnDestroy()
    {
        vertexBuffer?.Release();
        vertexBuffer = null;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
        baseVertices.CopyTo(vertexBufferData, 0);
        vertexBuffer.SetData(vertexBufferData);
        waterCompShader.SetVector("WorldOffset", transform.position);
        waterCompShader.SetFloat("WaveAmplitude", WaterWaveManager.Instance.WaveAmplitude);
        waterCompShader.SetFloat("WaveSpeed", WaterWaveManager.Instance.WaveSpeed);
        waterCompShader.SetFloat("WaveFrequency", WaterWaveManager.Instance.WaveFrequency);
        waterCompShader.SetFloat("WaterTime", WaterWaveManager.Instance.WaterTime);
        int threadGroups = Mathf.CeilToInt(baseVertices.Length / 64f);
        waterCompShader.Dispatch(kernelIndex, threadGroups, 1, 1);
        vertexBuffer.GetData(vertexBufferData);
        vertexBufferData.CopyTo(modifiedVertices, 0);
        mesh.vertices = modifiedVertices;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void InitBuffer()
    {
        vertexBuffer?.Release();
        vertexBufferData = new Vector3[baseVertices.Length];
        baseVertices.CopyTo(vertexBufferData, 0);
        vertexBuffer = new ComputeBuffer(baseVertices.Length, 3 * sizeof(float));
        vertexBuffer.SetData(vertexBufferData);
        waterCompShader.SetBuffer(kernelIndex, "VertexBuffer", vertexBuffer);
        waterCompShader.SetInt("VertexCount", baseVertices.Length);
    }

    void GenerateMesh()
    {
        Vector3[] vertices = GenerateVertices();
        int[] triangles = GenerateTriangles();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    Vector3[] GenerateVertices()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        for (int z = 0; z < resolution; ++z)
        {
            for (int x = 0; x < resolution; ++x)
            {
                float xPos = x * size / (resolution - 1) - size / 2f;
                float yPos = 0f;
                float zPos = z * size / (resolution - 1) - size / 2f;
                vertices[z * resolution + x] = new Vector3(xPos, yPos, zPos);
            }
        }
        return vertices;
    }

    int[] GenerateTriangles()
    {
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triangleIndex = 0;
        for (int i = 0; i < resolution - 1; ++i)
        {
            for (int j = 0; j < resolution - 1; ++j)
            {
                int baseIndex = i * resolution + j;
                triangles[triangleIndex++] = baseIndex;
                triangles[triangleIndex++] = baseIndex + resolution + 1;
                triangles[triangleIndex++] = baseIndex + 1;

                triangles[triangleIndex++] = baseIndex;
                triangles[triangleIndex++] = baseIndex + resolution;
                triangles[triangleIndex++] = baseIndex + resolution + 1;
            }
        }
        return triangles;
    }
}
