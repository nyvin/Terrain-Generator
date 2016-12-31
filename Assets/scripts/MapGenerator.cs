using System;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEditorInternal;

public class MapGenerator : MonoBehaviour
{
    public enum MapType
    {
        HeightMap,
        ColorMap,
        MeshMap
    };

    public int MapWidth;
    public int MapHeight;

    public MapType TypeOfMap;
    public SettingsOfGenerator Settings;
    public bool FilterMode;
    public Terrain[] TerrainTypes;
    public MapDisplay display;
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapWidth, MapHeight, Settings.minHeight, Settings.maxHeight, Settings.Seed, Settings.NoiseScale, Settings.Octaves, Settings.Persistance, Settings.Lacunarity, Settings.Offset);
        
        Texture2D mapTexture = Texture2D.whiteTexture;

        switch (TypeOfMap)
        {
            case MapType.HeightMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(noiseMap, FilterMode);
                display.DrawTexture(mapTexture, FilterMode);
                break;
            case MapType.ColorMap:
                mapTexture = TextureGenerator.TextureFromColorMap(MapWidth, MapHeight, GenereateColorMap(noiseMap), FilterMode);
                display.DrawTexture(mapTexture, FilterMode);
                break;
            case MapType.MeshMap:
                mapTexture = Settings.isMeshColored ? TextureGenerator.TextureFromColorMap(MapWidth, MapHeight, GenereateColorMap(noiseMap), FilterMode) : TextureGenerator.TextureFromHeightMap(noiseMap, FilterMode);
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, Settings.MesAnimationCurve , Settings.MeshMultipler, 0), mapTexture);
                break;
        }
    }

    void OnValidate()
    {
        if (MapWidth < 1)
        {
            MapWidth = 1;
        }
        if (MapHeight < 1)
        {
            MapHeight = 1;
        }
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
        Color[] colorMap = new Color[MapWidth * MapHeight];

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                for (int i = 0; i < TerrainTypes.Length; i++)
                {
                    if (heightMap[x, y] <= TerrainTypes[i].Height)
                    {
                        colorMap[y * MapWidth + x] = TerrainTypes[i].Color;
                        break;
                    }
                }

            }
        }
        return colorMap;
    }

}

[System.Serializable]
public struct SettingsOfGenerator
{
    [Range(0, 1)]
    public float minHeight;
    [Range(0, 1)]
    public float maxHeight;

    public float NoiseScale;
    [Range(1, 10)]
    public int Octaves;
    [Range(0, 1)]
    public float Persistance;
    public float Lacunarity;

    public int Seed;
    public Vector2 Offset;

    public float MeshMultipler;
    public AnimationCurve MesAnimationCurve;
    public bool isMeshColored;
    public bool AutoUpdate;
}

[System.Serializable]
public struct Terrain
{
    public string Name;
    public float Height;
    public Color Color;
}

