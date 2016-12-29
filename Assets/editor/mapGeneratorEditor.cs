using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof (MapGenerator))]
public class NewBehaviourScript : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator) target;

        if (DrawDefaultInspector())
        {
            if (mapGen.Settings.AutoUpdate)
            {
                mapGen.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}
