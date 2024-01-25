using System.Collections.Generic;
using UnityEngine;

public class GraphicDrawer : Singleton<GraphicDrawer>
{
    public Mesh drawMesh;
    private Dictionary<Material, Stack<Matrix4x4>> drawData;

    void Start()
    {
        drawData = new Dictionary<Material, Stack<Matrix4x4>>();
    }

    void Update()
    {
        DrawMeshes();
    }

    private void DrawMeshes()
    {
        foreach (KeyValuePair<Material, Stack<Matrix4x4>> draw in drawData)
        {
            //Debug.Log($"drawData Material -> {drawData.drawMaterial.name} TRS count -> {drawData.TRSs.Count}");

            RenderParams rp = new RenderParams(draw.Key);

            while(draw.Value.Count > 0)
            {            
                Graphics.RenderMesh(rp, drawMesh, 0, draw.Value.Pop());
            }
        }
    }

    public void AddInstance(Material material, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 TRS = Matrix4x4.TRS(position, rotation, scale);

        if (!drawData.ContainsKey(material)) drawData.Add(material, new Stack<Matrix4x4>());

        drawData[material].Push(TRS);
    }

    private Vector3 Matrix4x4GetPosition(Matrix4x4 m) => new Vector3(m[0, 3], m[1, 3], m[2, 3]);

    private Vector3 Matrix4x4GetScale(Matrix4x4 m) => new Vector3(m[0, 0], m[1, 1], m[2, 2]);

    private void Matrix4x4SetPosition(ref Matrix4x4 m, Vector3 position) => new Vector3(m[0, 3] = position.x, m[1, 3] = position.y, m[2, 3] = position.z);

    private void Matrix4x4SetScale(ref Matrix4x4 m, Vector3 scale) => new Vector3(m[0, 0] = scale.x, m[1, 1] = scale.y, m[2, 2] = scale.z);

    private Matrix4x4 GetTransformTRSMatrix(Transform transform) => Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);

    private Bounds TRSToBounds(Matrix4x4 TRS) => new Bounds(Matrix4x4GetPosition(TRS), Matrix4x4GetScale(TRS));

    private bool IsLayerRendered(Camera camera, int layer) => ((camera.cullingMask & (1 << layer)) != 0);

}
