using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class FilesConverterExtension : EditorWindow
{
    private string normalizedFileName;
    private string newFileName;
    private string levelId;
    private Biomes biomeType;
    private Difficulty difficulty;

    [MenuItem("Tools/Files converter extension")]
    private static void ShowWindow()
    {
        GetWindow<FilesConverterExtension>("Files converter extension");
    }

    private void OnGUI()
    {
        DrawGUI();
    }

    private void DrawGUI()
    {
        EditorGUILayout.LabelField("Normalized file has to be in Assets/Maps");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Normalized file name");
        normalizedFileName = EditorGUILayout.TextField(normalizedFileName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New file name");
        newFileName = EditorGUILayout.TextField(newFileName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Level ID");
        levelId = EditorGUILayout.TextField(levelId);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Biome");
        biomeType = (Biomes)EditorGUILayout.EnumPopup(biomeType);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Difficulty");
        difficulty = (Difficulty)EditorGUILayout.EnumPopup(difficulty);
        EditorGUILayout.EndHorizontal();

        if (!File.Exists(Path.Combine(Application.dataPath, "Maps", normalizedFileName)) || string.IsNullOrEmpty(levelId) 
            || string.IsNullOrEmpty(newFileName))
            GUI.enabled = false;

        if (GUILayout.Button("Convert"))
        {
            MapSerializer serializer = new MapSerializer(Path.Combine(Application.dataPath, "Maps", newFileName));
            serializer.ConvertNormalizedFile(Path.Combine(Application.dataPath, "Maps", normalizedFileName), levelId, biomeType, difficulty);
        }

        GUI.enabled = false;
    }   
}
