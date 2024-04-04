using System;
using UnityEngine;
using UnityEditor;
using static ConfigSorter;
using System.Collections.Generic;

public static class EConfigGUI
{
    private static bool CheckForMissingFields(SerializedObject serializedConfig)
    {
        SerializedProperty iterator = serializedConfig.GetIterator();
        while (iterator.NextVisible(true))
        {
            if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue == null)
            {
                return true;
            }
        }
        return false;
    }

    private static GUIStyle CreateFoldoutStyle(bool hasMissingField)
    {
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        if (hasMissingField)
        {
            foldoutStyle.normal.textColor = Color.red;
            foldoutStyle.onNormal.textColor = Color.red;
        }
        return foldoutStyle;
    }

    public static bool DrawFoldoutHeader(ScriptableObject config, GUIStyle foldoutStyle, Dictionary<ScriptableObject, bool> foldouts)
    {
        bool foldoutState = foldouts.TryGetValue(config, out bool currentState) ? currentState : false;

        Rect foldoutRect = GUILayoutUtility.GetRect(1f, 17f);
        const float arrowWidth = 16f;

        if (Event.current.type == EventType.Repaint)
        {
            foldoutStyle.Draw(foldoutRect, new GUIContent($"{config.name} ({config.GetType().Name})"), false, false, foldoutState, false);
        }

        Rect arrowRect = new Rect(foldoutRect.x, foldoutRect.y, arrowWidth, foldoutRect.height);
        if (Event.current.type == EventType.MouseDown && arrowRect.Contains(Event.current.mousePosition))
        {
            foldoutState = !foldoutState;
            foldouts[config] = foldoutState;
            Event.current.Use();
        }

        Rect titleRect = new Rect(foldoutRect.x + arrowWidth, foldoutRect.y, foldoutRect.width - arrowWidth, foldoutRect.height);
        if (Event.current.type == EventType.MouseDown && titleRect.Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;
            Event.current.Use();
        }

        return foldoutState;
    }

    public static void DrawProperties(SerializedObject serializedConfig, ScriptableObject config, Dictionary<ScriptableObject, bool> foldouts)
    {
        string assetPath = AssetDatabase.GetAssetPath(config);
        bool foldoutState = foldouts[config];

        bool hasMissingField = CheckForMissingFields(serializedConfig);

        GUIStyle foldoutStyle = CreateFoldoutStyle(hasMissingField);

        foldoutState = DrawFoldoutHeader(config, foldoutStyle, foldouts);

        if (foldoutState)
        {
            serializedConfig.Update();
            SerializedProperty iterator = serializedConfig.GetIterator();
            iterator.NextVisible(true);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Restore", GUILayout.Width(60)))
            {
                BackupManager.RestoreBackup(assetPath);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            do
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            while (iterator.NextVisible(false));
            GUILayout.EndVertical();

            if (serializedConfig.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(serializedConfig.targetObject);
            }
        }

        foldouts[config] = foldoutState;
    }

    public static void DrawSortOptions(ref SortType currentSortType, ref bool ascending, ref bool grouping, Action sortConfigsCallback)
    {
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Toggle(currentSortType == SortType.Name, "Name", "Button"))
        {
            if (currentSortType != SortType.Name)
            {
                currentSortType = SortType.Name;
                sortConfigsCallback.Invoke();
                GUI.changed = true; 
            }
        }

        if (GUILayout.Toggle(currentSortType == SortType.DateModified, "Date Modified", "Button"))
        {
            if (currentSortType != SortType.DateModified)
            {
                currentSortType = SortType.DateModified;
                sortConfigsCallback.Invoke();
                GUI.changed = true; 
            }
        }

        if (GUILayout.Toggle(currentSortType == SortType.ScriptType, "Script Type", "Button"))
        {
            if (currentSortType != SortType.ScriptType)
            {
                currentSortType = SortType.ScriptType;
                sortConfigsCallback.Invoke();
                GUI.changed = true; 
            }
        }

        grouping = GUILayout.Toggle(grouping, "\u2630", "Button", GUILayout.Width(20));

        bool prevAscending = ascending;
        ascending = GUILayout.Toggle(ascending, ascending ? "\u25B2" : "\u25BC", "Button", GUILayout.Width(20));
        if (ascending != prevAscending)
        {
            sortConfigsCallback.Invoke();
            GUI.changed = true; 
        }

        GUILayout.EndHorizontal();
    }
}
