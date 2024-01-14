using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Terrain terrain;
    public int terrainWidth = 256;
    public int terrainDepth = 256;
    public float heightMultiplier = 20f;
    public float smoothness = 0.01f;
    public float snowHeight = 0.8f; 

    public int octaves = 5;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    private float _xOffset;
    private float _zOffset;
    

    private void Start()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }

        _xOffset = Random.Range(0, 10000);
        _zOffset = Random.Range(0, 10000);
    }

    public void GenerateTerrain()
    {
        terrain.terrainData = GenerateTerrainData();
    }

    TerrainData GenerateTerrainData()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, heightMultiplier, terrainDepth);
        terrainData.SetHeights(0, 0, GenerateHeights());
        ApplyTexture(terrainData);
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[terrainWidth, terrainDepth];
        for (int x = 0; x < terrainWidth; x++)
        {
            for (int z = 0; z < terrainDepth; z++)
            {
                heights[x, z] = CalculateHeight(x, z);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int z)
    {
        float height = 0;
        float scale = smoothness;
        float amplitude = 1;
        float frequency = 1;
        
        for (int i = 0; i < octaves; i++)
        {
            float xCoord = (x + _xOffset) / terrainWidth * scale * frequency;
            float zCoord = (z + _zOffset) / terrainDepth * scale * frequency;

            height += Mathf.PerlinNoise(xCoord, zCoord) * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return height;
    }

    
    void ApplyTexture(TerrainData terrainData)
    {
        // var textureCount = terrainData.alphamapLayers;
        // Debug.Log("Number of terrain layers: " + textureCount);
        //
        // terrainData.alphamapResolution = terrainWidth;
        //
        // // 纹理数组
        // var splatTextures = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, textureCount];
        //
        // for (int y = 0; y < terrainData.alphamapHeight; y++)
        // {
        //     for (int x = 0; x < terrainData.alphamapWidth; x++)
        //     {
        //         float height = terrainData.GetHeight(y, x) / heightMultiplier;
        //         
        //         float[] splat = new float[textureCount];
        //         if (height < 0.2f) // 海洋
        //         {
        //             splat[0] = 1;
        //         }
        //         else if (height < 0.6f) // 草地
        //         {
        //             splat[1] = 1;
        //         }
        //         else if (height < snowHeight) // 山脉
        //         {
        //             splat[2] = 1;
        //         }
        //         else // 雪
        //         {
        //             splat[3] = 1;
        //         }
        //
        //         for (int i = 0; i < textureCount; i++)
        //         {
        //             splatTextures[x, y, i] = splat[i];
        //         }
        //     }
        // }
        //
        // terrainData.SetAlphamaps(0, 0, splatTextures);
    }
}
