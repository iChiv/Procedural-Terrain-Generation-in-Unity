using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoisMapGenerator : MonoBehaviour
{
    
    public static NoisMapGenerator instance;


    public int mapchunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    //height
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    //shader
    public Material terrainMaterial;

    //color
    public Color[] baseColours;
    [Range(0,1)]
    public float[] baseStartHeight;
    [Range(0,1)]
    public float[] baseBlends;

    //mid-point displacement
    public float roughness;



    //public TerrainType[] regions;

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    float savedMinHeight;
    float savedMaxHeight;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            GenerateMap();
        }
    }

    //generate map
    public void GenerateMap()
    {
        float[,] noiseMap = PerlinNoise.instance.GeneratePerlinNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] heightMap = MidPointDisplacement.instance.MidpointDisplacement(mapWidth, roughness, noiseScale);
        Debug.Log(heightMap.Length);
        Color[] colourMap = new Color[mapWidth * mapHeight];

        //set shader and texture
        float minHeight = meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        float maxHeight = meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        SetMaterial(terrainMaterial, minHeight, maxHeight);
        ApplyToMaterial(terrainMaterial);
        //DrawMeshWithMaterial(MeshGenerator.instance.GenerateTrrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), terrainMaterial);
        DrawMeshWithMaterial(HK_MeshGenerator.instance.GenerateTrrainMesh(heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), terrainMaterial);
    }

    public void DrawMeshWithMaterial(MeshData meshData, Material material)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial = material;
    }

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("baseColourCount", baseColours.Length);
        material.SetColorArray("baseColours", baseColours);
        material.SetFloatArray("baseStartHeight", baseStartHeight);

        SetMaterial(material, savedMinHeight, savedMaxHeight);
    }

    public void SetMaterial(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

}
public struct MapData {
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;

	public MapData (float[,] heightMap, Color[] colourMap)
	{
		this.heightMap = heightMap;
		this.colourMap = colourMap;
	}
}

