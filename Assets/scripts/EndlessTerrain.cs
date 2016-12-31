using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    private const float viewerMoveTresholdForChunkUpdate = 25f;

    private const float sqrViewerMoveTresholdForChunkUpdate = viewerMoveTresholdForChunkUpdate*viewerMoveTresholdForChunkUpdate;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material chunkMaterial;

    public Transform chunkParent;
    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    private static ChunkGenerator chunkGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDst;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    void Start()
    {
        chunkGenerator = FindObjectOfType<ChunkGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstTreshold;
        chunkSize = ChunkGenerator.chunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance/chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveTresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }
    
    void UpdateVisibleChunks()
    {
        foreach (TerrainChunk chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY+yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, chunkParent, chunkMaterial));
                }

            }
        }
    }

    public class TerrainChunk
    {
        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private LODInfo[] detailLevels;
        private LODMesh[] lodMeshes;

        private ChunkData chunkData;
        private bool chunkDataRecieved;

        private int previousLODIndex = -1;


        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels,  Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord*size;
            bounds = new Bounds(position, Vector2.one*size);
            Vector3 positionV3 = new Vector3( position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            lodMeshes = new LODMesh[this.detailLevels.Length];
            for (int i = 0; i < this.detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(this.detailLevels[i].lod, UpdateTerrainChunk);
            }

            chunkGenerator.RequestChunkData(position, OnChunkDataRecived);
        }

        void OnChunkDataRecived(ChunkData chunkData)
        {
            this.chunkData = chunkData;
            chunkDataRecieved = true;



            Texture2D texture = TextureGenerator.TextureFromColorMap(ChunkGenerator.chunkSize, ChunkGenerator.chunkSize, chunkData.colorMap, ChunkGenerator.FilterMode);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (chunkDataRecieved)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < this.detailLevels.Length - 1; i++) 
                    {
                        if (viewerDistanceFromNearestEdge > this.detailLevels[i].visibleDstTreshold)
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
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(chunkData);
                        }
                    }

                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;

        private System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CretaeMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(ChunkData chunkData)
        {
            hasRequestedMesh = true;
            chunkGenerator.RequestMeshData(chunkData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstTreshold;
    }

}
