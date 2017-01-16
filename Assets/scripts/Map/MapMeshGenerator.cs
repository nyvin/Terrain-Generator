using UnityEngine;
using System.Collections;

public static class MapMeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, AnimationCurve _heightCurve, float meshMultipler, int levelOfDetail, bool useFlatShading)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1)/-2f;
        float topLeftZ = (height - 1)/2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail*2;
        int verticiesPerLine = (width - 1)/meshSimplificationIncrement + 1;
        MeshData meshData = new MeshData(width, height, useFlatShading);
        int vertexIndex = 0;


        for (int y = 0; y < height; y+= meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3( topLeftX + x, heightCurve.Evaluate(heightMap[x,y]) * meshMultipler, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x/(float) width, y/(float) height);
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
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    private int _triangleIndex;

    bool useFlatShading;

    public MeshData(int width, int height, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        vertices = new Vector3[width * height];
        uvs = new Vector2[width * height];
        triangles = new int[(width - 1)*(height - 1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[_triangleIndex] = a;
        triangles[_triangleIndex + 1] = b;
        triangles[_triangleIndex + 2] = c;

        _triangleIndex += 3;
    }

    void FlatShading()
    {
        Vector3[] flatShadedVerices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVerices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVerices;
        uvs = flatShadedUvs;
    }

    public Mesh CretaeMesh()
    {
        Mesh mesh = new Mesh();
        if(useFlatShading)
        {
            FlatShading();
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}