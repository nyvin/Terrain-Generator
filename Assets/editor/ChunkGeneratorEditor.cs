using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ChunkGenerator))]
public class ChunkEditor : Editor
{

    public override void OnInspectorGUI()
    {
        ChunkGenerator mapGen = (ChunkGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.Settings.AutoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
