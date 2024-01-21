using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GraphicDrawer : Singleton<GraphicDrawer>
{
    public Mesh drawMesh;
    [SerializeField] private List<DrawData> drawInstances;
    private Dictionary<Material, DrawData> drawInstancesLUT;

    [SerializeField, HideInInspector] private Bounds renderBoundingBox; // All TRS's bounds

    [SerializeField] private ComputeBuffer meshPropertiesBuffer;
    [SerializeField] private ComputeBuffer argsBuffer;

    [Serializable]
    public struct DrawData
    {
        public Material drawMaterial;
        public List<Matrix4x4> positions;

        public DrawData(Material drawMaterial, List<Matrix4x4> positions)
        {
            this.drawMaterial = drawMaterial;
            this.positions = positions;
        }
    }

    void Start()
    {
        drawInstancesLUT = new Dictionary<Material, DrawData>(drawInstances.Count);

        drawInstances.ForEach(drawSprite =>
        {
            if (!drawInstancesLUT.ContainsKey(drawSprite.drawMaterial)) drawInstancesLUT.Add(drawSprite.drawMaterial, drawSprite);
        });
    }

    void Update()
    {
        DrawInstances();
    }

    private void DrawInstances()
    {
        drawInstances.ForEach(drawData =>
        {
            if (AreComputeBuffersInitialized()) DisposeComputeBuffers();
            renderBoundingBox = new Bounds(Vector3.zero, new Vector3(10f, 10f, 10f));

            int population = drawData.positions.Count;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetBuffer("_DrawData", meshPropertiesBuffer);

            //Args buffer initialization
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)drawMesh.GetIndexCount(0);
            args[1] = (uint)population;
            args[2] = (uint)drawMesh.GetIndexStart(0);
            args[3] = (uint)drawMesh.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);

            //Fill the compute buffer which transporting the TRS matrices
            meshPropertiesBuffer = new ComputeBuffer(population, sizeof(float) * 4 * 4);  //Marshal.SizeOf<DrawData.positions>()
            meshPropertiesBuffer.SetData(drawData.positions.ToArray());
            drawData.drawMaterial.SetBuffer("_DrawData", meshPropertiesBuffer);

            Graphics.DrawMeshInstancedIndirect(drawMesh, 0, drawData.drawMaterial, renderBoundingBox, argsBuffer, 0, mpb, ShadowCastingMode.Off, false, gameObject.layer, null, LightProbeUsage.Off, null);

            drawData.positions.Clear();
        });
    }

    public void AddInstance(Material material, Vector3 position) => AddInstance(material, position, Vector3.one);

    public void AddInstance(Material material, Vector3 position, Vector3 scale)
    {
        Matrix4x4 TRS = Matrix4x4.identity;
        Matrix4x4SetPosition(ref TRS, position);
        Matrix4x4SetScale(ref TRS, scale);

        if (!drawInstancesLUT.ContainsKey(material))
        {
            drawInstances.Add(new DrawData(material, new List<Matrix4x4>()));
            drawInstancesLUT.Add(material, drawInstances[drawInstances.Count - 1]);
        }

        drawInstancesLUT[material].positions.Add(TRS);
    }

    private Vector3 Matrix4x4GetPosition(Matrix4x4 m) => new Vector3(m[0, 3], m[1, 3], m[2, 3]);

    private Vector3 Matrix4x4GetScale(Matrix4x4 m) => new Vector3(m[0, 0], m[1, 1], m[2, 2]);

    private void Matrix4x4SetPosition(ref Matrix4x4 m, Vector3 position) => new Vector3(m[0, 3] = position.x, m[1, 3] = position.y, m[2, 3] = position.z);

    private void Matrix4x4SetScale(ref Matrix4x4 m, Vector3 scale) => new Vector3(m[0, 0] = scale.x, m[1, 1] = scale.y, m[2, 2] = scale.z);

    private Matrix4x4 GetTransformTRSMatrix(Transform transform) => Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);

    private bool IsLayerRendered(Camera camera, int layer) => ((camera.cullingMask & (1 << layer)) != 0);

    private void OnDisable()
    {
        DisposeComputeBuffers();
    }

    private void OnDestroy()
    {
        DisposeComputeBuffers();
    }

    private void DisposeComputeBuffers()
    {
        if (meshPropertiesBuffer != null) meshPropertiesBuffer.Release();
        meshPropertiesBuffer = null;

        if (argsBuffer != null) argsBuffer.Release();
        argsBuffer = null;
    }

    private bool AreComputeBuffersInitialized()
    {
        if (argsBuffer == null || meshPropertiesBuffer == null ||
            argsBuffer.count == 0 || meshPropertiesBuffer.count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
