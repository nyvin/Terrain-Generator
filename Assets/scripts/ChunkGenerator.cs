using UnityEngine;
using System.Collections;

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
    [Range(0, 6)] public int levelOfDetail;
    public bool FilterMode;
    public Terrain[] TerrainTypes;
    public MapDisplay display;

    public void DrawMapInEditor()
    {
        ChunkData chunkData = GenerateChunkData();
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
                display.DrawMesh( MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, Settings.MesAnimationCurve, Settings.MeshMultipler, levelOfDetail), mapTexture);
                break;
         }
}

    private ChunkData GenerateChunkData()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize, chunkSize, Settings.minHeight, Settings.maxHeight, Settings.Seed, Settings.NoiseScale, Settings.Octaves, Settings.Persistance, Settings.Lacunarity, Settings.Offset);
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
}

public struct ChunkData
{
    public float[,] heightMap;
    public Color[] colorMap;

    public ChunkData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
};