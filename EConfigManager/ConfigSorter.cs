using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ConfigSorter
{
    public enum SortType
    {
        Name,
        DateModified,
        ScriptType
    }
    
    public static void SortConfigs(List<ScriptableObject> configs, SortType sortType, bool ascending)
    {
        configs.RemoveAll(item => item == null); // Remove any null entries
        switch (sortType)
        {
            case SortType.Name:
                configs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal) * (ascending ? 1 : -1));
                break;
            case SortType.DateModified:
                configs.Sort((a, b) =>
                {
                    var pathA = AssetDatabase.GetAssetPath(a);
                    var pathB = AssetDatabase.GetAssetPath(b);
                    var timeA = string.IsNullOrEmpty(pathA) ? DateTime.MinValue : File.GetLastWriteTime(pathA);
                    var timeB = string.IsNullOrEmpty(pathB) ? DateTime.MinValue : File.GetLastWriteTime(pathB);
                    return DateTime.Compare(timeA, timeB) * (ascending ? 1 : -1);
                });
                break;
            case SortType.ScriptType:
                configs.Sort((a, b) => string.Compare(a.GetType().ToString(), b.GetType().ToString(), StringComparison.Ordinal) * (ascending ? 1 : -1));
                break;
        }
    }
}
