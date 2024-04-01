using System;
using UnityEngine;
using UnityEditor;
using static ConfigSorter;
using System.Collections.Generic;

public static class EConfigGUI
{
    public static void DrawConfig(SerializedObject serializedConfig, Dictionary<ScriptableObject, bool> foldouts, Dictionary<ScriptableObject, SerializedObject> serializedConfigs, ScriptableObject config)
    {
        bool hasMissingField = CheckForMissingFields(serializedConfig);
        GUIStyle foldoutStyle = CreateFoldoutStyle(hasMissingField);

        bool foldoutState = DrawFoldoutHeader(config, foldoutStyle, foldouts);

        if (foldoutState)
        {
            DrawProperties(serializedConfig);
        }
    }

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

    private static void DrawProperties(SerializedObject serializedConfig)
    {
        serializedConfig.Update();
        SerializedProperty iterator = serializedConfig.GetIterator();
        iterator.NextVisible(true);
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

    public static void DrawSortOptions(ref SortType currentSortType, ref bool ascending, Action sortConfigsCallback)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Name"))
        {
            currentSortType = SortType.Name;
            sortConfigsCallback.Invoke();
        }
        if (GUILayout.Button("Date Modified"))
        {
            currentSortType = SortType.DateModified;
            sortConfigsCallback.Invoke();
        }
        if (GUILayout.Button("Script Type"))
        {
            currentSortType = SortType.ScriptType;
            sortConfigsCallback.Invoke();
        }
        ascending = GUILayout.Toggle(ascending, ascending ? "\u25B2" : "\u25BC", "Button", GUILayout.Width(20));
        if (GUI.changed)
        {
            sortConfigsCallback.Invoke();
        }
        GUILayout.EndHorizontal();
    }
}
