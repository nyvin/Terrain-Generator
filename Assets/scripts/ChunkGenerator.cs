using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class ChunkGenerator : MonoBehaviour
{

    public enum MapType
    {
        HeightMap,
        ColorMap,
        MeshMap
    };

    public const int chunkSize = 241;

    public MapType TypeOfMap;
    public SettingsOfGenerator Settings;
    [Range(0, 6)] public int editorPreviewLOD;
    public static bool FilterMode;
    public Terrain[] TerrainTypes;
    public MapDisplay display;

    Queue<ChunkThreadInfo<ChunkData>> chunkDataThreadInfoQueue = new Queue<ChunkThreadInfo<ChunkData>>();
    Queue<ChunkThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<ChunkThreadInfo<MeshData>>();
    public void DrawMapInEditor()
    {
        ChunkData chunkData = GenerateChunkData(Vector2.zero);
        Texture2D mapTexture = Texture2D.whiteTexture;
        switch (TypeOfMap)
        {
            case MapType.HeightMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(chunkData.heightMap, FilterMode);
                display.DrawTexture(mapTexture, FilterMode);
                break;
            case MapType.ColorMap:
                mapTexture = TextureGenerator.TextureFromColorMap(chunkSize, chunkSize, chunkData.colorMap, FilterMode);
                display.DrawTexture(mapTexture, FilterMode);
                break;
            case MapType.MeshMap:
                mapTexture = Settings.isMeshColored? TextureGenerator.TextureFromColorMap(chunkSize, chunkSize, chunkData.colorMap, FilterMode) : TextureGenerator.TextureFromHeightMap(chunkData.heightMap, FilterMode);
                display.DrawMesh( MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, Settings.MesAnimationCurve, Settings.MeshMultipler, editorPreviewLOD), mapTexture);
                break;
         }
    }

    public void RequestChunkData(Vector2 center, Action<ChunkData> callback)
    {
        ThreadStart threadStart = delegate
        {
            ChunkDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void ChunkDataThread(Vector2 center, Action<ChunkData> callback)
    {
        ChunkData chunkData = GenerateChunkData(center);
        lock (chunkDataThreadInfoQueue)
        {
            chunkDataThreadInfoQueue.Enqueue(new ChunkThreadInfo<ChunkData>(callback, chunkData));
        }
    }

    public void RequestMeshData(ChunkData chunkData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(chunkData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(ChunkData chunkData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, Settings.MesAnimationCurve, Settings.MeshMultipler, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new ChunkThreadInfo<MeshData>(callback, meshData));
        }
    }



    void Update()
    {
        if (chunkDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < chunkDataThreadInfoQueue.Count; i++)
            {
                ChunkThreadInfo<ChunkData> threadInfo = chunkDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                ChunkThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
}


    private ChunkData GenerateChunkData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize, chunkSize, Settings.minHeight, Settings.maxHeight, Settings.Seed, Settings.NoiseScale, Settings.Octaves, Settings.Persistance, Settings.Lacunarity, Settings.Offset + center);
        Color[] colorMap = GenereateColorMap(noiseMap);

        return new ChunkData(noiseMap, colorMap);
    }

    void OnValidate()
    {
        if (Settings.NoiseScale < 0)
        {
            Settings.NoiseScale = 0;
        }
        if (Settings.Lacunarity < 1)
        {
            Settings.Lacunarity = 1;
        }
        if (Settings.Octaves < 0)
        {
            Settings.Octaves = 0;
        }
    }

    public Color[] GenereateColorMap(float[,] heightMap)
    {
        Color[] colorMap = new Color[chunkSize * chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int i = 0; i < TerrainTypes.Length; i++)
                {
                    if (heightMap[x, y] <= TerrainTypes[i].Height)
                    {
                        colorMap[y * chunkSize + x] = TerrainTypes[i].Color;
                        break;
                    }
                }

            }
        }
        return colorMap;
    }

    struct ChunkThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public ChunkThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

public struct ChunkData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public ChunkData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
};