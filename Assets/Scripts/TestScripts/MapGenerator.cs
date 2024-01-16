using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
    }

    public DrawMode drawMode;
    
    public Noise.NormalizeMode normalizeMode;

    public const int MapChunkSize = 241;
    [FormerlySerializedAs("levelOfDetail")] [Range(0,6)]public int previewLevelOfDetail;
    
    // public int mapWidth;
    // public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    private Queue<MapThreadInfoStruct<MapDataStruct>> mapDataThreadInfoQueue = 
        new Queue<MapThreadInfoStruct<MapDataStruct>>();

    private Queue<MapThreadInfoStruct<MeshData>> meshDataThreadInfoQueue =
        new Queue<MapThreadInfoStruct<MeshData>>();
    
    public void DrawMapInEditor()
    {
        MapDataStruct mapData = GenerateMapData(Vector2.zero);
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        else if(drawMode == DrawMode.ColorMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
        else if (drawMode == DrawMode.Mesh)
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, previewLevelOfDetail),TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize) );
    }

    public void RequestMapData(Vector2 center ,Action<MapDataStruct> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapDataStruct> callback)
    {
        MapDataStruct mapDataStruct = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfoStruct<MapDataStruct>(callback,mapDataStruct));
        }
    }

    public void RequestMeshData(MapDataStruct mapDataStruct,int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread (mapDataStruct, lod, callback);
        };

        new Thread (threadStart).Start ();
    }

    void MeshDataThread(MapDataStruct mapDataStruct,int lod , Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapDataStruct.heightMap, meshHeightMultiplier,
            meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfoStruct<MeshData>(callback,meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfoStruct<MapDataStruct> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfoStruct<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapDataStruct GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, seed, noiseScale,octaves,persistance,lacunarity, center+offset,normalizeMode);

        Color[] colorMap = new Color[MapChunkSize*MapChunkSize];
        
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * MapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        
        return new MapDataStruct (noiseMap, colorMap);
    }

    private void OnValidate()
    {
        // if (MapChunkSize < 1)
        //     MapChunkSize = 1;
        //
        // if (MapChunkSize < 1)
        //     MapChunkSize = 1;
        
        if (lacunarity < 1)
            lacunarity = 1;
        
        if (octaves < 0)
            octaves = 0;
    }
    
    struct MapThreadInfo<T> 
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
		
    }

    struct MapThreadInfoStruct<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfoStruct(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
    
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapDataStruct
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapDataStruct (float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
