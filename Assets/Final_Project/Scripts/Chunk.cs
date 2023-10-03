using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public NoiseGenerator NoiseGenerator;
    public ComputeShader MarchingShader;
    public MeshCollider MeshCollider;
    Mesh _mesh;
    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public static int SizeOf => sizeof(float) * 3 * 3;
    }
    ComputeBuffer _trianglesBuffer;
    ComputeBuffer _trianglesCountBuffer;
    ComputeBuffer _weightsBuffer;
    float[] _weights;
    void Start()
    {
        this.transform.localScale = this.transform.localScale * GridMetrics.ChunkScale;
        _weights = NoiseGenerator.GetNoise(transform.position);
        _mesh = new Mesh();
        UpdateMesh();
    }
    private void Awake()
    {
        CreateBuffers();
    }
    private void OnDestroy()
    {
        ReleaseBuffers();
    }
    void CreateBuffers()
    {
        _trianglesBuffer = new ComputeBuffer(5 * (GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk), Triangle.SizeOf, ComputeBufferType.Append);
        _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float));
    }
    void ReleaseBuffers()
    {
        _trianglesBuffer.Release();
        _trianglesCountBuffer.Release();
        _weightsBuffer.Release();
    }
    Mesh ConstructMesh()
    {
        int kernel = MarchingShader.FindKernel("March");

        MarchingShader.SetBuffer(0, "_Triangles", _trianglesBuffer);
        MarchingShader.SetBuffer(0, "_Weights", _weightsBuffer);

        MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        MarchingShader.SetInt("_ChunkScale", GridMetrics.ChunkScale);
        MarchingShader.SetFloat("_IsoLevel", .5f);

        _weightsBuffer.SetData(_weights);
        _trianglesBuffer.SetCounterValue(0);

        MarchingShader.Dispatch(kernel, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads);
        
        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        _trianglesBuffer.GetData(triangles);
        return CreateMeshFromTriangles(triangles);
    }
    int ReadTriangleCount()
    {
        int[] triCount = { 0 };
        ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
        _trianglesCountBuffer.GetData(triCount);
        return triCount[0];
    }
    Mesh CreateMeshFromTriangles(Triangle[] triangles)
    {
        Vector3[] verts = new Vector3[triangles.Length * 3];
        int[] tris = new int[triangles.Length * 3];
/*        Vector2[] uv2 = new Vector2[triangles.Length * 3];*/

        for (int i = 0; i < triangles.Length; i++)
        {
            int startIndex = i * 3;
            verts[startIndex] = triangles[i].a;
            verts[startIndex + 1] = triangles[i].b;
            verts[startIndex + 2] = triangles[i].c;
            tris[startIndex] = startIndex;
            tris[startIndex + 1] = startIndex + 1;
            tris[startIndex + 2] = startIndex + 2;
/*            uv2[startIndex] = new Vector2(triangles[i].a.x / 32, triangles[i].a.z / 32);
            uv2[startIndex + 1] = new Vector2(triangles[i].b.x / 32, triangles[i].b.z / 32);
            uv2[startIndex + 2] = new Vector2(triangles[i].c.x / 32, triangles[i].c.z / 32);*/
        }
        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.triangles = tris;
/*        _mesh.uv = uv2;
        _mesh.uv2 = uv2;
        _mesh.uv3 = uv2;*/
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        return _mesh; 
    }
    void UpdateMesh()
    {
        Mesh mesh = ConstructMesh();
        GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
        MeshCollider.sharedMesh = mesh;
    }
    public void EditWeights(Vector3 hitPosition, float brushSize, bool add)
    {
        int kernel = MarchingShader.FindKernel("UpdateWeights");

        _weightsBuffer.SetData(_weights);

        MarchingShader.SetBuffer(kernel, "_Weights", _weightsBuffer);
        MarchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        MarchingShader.SetInt("_ChunkScale", GridMetrics.ChunkScale);
        MarchingShader.SetVector("_HitPosition", hitPosition - transform.position);
        MarchingShader.SetFloat("_BrushSize", brushSize);
        MarchingShader.SetFloat("_TerraformStrength", add ? 1f : -1f);

        MarchingShader.Dispatch(kernel
            , GridMetrics.PointsPerChunk / GridMetrics.NumThreads
            , GridMetrics.PointsPerChunk / GridMetrics.NumThreads
            , GridMetrics.PointsPerChunk / GridMetrics.NumThreads);

        _weightsBuffer.GetData(_weights);
        UpdateMesh();
    }

    /*    void Start()
        {
            _weights = NoiseGenerator.GetNoise();

            _weights = new float[GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk];
            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = Random.value;
            }
        }*/
    /*private void OnDrawGizmos()
    {
        if (_weights == null || _weights.Length == 0)
        {
            return;
        }
        for (int x = 0; x < GridMetrics.PointsPerChunk; x++)
        {
            for (int y = 0; y < GridMetrics.PointsPerChunk; y++)
            {
                for (int z = 0; z < GridMetrics.PointsPerChunk; z++)
                {
                    int index = x + GridMetrics.PointsPerChunk * (y + GridMetrics.PointsPerChunk * z);
                    float noiseValue = _weights[index];
                    Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
                    Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * .2f);
                }
            }
        }
    }*/
}
