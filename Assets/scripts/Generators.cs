using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generators : MonoBehaviour
{
    public int MapWidth;
    public int MapHeight;

    public float[,] falloffMap;

    public MapDisplay display;

    public SettingsToGenerators settings;

    public virtual void DrawMapInEditor() { print("Is this work?"); }

    public float[,] getNoiseMap(Vector2 addOffset)
    {
        float[,] map = Noise.GenerateNoiseMap(MapWidth, MapHeight, settings.minHeight, settings.maxHeight, settings.Seed, settings.NoiseScale, settings.Octaves, settings.Persistance, settings.Lacunarity, settings.Offset + addOffset, settings.isChunk);
        if(settings.isFalloff)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    map[x, y] = Mathf.Clamp01(map[x, y] - falloffMap[x, y]);
                }
            }
        }
        return map;
    }
}
