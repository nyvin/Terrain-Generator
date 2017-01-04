using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsToGenerators : MonoBehaviour
{
    public Generators generator;

    public enum MapType
    {
        HeightMap,
        ColorMap,
        MeshMap,
        FalloffMap
    };
    public MapType TypeOfMap;

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


    [Range(0, 10)]
    public float falloffParamA;
    [Range(0, 10)]
    public float falloffParamB;

    public bool isMeshColored;
    public bool AutoUpdate;

    public bool isChunk;
    public bool isFalloff;

    public Terrain[] TerrainTypes;

    [System.Serializable]
    public struct Terrain
    {
        public string Name;
        public float Height;
        public Color Color;
    }

    void OnValidate()
    {
        if (NoiseScale < 0)
        {
            NoiseScale = 0;
        }
        if (Lacunarity < 1)
        {
            Lacunarity = 1;
        }
        if (Octaves < 0)
        {
            Octaves = 0;
        }
        generator.falloffMap = FalloffGenerator.GenerateFalloffMap(generator.MapWidth, generator.MapHeight, falloffParamA, falloffParamB);
    }
};
