using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ConfigGrouping
{
    public static Dictionary<string, List<ScriptableObject>> GroupConfigs(List<ScriptableObject> configs, ConfigSorter.SortType groupBy)
    {
        switch (groupBy)
        {
            case ConfigSorter.SortType.Name:
                return configs.GroupBy(config => config.name.Substring(0, 1).ToUpper()).ToDictionary(group => group.Key, group => group.ToList());
            case ConfigSorter.SortType.DateModified:
                return configs.GroupBy(config =>
                {
                    var path = AssetDatabase.GetAssetPath(config);
                    return string.IsNullOrEmpty(path) ? "Unknown Date" : File.GetLastWriteTime(path).ToString("yyyy-MM");
                }).ToDictionary(group => group.Key, group => group.ToList());
            case ConfigSorter.SortType.ScriptType:
                return configs.GroupBy(config => config.GetType().Name).ToDictionary(group => group.Key, group => group.ToList());
            default:
                return new Dictionary<string, List<ScriptableObject>>();
        }
    }
}