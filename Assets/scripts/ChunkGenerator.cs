using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class ChunkGenerator : Generators
{
    public const int chunkSize = 241;

    [Range(0, 6)] public int editorPreviewLOD;

    Queue<ChunkThreadInfo<ChunkData>> chunkDataThreadInfoQueue = new Queue<ChunkThreadInfo<ChunkData>>();
    Queue<ChunkThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<ChunkThreadInfo<MeshData>>();

    private void Awake()
    {
        MapWidth = chunkSize;
        MapHeight = chunkSize;
    }

    public override void DrawMapInEditor()
    {
        ChunkData chunkData = GenerateChunkData(Vector2.zero);
        Texture2D mapTexture = Texture2D.whiteTexture;
        switch (settings.TypeOfMap)
        {
            case SettingsToGenerators.MapType.HeightMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(chunkData.heightMap);
                display.DrawTexture(mapTexture);
                break;
            case SettingsToGenerators.MapType.ColorMap:
                mapTexture = TextureGenerator.TextureFromColorMap(chunkSize, chunkSize, chunkData.colorMap);
                display.DrawTexture(mapTexture);
                break;
            case SettingsToGenerators.MapType.MeshMap:
                mapTexture = settings.isMeshColored? TextureGenerator.TextureFromColorMap(chunkSize, chunkSize, chunkData.colorMap) : TextureGenerator.TextureFromHeightMap(chunkData.heightMap);
                display.DrawMesh( MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, settings.MesAnimationCurve, settings.MeshMultipler, editorPreviewLOD), mapTexture);
                break;
            case SettingsToGenerators.MapType.FalloffMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(chunkSize, chunkSize, settings.falloffParamA, settings.falloffParamB));
                display.DrawTexture(mapTexture);
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
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, settings.MesAnimationCurve, settings.MeshMultipler, lod);
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
        float[,] noiseMap = getNoiseMap(center);
        Color[] colorMap = GenereateColorMap(noiseMap);

        return new ChunkData(noiseMap, colorMap);
    }

    public Color[] GenereateColorMap(float[,] heightMap)
    {
        Color[] colorMap = new Color[chunkSize * chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int i = 0; i < settings.TerrainTypes.Length; i++)
                {
                    if (heightMap[x, y] >= settings.TerrainTypes[i].Height)
                    {
                        colorMap[y * chunkSize + x] = settings.TerrainTypes[i].Color;
                    }
                    else
                    {
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