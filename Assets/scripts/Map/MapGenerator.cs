using System;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEditorInternal;

public class MapGenerator : Generators
{
    public int mapWidth;
    public int mapHeight;

    public MapDisplay display;

    public override Size getSize()
    {
        return new Size(mapWidth, mapHeight);
    }

    public override void DrawMapInEditor()
    {
        float[,] noiseMap = getNoiseMap(mapWidth, mapHeight, Vector2.zero);
        
        Texture2D mapTexture = Texture2D.whiteTexture;

        switch (settings.TypeOfMap)
        {
            case SettingsToGenerators.MapType.HeightMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(noiseMap);
                display.DrawTexture(mapTexture);
                break;
            case SettingsToGenerators.MapType.ColorMap:
                mapTexture = TextureGenerator.TextureFromColorMap(mapWidth, mapHeight, GenereateColorMap(noiseMap));
                display.DrawTexture(mapTexture);
                break;
            case SettingsToGenerators.MapType.MeshMap:
                mapTexture = settings.isMeshColored ? TextureGenerator.TextureFromColorMap(mapWidth, mapHeight, GenereateColorMap(noiseMap)) : TextureGenerator.TextureFromHeightMap(noiseMap);
                display.DrawMesh(MapMeshGenerator.GenerateTerrainMesh(noiseMap, settings.MesAnimationCurve , settings.MeshMultipler, 0), mapTexture);
                break;
            case SettingsToGenerators.MapType.FalloffMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapWidth, mapHeight, settings.falloffParamA, settings.falloffParamB));
                display.DrawTexture(mapTexture);
                break;
        }
    }

    public Color[] GenereateColorMap(float[,] heightMap)
    {
        Color[] colorMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int i = 0; i < settings.TerrainTypes.Length; i++)
                {
                    if (heightMap[x, y] >= settings.TerrainTypes[i].Height)
                    {
                        colorMap[y * mapWidth + x] = settings.TerrainTypes[i].Color;
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

}

