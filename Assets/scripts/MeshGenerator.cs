using UnityEngine;
using System.Collections;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, AnimationCurve highCurve, float meshMultipler, int levelOfDetail)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1)/-2f;
        float topLeftZ = (height - 1)/2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail*2;
        int verticiesPerLine = (width - 1)/meshSimplificationIncrement + 1;
        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;


        for (int y = 0; y < height; y+= meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement)
            {
                meshData.Vertices[vertexIndex] = new Vector3( topLeftX + x, highCurve.Evaluate(heightMap[x,y]) * meshMultipler, topLeftZ - y);
                meshData.Uvs[vertexIndex] = new Vector2(x/(float) width, y/(float) height);
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticiesPerLine + 1, vertexIndex + verticiesPerLine);
                    meshData.AddTriangle(vertexIndex + verticiesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector2[] Uvs;
    private int _triangleIndex;

    public MeshData(int width, int height)
    {
        Vertices = new Vector3[width * height];
        Uvs = new Vector2[width * height];
        Triangles = new int[(width - 1)*(height - 1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        Triangles[_triangleIndex] = a;
        Triangles[_triangleIndex + 1] = b;
        Triangles[_triangleIndex + 2] = c;

        _triangleIndex += 3;
    }

    public Mesh CretaeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.uv = Uvs;
        mesh.triangles = Triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}