using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;

[CustomEditor(typeof(SpriteSceneLoader))]
public class SpriteSceneLoaderEditor : Editor
{
    string[] sceneNames;
    int selectedIndex;

    void OnEnable()
    {
        // Obtiene todas las escenas añadidas en Build Settings
        var scenes = EditorBuildSettings.scenes;
        List<string> names = new List<string>();

        foreach (var scene in scenes)
        {
            string path = scene.path;
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            names.Add(name);
        }

        sceneNames = names.ToArray();

        SpriteSceneLoader loader = (SpriteSceneLoader)target;
        selectedIndex = System.Array.IndexOf(sceneNames, loader.sceneName);
        if (selectedIndex == -1) selectedIndex = 0;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SpriteSceneLoader loader = (SpriteSceneLoader)target;

        EditorGUILayout.LabelField("Escena a cargar:");
        selectedIndex = EditorGUILayout.Popup(selectedIndex, sceneNames);
        loader.sceneName = sceneNames[selectedIndex];

        if (GUI.changed)
        {
            EditorUtility.SetDirty(loader);
        }
    }
}
