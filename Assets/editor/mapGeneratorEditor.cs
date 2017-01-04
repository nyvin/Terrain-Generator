using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SettingsToGenerators))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SettingsToGenerators set = (SettingsToGenerators)target;
        if (DrawDefaultInspector())
        {
            if (set.AutoUpdate)
            {
                set.generator.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            set.generator.DrawMapInEditor();
        }
    }
}
