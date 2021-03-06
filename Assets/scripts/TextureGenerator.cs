﻿using UnityEngine;
using System.Collections;

public static class TextureGenerator {

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
     {
         int width = heightMap.GetLength(0);
         int height = heightMap.GetLength(1);

         Texture2D texture = new Texture2D(width, height);

         Color[] colorMap = new Color[width * height];

         for (int y = 0; y < height; y++)
         {
             for (int x = 0; x < width; x++)
             {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
             }
         }

         texture.SetPixels(colorMap);
         texture.Apply();
         texture.filterMode = FilterMode.Point;
         texture.wrapMode = TextureWrapMode.Clamp;

         return texture;
     }

     public static Texture2D TextureFromColorMap(int width, int height, Color[] colorMap)
     {
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colorMap);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        return texture;
    }
}
