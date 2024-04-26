using UnityEditor;
using UnityEngine;

public static class EConfigFinderUtility
{
    public static void UpdateConfigHolder(EConfigHolder holder, bool excludePackages)
    {
        if (holder == null)
            return;

        holder.configs.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            if (excludePackages && assetPath.StartsWith("Packages/"))
                continue;

            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (asset != null && !(asset is EConfigHolder))
            {
                holder.configs.Add(asset);
            }
        }

        EditorUtility.SetDirty(holder);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
