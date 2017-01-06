using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generators : MonoBehaviour
{   public float[,] falloffMap;

    public SettingsToGenerators settings;

    public virtual void DrawMapInEditor() {}

    public virtual Size getSize() { return new Size(0, 0); }

    public float[,] getNoiseMap(int width, int height, Vector2 addOffset)
    {
        float[,] map = Noise.GenerateNoiseMap(width, height, settings.minHeight, settings.maxHeight, settings.Seed, settings.NoiseScale, settings.Octaves, settings.Persistance, settings.Lacunarity, settings.Offset + addOffset, settings.isChunk);

        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        if (settings.isFalloff)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    map[x, y] = Mathf.Clamp01(map[x, y] - falloffMap[x, y]);
                }
            }
        }
        return map;
    }
}

public struct Size
{
    public int width;
    public int height;
    public Size (int x, int y)
    {
        width = x;
        height = y;
    }
};