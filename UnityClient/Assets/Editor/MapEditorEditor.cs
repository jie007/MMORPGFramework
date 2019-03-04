using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapEditorBehaviour))]
public class MapEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapEditorBehaviour mapEditor = (MapEditorBehaviour)target;
        if (GUILayout.Button("Save Map"))
        {
            var map = mapEditor.GetMap();

            string json = JsonConvert.SerializeObject(map);

            File.WriteAllText(Path.Combine(Application.dataPath, "StreamingAssets", map.MapName + ".json"), json);
        }
    }
}
