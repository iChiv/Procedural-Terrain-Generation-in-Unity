using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    
    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth-1) * (meshHeight-1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public void FlatShading() {
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++) {
			flatShadedVertices [i] = vertices [triangles [i]];
			flatShadedUvs [i] = uvs [triangles [i]];
			triangles [i] = i;
		}

		vertices = flatShadedVertices;
		uvs = flatShadedUvs;
	}

    public Mesh CreatMesh()
    {
        Mesh mesh = new Mesh ();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateBounds();
        mesh.RecalculateNormals ();
        return mesh;
    }
}
public class HK_MeshGenerator : MonoBehaviour
{
    public static HK_MeshGenerator instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    public MeshData GenerateTrrainMesh(float[,] heightMap, float multiplier, AnimationCurve heightCurve, int levelOfDetail)
    {

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail==0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement +1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);

        int i = 0;

        for(int y = 0; y < width; y += meshSimplificationIncrement)
        {
            for(int x = 0; x < height; x += meshSimplificationIncrement)
            {
                meshData.vertices[i] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * multiplier, topLeftZ - y);
                meshData.uvs[i] = new Vector2(x / (float)width, y / (float)height);

                if(x < width-1 && y < height-1)
                {
                    meshData.AddTriangle(i, i + verticesPerLine + 1, i + verticesPerLine);
                    meshData.AddTriangle(i + verticesPerLine + 1, i, i + 1);
                }
                i++;
            }
        }
        return meshData;
    }
}
