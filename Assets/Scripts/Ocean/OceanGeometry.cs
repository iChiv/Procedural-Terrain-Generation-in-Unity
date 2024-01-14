using System.Collections.Generic;
using UnityEngine;

public class OceanGeometry : MonoBehaviour
{
    [SerializeField]
    WavesGenerator wavesGenerator;
    [SerializeField]
    Transform camera;
    [SerializeField]
    Material oceanMaterial;

    [SerializeField]
    float lengthScale = 10;
    [SerializeField, Range(1, 40)]
    int vertexDensity = 30;

    Element mesh;

    private void Start()
    {
        if (camera == null)
            camera = Camera.main.transform;

        // 设置材质的纹理
        oceanMaterial.SetTexture("_Displacement_c0", wavesGenerator.cascade0.Displacement);
        oceanMaterial.SetTexture("_Derivatives_c0", wavesGenerator.cascade0.Derivatives);
        oceanMaterial.SetTexture("_Turbulence_c0", wavesGenerator.cascade0.Turbulence);

        oceanMaterial.SetTexture("_Displacement_c1", wavesGenerator.cascade1.Displacement);
        oceanMaterial.SetTexture("_Derivatives_c1", wavesGenerator.cascade1.Derivatives);
        oceanMaterial.SetTexture("_Turbulence_c1", wavesGenerator.cascade1.Turbulence);

        oceanMaterial.SetTexture("_Displacement_c2", wavesGenerator.cascade2.Displacement);
        oceanMaterial.SetTexture("_Derivatives_c2", wavesGenerator.cascade2.Derivatives);
        oceanMaterial.SetTexture("_Turbulence_c2", wavesGenerator.cascade2.Turbulence);

        InstantiateMeshes();
    }

    int GridSize()
    {
        return 4 * vertexDensity + 1;
    }

    Vector3 Snap(Vector3 coords, float scale)
    {
        coords.x = Mathf.Floor(coords.x / scale) * scale;
        coords.z = Mathf.Floor(coords.z / scale) * scale;
        coords.y = 0;
        return coords;
    }

    void InstantiateMeshes()
    {
        // 清除既有的子对象
        foreach (var child in gameObject.GetComponentsInChildren<Transform>())
        {
            if (child != transform)
                Destroy(child.gameObject);
        }

        // 创建中心网格
        int k = GridSize();
        mesh = InstantiateElement("Ocean Plane", CreatePlaneMesh(k, k, 50), oceanMaterial);
    }

    Element InstantiateElement(string name, Mesh mesh, Material mat)
    {
        GameObject go = new GameObject();
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = true;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
        meshRenderer.material = mat;
        meshRenderer.allowOcclusionWhenDynamic = false;
        return new Element(go.transform, meshRenderer);
    }

    Mesh CreatePlaneMesh(int width, int height, float lengthScale)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Ocean Plane";
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];

        for (int i = 0, y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                vertices[i] = new Vector3(x, 0, y) * lengthScale;
            }
        }

        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++)
        {
            for (int x = 0; x < width; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    class Element
    {
        public Transform Transform;
        public MeshRenderer MeshRenderer;

        public Element(Transform transform, MeshRenderer meshRenderer)
        {
            Transform = transform;
            MeshRenderer = meshRenderer;
        }
    }
}