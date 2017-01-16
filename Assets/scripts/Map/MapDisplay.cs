using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour
{

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData dataOfMesh, Texture2D texture, bool isCollider)
    {
        Mesh mesh = dataOfMesh.CretaeMesh();
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial.mainTexture = texture;
        if(isCollider)
        {
            meshCollider.sharedMesh = mesh;
        }
    }
}