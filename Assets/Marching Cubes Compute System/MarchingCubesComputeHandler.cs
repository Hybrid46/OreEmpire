using UnityEngine;

public sealed class MarchingCubesComputeHandler
{
    private struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return a;
                    case 1: return b;
                    default: return c;
                }
            }
        }
    }

    const int threadGroupSize = 8;
    private int numPointsPerAxis;
    private float isoLevel;

    private ComputeShader marchingCompute;

    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    public MarchingCubesComputeHandler(ComputeShader marchingCompute, int numPointsPerAxis = 16 + 1, float isoLevel = 0.5f)
    {
        this.numPointsPerAxis = numPointsPerAxis;
        this.marchingCompute = marchingCompute;
        this.isoLevel = isoLevel;

        CreateBuffers();
    }

    public Mesh ComputeMesh(Vector4[] pointData)
    {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)threadGroupSize);

        // Points buffer is populated inside shader with pos (xyz) + density (w).
        pointsBuffer.SetData(pointData, 0, 0, pointData.Length);

        triangleBuffer.SetCounterValue(0);
        marchingCompute.SetBuffer(0, "points", pointsBuffer);
        marchingCompute.SetBuffer(0, "triangles", triangleBuffer);
        marchingCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
        marchingCompute.SetFloat("isoLevel", isoLevel);

        marchingCompute.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        return GenerateMesh(numTris, tris);
    }

    private Mesh GenerateMesh(int numTris, Triangle[] tris)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[numTris * 3];
        int[] meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();

        return mesh;
    }

    public void Destroy()
    {
        ReleaseBuffers();
    }

    private void CreateBuffers()
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        ReleaseBuffers();

        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    private void ReleaseBuffers()
    {
        if (triangleBuffer != null) triangleBuffer.Release();
        if (pointsBuffer != null) pointsBuffer.Release();
        if (triCountBuffer != null) triCountBuffer.Release();
    }
}
