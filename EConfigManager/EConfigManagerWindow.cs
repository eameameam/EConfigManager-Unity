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
    Dictionary<string, bool> _groupFoldouts = new Dictionary<string, bool>();

    ConfigSorter.SortType _currentSortType = ConfigSorter.SortType.Name;
    bool _ascending = true;
    bool _grouping = false;
    bool _configsSorted = true;
    
    public static EConfigManagerWindow Instance { get; set; }
    public delegate void ConfigsChangedDelegate();
    public static event ConfigsChangedDelegate OnConfigsChanged;

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

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical(GUILayout.Width(30), GUILayout.Height(30));
        GUILayout.Label("Backup", EditorStyles.whiteLabel);
        if (GUILayout.Button("Save"))
        {
            if (EditorUtility.DisplayDialog("Save Backup", "Are you sure you want to save the backup?", "Save", "Cancel"))
            {
                BackupManager.SaveBackup();
                Debug.Log("Backup saved.");
            }
        }
        if (GUILayout.Button("Restore"))
        {
            if (EditorUtility.DisplayDialog("Restore Backup for all configs", "Are you sure you want to restore the backup for all configs in the list?", "Restore", "Cancel"))
            {
                BackupManager.RestoreBackup();
                Debug.Log("Backup loaded.");
            }
        }
        GUILayout.EndVertical();

        GUILayout.Space(10); 

        GUILayout.EndHorizontal();

        GUILayout.Label("Configs", EditorStyles.helpBox);

        if (!(_eConfigHolder.configs.Count > 0))
        {
            GUILayout.Space(20);

            GUILayout.Label("No Configs Found", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        EConfigGUI.DrawSortOptions(ref _currentSortType, ref _ascending, ref _grouping,() => SortConfigs());

        if (!_configsSorted)
        {
            SortConfigs();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        bool wasGrouping = _grouping;
        if (_eConfigHolder.configs.Count > 0)
        {
            EditorGUI.indentLevel = 0;
            Color originalBackgroundColor = GUI.backgroundColor;

            if (_grouping)
            {
                var groupedConfigs = ConfigGrouping.GroupConfigs(_eConfigHolder.configs, _currentSortType);
                foreach (var groupKey in groupedConfigs.Keys)
                {
                    bool foldout = _groupFoldouts.TryGetValue(groupKey, out bool currentFoldoutState) ? currentFoldoutState : false;
                    foldout = EditorGUILayout.Foldout(foldout, groupKey, true);
                    _groupFoldouts[groupKey] = foldout;

                    if (foldout)
                    {
                        EditorGUI.indentLevel++;
                        
                        var backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.gray;

                        foreach (var config in groupedConfigs[groupKey])
                        {
                            if (config != null)
                            {
                                SerializedObject serializedConfig = _serializedConfigs[config];
                                EditorGUILayout.BeginVertical("box");
                                EConfigGUI.DrawProperties(serializedConfig, config, _foldouts);
                                EditorGUILayout.EndVertical();
                            }
                        }
                        
                        EditorGUI.indentLevel--;
                        GUI.backgroundColor = backgroundColor;
                    }
                }
            }
            else
            {
                foreach (var config in _eConfigHolder.configs)
                {
                    if (config != null)
                    {
                        SerializedObject serializedConfig = _serializedConfigs[config];
                        EditorGUILayout.BeginVertical("box");
                        EConfigGUI.DrawProperties(serializedConfig, config, _foldouts);
                        EditorGUILayout.EndVertical();
                    }
                }
            }

            if (wasGrouping != _grouping || EditorGUI.indentLevel != 0 || originalBackgroundColor != GUI.backgroundColor)
            {
                Debug.LogError("GUI state has changed. Resetting...");
                EditorGUI.indentLevel = 0;
                GUI.backgroundColor = originalBackgroundColor;
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
        ConfigSorter.SortConfigs(_eConfigHolder.configs, _currentSortType, _ascending);
        _configsSorted = true;
        EditorUtility.SetDirty(_eConfigHolder);
    }

    void HandleConfigsChanged()
    {
        LoadConfigs();
        Repaint();
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
