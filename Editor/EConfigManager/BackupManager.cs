using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BackupManager
{
    static string BackupFilePath => Path.Combine(Application.dataPath, "ScriptableObjectBackup.json");

    [System.Serializable]
    public class ReferenceData { public string propertyPath; public string assetPath; }
    [System.Serializable]
    public class BackupEntry { public string assetPath; public List<ReferenceData> references; }
    [System.Serializable]
    public class BackupWrapper { public List<BackupEntry> backups = new List<BackupEntry>(); }

    public static void SaveBackup()
    {
        var eConfigHolder = AssetDatabase.LoadAssetAtPath<EConfigHolder>("Assets/EConfigHolder.asset");
        if (eConfigHolder == null) { Debug.LogError("EConfigHolder.asset not found."); return; }
        
        var backupWrapper = new BackupWrapper();
        ProcessConfigurations(eConfigHolder, backupWrapper);
        SaveToJson(backupWrapper);
    }

    private static void ProcessConfigurations(EConfigHolder holder, BackupWrapper wrapper)
    {
        foreach (var config in holder.configs)
        {
            if (config == null) continue;
            var referencesList = ProcessSerializedObject(new SerializedObject(config));
            if (referencesList.Count > 0)
            {
                wrapper.backups.Add(new BackupEntry { assetPath = AssetDatabase.GetAssetPath(config), references = referencesList });
            }
        }
    }

    private static List<ReferenceData> ProcessSerializedObject(SerializedObject serializedObject)
    {
        var referencesList = new List<ReferenceData>();
        var serializedProperty = serializedObject.GetIterator();
        bool isFirstProperty = true;
        
        while (serializedProperty.NextVisible(true))
        {
            if (isFirstProperty && serializedProperty.propertyPath.Equals("m_Script", StringComparison.Ordinal))
            {
                isFirstProperty = false;
                continue;
            }
            isFirstProperty = false;
            
            if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference && serializedProperty.objectReferenceValue != null)
            {
                string referencePath = AssetDatabase.GetAssetPath(serializedProperty.objectReferenceValue);
                referencesList.Add(new ReferenceData
                {
                    propertyPath = serializedProperty.propertyPath,
                    assetPath = referencePath
                });
            }
        }

        return referencesList;
    }

    private static void SaveToJson(BackupWrapper wrapper)
    {
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(BackupFilePath, json);
        Debug.Log($"Backup saved to {BackupFilePath}");
    }

    public static void RestoreBackup()
    {
        RestoreBackupInternal();
    }
    public static void RestoreBackup(string assetPath = null)
    {
        if (!File.Exists(BackupFilePath)) { Debug.LogError("Backup file not found."); return; }
        
        var backupWrapper = JsonUtility.FromJson<BackupWrapper>(File.ReadAllText(BackupFilePath));
        bool restoreAll = string.IsNullOrEmpty(assetPath);
        
        foreach (var entry in backupWrapper.backups)
        {
            if (restoreAll || entry.assetPath.Equals(assetPath, StringComparison.OrdinalIgnoreCase))
            {
                var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(entry.assetPath);
                if (config == null) { Debug.LogError($"Asset not found: {entry.assetPath}"); continue; }
                RestoreReferences(new SerializedObject(config), entry.references);

                if (!restoreAll)
                    break;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log(restoreAll ? "All backups restored." : $"Backup restored for {assetPath}.");
    }

    private static void RestoreBackupInternal(string specificAssetPath = null)
    {
        if (!File.Exists(BackupFilePath)) { Debug.LogError("Backup file not found."); return; }
        var backupWrapper = JsonUtility.FromJson<BackupWrapper>(File.ReadAllText(BackupFilePath));
        
        foreach (var entry in backupWrapper.backups)
        {
            if (!string.IsNullOrEmpty(specificAssetPath) && !entry.assetPath.Equals(specificAssetPath, StringComparison.OrdinalIgnoreCase))
                continue;

            var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(entry.assetPath);
            if (config == null) { Debug.LogError($"Asset not found: {entry.assetPath}"); continue; }
            RestoreReferences(new SerializedObject(config), entry.references);

            if (!string.IsNullOrEmpty(specificAssetPath))
                break;
        }

        if (!string.IsNullOrEmpty(specificAssetPath))
            Debug.Log($"Backup restored for {specificAssetPath}.");
        else
            Debug.Log("Backup restored for all assets.");
    }

    private static void RestoreReferences(SerializedObject serializedObject, List<ReferenceData> references)
    {
        foreach (var reference in references)
        {
            var prop = serializedObject.FindProperty(reference.propertyPath);
            if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(reference.assetPath);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    public static List<string> GetAvailableBackups()
    {
        if (!File.Exists(BackupFilePath)) { Debug.LogError("Backup file not found."); return new List<string>(); }
        var backupWrapper = JsonUtility.FromJson<BackupWrapper>(File.ReadAllText(BackupFilePath));
        List<string> availableBackups = new List<string>();
        foreach (var entry in backupWrapper.backups)
        {
            availableBackups.Add(entry.assetPath);
        }
        return availableBackups;
    }
}
