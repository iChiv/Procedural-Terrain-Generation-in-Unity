using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    
    public LODInfo[] detailLevels;
    public static float MaxViewDistance;
    
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPos;
    private Vector2 viewerPosOld;
    private static MapGenerator _mapGenerator;
    private int _chunkSize;
    private int _chunkVisibleInViewDistance;
    private Dictionary<Vector2, TerrainChunk> TerrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        _mapGenerator = FindObjectOfType<MapGenerator>();
        
        MaxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        
        _chunkSize = MapGenerator.MapChunkSize - 1;
        _chunkVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / _chunkSize);
        
        UpdateVisibleChunks ();
    }

    private void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);
        
        if ((viewerPosOld - viewerPos).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            viewerPosOld = viewerPos;
            UpdateVisibleChunks ();
        }
    }

    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) 
        {
            terrainChunksVisibleLastUpdate [i].SetVisible (false);
        }
        terrainChunksVisibleLastUpdate.Clear ();
			
        int currentChunkCoordX = Mathf.RoundToInt (viewerPos.x / _chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt (viewerPos.y / _chunkSize);

        for (int yOffset = -_chunkVisibleInViewDistance; yOffset <= _chunkVisibleInViewDistance; yOffset++) 
        {
            for (int xOffset = -_chunkVisibleInViewDistance; xOffset <= _chunkVisibleInViewDistance; xOffset++) 
            {
                Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (TerrainChunkDic.ContainsKey (viewedChunkCoord)) 
                {
                    TerrainChunkDic [viewedChunkCoord].UpdateTerrainChunk ();
                    // if (TerrainChunkDic [viewedChunkCoord].IsVisible ()) 
                    // {
                    //     terrainChunksVisibleLastUpdate.Add (TerrainChunkDic [viewedChunkCoord]);
                    // }
                } 
                else 
                {
                    TerrainChunkDic.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, _chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }
    
    public class TerrainChunk
    {
        private GameObject _meshObject;
        private Vector2 _position;
        private Bounds Bounds;

        private MapDataStruct _mapDataStruct;
        private bool _mapDataReceived;
        private int previousLODIndex = -1;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        private LODInfo[] _detailLevels;
        private LODMesh[] _lodMeshes;
        
        
        public TerrainChunk(Vector2 cord, int size,LODInfo[] detailLevels, Transform parent, Material material)
        {
            this._detailLevels = detailLevels;
            
            _position = cord * size;
            Bounds = new Bounds(_position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(_position.x, 0, _position.y);

            _meshObject = new GameObject("Terrain Chunk");
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshRenderer.material = material;
            
            _meshObject.transform.position = positionV3;
            _meshObject.transform.parent = parent;
            // _meshObject.transform.localScale = Vector3.one ; 
            SetVisible(false);

            _lodMeshes = new LODMesh[_detailLevels.Length];
            for (int i = 0; i < _detailLevels.Length; i++)
            {
                _lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }
            
            _mapGenerator.RequestMapData(_position,onMapReceived);
        }

        void onMapReceived(MapDataStruct mapDataStruct)
        {
            this._mapDataStruct = mapDataStruct;
            _mapDataReceived = true;

            Texture2D texture2D = TextureGenerator.TextureFromColorMap(mapDataStruct.colorMap,
                MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
            _meshRenderer.material.mainTexture = texture2D;
            
            UpdateTerrainChunk();
            
            
        }

        public void UpdateTerrainChunk()
        {
            if (_mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(Bounds.SqrDistance(viewerPos));
                bool visible = viewerDistanceFromNearestEdge <= MaxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < _detailLevels.Length - 1 ; i++)
                    {
                        if (viewerDistanceFromNearestEdge > _detailLevels[i].visibleDistThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = _lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            _meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(_mapDataStruct);
                        }
                    }
                    terrainChunksVisibleLastUpdate.Add(this);
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }
        
        public bool IsVisible() 
        {
            return _meshObject.activeSelf;
        }
    }
    
    class LODMesh 
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh ();
            hasMesh = true;

            updateCallback ();
        }

        public void RequestMesh(MapDataStruct mapDataStruct) {
            hasRequestedMesh = true;
            _mapGenerator.RequestMeshData (mapDataStruct, lod, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDistThreshold;
    }
}
