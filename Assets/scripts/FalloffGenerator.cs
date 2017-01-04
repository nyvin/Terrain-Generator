using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int width, int height, float paramA, float paramB)
    /* paramA and paramB using to change curve:
        newVal(x) = x^A / (x^A + (B - Bx)^A)
    */
    {
        float[,] map = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float _x = x / (float)width * 2 - 1;
                float _y = y / (float)height * 2 - 1;
                float value = Mathf.Max(Mathf.Abs(_x), Mathf.Abs(_y));

                map[x, y] = Evaluate(value, paramA, paramB);
            }
        }
        return map;
    }

    private static float Evaluate(float value, float a, float b)
    {
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
