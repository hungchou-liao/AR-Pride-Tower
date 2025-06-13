using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FileOrganizer : EditorWindow
{
    [MenuItem("Tools/AR Foundation/Organize Files")]
    public static void ShowWindow()
    {
        GetWindow<FileOrganizer>("File Organizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("AR Foundation File Organizer", EditorStyles.boldLabel);

        if (GUILayout.Button("Organize Files"))
        {
            OrganizeFiles();
        }
    }

    private void OrganizeFiles()
    {
        // Create necessary directories
        CreateDirectories();

        // Move FBX files to Models folder
        MoveFiles("Assets/Resources/Prefabs", "Assets/Models", "*.fbx");

        // Move optimized prefabs to AR_Objects folder
        MoveFiles("Assets/Resources/Prefabs", "Assets/Resources/Prefabs/AR_Objects", "*_Optimized.prefab");

        // Refresh the asset database
        AssetDatabase.Refresh();

        Debug.Log("File organization complete!");
    }

    private void CreateDirectories()
    {
        string[] directories = new string[]
        {
            "Assets/Models",
            "Assets/Resources/Prefabs/AR_Objects"
        };

        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log($"Created directory: {dir}");
            }
        }
    }

    private void MoveFiles(string sourceDir, string targetDir, string pattern)
    {
        if (!Directory.Exists(sourceDir) || !Directory.Exists(targetDir))
        {
            Debug.LogError($"Source or target directory does not exist!");
            return;
        }

        string[] files = Directory.GetFiles(sourceDir, pattern);
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string targetPath = Path.Combine(targetDir, fileName);

            // Skip if file already exists in target
            if (File.Exists(targetPath))
            {
                Debug.Log($"File already exists in target: {fileName}");
                continue;
            }

            // Move the file
            File.Move(file, targetPath);
            Debug.Log($"Moved {fileName} to {targetDir}");
        }
    }
}