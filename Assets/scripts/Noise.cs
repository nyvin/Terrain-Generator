using System;
using UnityEngine;
using System.Collections;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float minHeight, float maxHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, bool isChunk)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0f;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        float localMaximumNoiseHeight = float.MinValue;
        float localMinimumNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        float heightDifference = maxHeight - minHeight;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > localMaximumNoiseHeight)
                {
                    localMaximumNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < localMinimumNoiseHeight)
                {
                    localMinimumNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(!isChunk)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(localMinimumNoiseHeight, localMaximumNoiseHeight, noiseMap[x, y]) * heightDifference + minHeight;
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue) * heightDifference + minHeight;
                }
            }
        }

        return noiseMap;
    }
}
