using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EConfigManagerWindow : EditorWindow
{
    EConfigHolder _eConfigHolder;
    Vector2 _scrollPosition;
    Dictionary<ScriptableObject, bool> _foldouts = new Dictionary<ScriptableObject, bool>();
    Dictionary<ScriptableObject, SerializedObject> _serializedConfigs = new Dictionary<ScriptableObject, SerializedObject>();
    SortType _currentSortType = SortType.Name;
    bool _ascending = true;
    bool _configsSorted = true;

    public static EConfigManagerWindow Instance { get; set; }
    public delegate void ConfigsChangedDelegate();
    public static event ConfigsChangedDelegate OnConfigsChanged;

    enum SortType
    {
        Name,
        DateModified
    }

    [MenuItem("Escripts/EConfig Manager")]
    public static void ShowWindow()
    {
        Instance = GetWindow<EConfigManagerWindow>("EConfig Manager");
        SubscribeToEvents();
    }

    void OnEnable()
    {
        Instance = this;
        LoadConfigs();
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("EConfig Manager", EditorStyles.largeLabel);
        GUILayout.Label("Firstly set your configs by pressing SET button,\nit will open a window that you can set your configs.", EditorStyles.miniLabel);
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
        if (GUILayout.Button("SET", GUILayout.Height(20), GUILayout.Width(40)))
        {
            ESelectConfigsWindow.ShowWindow();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);

        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.Label("Configs", EditorStyles.helpBox);


        if (!(_eConfigHolder.configs.Count > 0))
        {
            GUILayout.Space(20);

            GUILayout.Label("No Configs Found", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        DrawSortOptions();

        if (!_configsSorted)
        {
            SortConfigs();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        if (_eConfigHolder.configs.Count > 0)
        {
            foreach (var config in _eConfigHolder.configs)
            {
                if (config != null)
                {
                    DrawConfig(config);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void LoadConfigs()
    {
        _eConfigHolder = AssetDatabase.LoadAssetAtPath<EConfigHolder>("Assets/EConfigHolder.asset");
        InitializeFoldoutsAndSerializedObjects();
        SortConfigsIfNeeded();
    }

    void InitializeFoldoutsAndSerializedObjects()
    {
        foreach (var config in _eConfigHolder?.configs ?? Enumerable.Empty<ScriptableObject>())
        {
            if (config != null) 
            {
                _foldouts.TryAdd(config, false);
                _serializedConfigs.TryAdd(config, new SerializedObject(config));
            }
        }
    }

    void SortConfigsIfNeeded()
    {
        if (!_configsSorted) SortConfigs();
    }

    void SortConfigs()
    {
        var configs = _eConfigHolder.configs;
        switch (_currentSortType)
        {
            case SortType.Name:
                configs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal) * (_ascending ? 1 : -1));
                break;
            case SortType.DateModified:
                configs.Sort((a, b) => DateTime.Compare(File.GetLastWriteTime(AssetDatabase.GetAssetPath(a)), File.GetLastWriteTime(AssetDatabase.GetAssetPath(b))) * (_ascending ? 1 : -1));
                break;
        }
        _configsSorted = true;
        EditorUtility.SetDirty(_eConfigHolder);
    }

    void HandleConfigsChanged()
    {
        LoadConfigs();
        Repaint();
    }

    void DrawConfig(ScriptableObject config)
    {
        if (!_foldouts.ContainsKey(config) || !_serializedConfigs.ContainsKey(config)) return;

        _foldouts[config] = EditorGUILayout.Foldout(_foldouts[config], config.name);
        if (_foldouts[config])
        {
            SerializedObject serializedConfig = _serializedConfigs[config];
            SerializedProperty iterator = serializedConfig.GetIterator();
            GUILayout.BeginVertical("box");
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(iterator, true);
            }
            GUILayout.EndVertical();
            serializedConfig.ApplyModifiedProperties();

            if (serializedConfig.hasModifiedProperties)
            {
                EditorUtility.SetDirty(config);
            }
        }
    }

    void DrawSortOptions()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort by Name"))
        {
            _currentSortType = SortType.Name;
            SortConfigs();
            Repaint();
        }
        if (GUILayout.Button("Sort by Date Modified"))
        {
            _currentSortType = SortType.DateModified;
            SortConfigs();
            Repaint(); 
        }
        _ascending = GUILayout.Toggle(_ascending, _ascending ? "Ascending" : "Descending", "Button");
        if (GUI.changed)
        {
            SortConfigs();
            Repaint(); 
        }
        GUILayout.EndHorizontal();
    }

    static void SubscribeToEvents()
    {
        OnConfigsChanged -= Instance.HandleConfigsChanged; 
        OnConfigsChanged += Instance.HandleConfigsChanged;
    }
    static void UnsubscribeFromEvents()
    {
        OnConfigsChanged -= Instance.HandleConfigsChanged;
    }

    public static void TriggerConfigsChanged()
    {
        OnConfigsChanged?.Invoke();
    }
}
