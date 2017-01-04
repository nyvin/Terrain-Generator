using System;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using UnityEditorInternal;

public class MapGenerator : Generators
{
    public override void DrawMapInEditor()
    {
        float[,] noiseMap = getNoiseMap(Vector2.zero);
        
        Texture2D mapTexture = Texture2D.whiteTexture;

        switch (settings.TypeOfMap)
        {
            case SettingsToGenerators.MapType.HeightMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(noiseMap);
                display.DrawTexture(mapTexture);
                break;
            case SettingsToGenerators.MapType.ColorMap:
                mapTexture = TextureGenerator.TextureFromColorMap(MapWidth, MapHeight, GenereateColorMap(noiseMap));
                display.DrawTexture(mapTexture);
                break;
            case SettingsToGenerators.MapType.MeshMap:
                mapTexture = settings.isMeshColored ? TextureGenerator.TextureFromColorMap(MapWidth, MapHeight, GenereateColorMap(noiseMap)) : TextureGenerator.TextureFromHeightMap(noiseMap);
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, settings.MesAnimationCurve , settings.MeshMultipler, 0), mapTexture);
                break;
            case SettingsToGenerators.MapType.FalloffMap:
                mapTexture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(MapWidth, MapHeight, settings.falloffParamA, settings.falloffParamB));
                display.DrawTexture(mapTexture);
                break;
        }
    }

    public Color[] GenereateColorMap(float[,] heightMap)
    {
        Color[] colorMap = new Color[MapWidth * MapHeight];

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                for (int i = 0; i < settings.TerrainTypes.Length; i++)
                {
                    if (heightMap[x, y] >= settings.TerrainTypes[i].Height)
                    {
                        colorMap[y * MapWidth + x] = settings.TerrainTypes[i].Color;
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

