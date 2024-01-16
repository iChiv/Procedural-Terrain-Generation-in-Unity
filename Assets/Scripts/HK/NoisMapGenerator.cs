using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoisMapGenerator : MonoBehaviour
{
    
    public static NoisMapGenerator instance;

    public enum TerrainType { PerlinNoise, MidPointDisplacement}
    public TerrainType terrainType;

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
    public int size; //only 2^n is valid
    public float roughness;
    public float meshHeightMultiplier2;



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

    void Start()
    {
        SetDefaltValueForPerlinNoise();
        SetDefaltValueForMidPointDisplacement();
        
        GenerateMap();
    }

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.G))
        // {
        //     GenerateMap();
        // }

         GenerateMap();
    }

    //generate map
    public void GenerateMap()
    {
        float[,] noiseMap = PerlinNoise.instance.GeneratePerlinNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] heightMap = MidPointDisplacement.instance.MidpointDisplacement(size, roughness, noiseScale, seed);
        Debug.Log(heightMap.Length);
        Color[] colourMap = new Color[mapWidth * mapHeight];

        //set shader and texture
        float minHeight = meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        float maxHeight = meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        SetMaterial(terrainMaterial, minHeight, maxHeight);
        ApplyToMaterial(terrainMaterial);
        
        switch(terrainType)
        {
            case TerrainType.PerlinNoise:

                DrawMeshWithMaterial(HK_MeshGenerator.instance.GenerateTrrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), terrainMaterial);
                break;

            case TerrainType.MidPointDisplacement:

                DrawMeshWithMaterial(HK_MeshGenerator.instance.GenerateTrrainMesh(heightMap, meshHeightMultiplier2, meshHeightCurve, levelOfDetail), terrainMaterial);
                break;
        }
    }

    public void SetDefaltValueForPerlinNoise()
    {
        mapWidth = 128;
        mapHeight = 128;
        noiseScale = 20f;
        octaves = 4;
        persistance = 0.5f;
        lacunarity = 2f;
        seed = 0;
        offset = new Vector2(0, 0);
        meshHeightMultiplier = 30f;
    }

    public void SetDefaltValueForMidPointDisplacement()
    {
        size = 128;
        roughness = 0.5f;
        meshHeightMultiplier2 = 50f;
    }

    public void DrawMeshWithMaterial(MeshData meshData, Material material)
    {
        meshFilter.sharedMesh = meshData.CreatMesh();
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


