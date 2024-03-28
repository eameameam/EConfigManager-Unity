using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ESelectConfigsWindow : EditorWindow
{
    EConfigHolder _eConfigHolder;
    Vector2 _scrollPosition;
    bool _excludePackages = true;
    
    
    public static void ShowWindow()
    {
        GetWindow<ESelectConfigsWindow>("Select Configs");
    }

    void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;

        _eConfigHolder = AssetDatabase.LoadAssetAtPath<EConfigHolder>("Assets/EConfigHolder.asset");
        if (_eConfigHolder == null)
        {
            _eConfigHolder = CreateInstance<EConfigHolder>();
            AssetDatabase.CreateAsset(_eConfigHolder, "Assets/EConfigHolder.asset");
            AssetDatabase.SaveAssets();
        }
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        if (EConfigManagerWindow.Instance == null)
        {
            Close();
        }
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        DrawHeaderSection();
        DrawToggle();
        DrawDragAndDropArea();
        DrawConfigurations();
    }

    void DrawHeaderSection()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();
        GUILayout.Label("Select Configs Here", EditorStyles.largeLabel);
        GUILayout.Label("You can drag and drop configs here,\nOr set them manually.", EditorStyles.miniLabel);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        DrawActionButtons();
        GUILayout.Space(30);
        GUILayout.EndHorizontal();
    }

    void DrawActionButtons()
    {
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
        if (GUILayout.Button("Clean\nList", GUILayout.Height(40), GUILayout.Width(50)))
        {
            _eConfigHolder.configs.Clear();
            EConfigManagerWindow.TriggerConfigsChanged();
        }

        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("ALL", GUILayout.Height(30), GUILayout.Width(50)))
        {
            EConfigFinderUtility.UpdateConfigHolder(_eConfigHolder, _excludePackages);
            EConfigManagerWindow.TriggerConfigsChanged();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndVertical();
    }

    void DrawToggle()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Only /Assets", EditorStyles.whiteMiniLabel);
        _excludePackages = EditorGUILayout.Toggle(_excludePackages, GUILayout.Width(20));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    void DrawDragAndDropArea()
    {
        GUILayout.Space(10);
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\nDrag Configs Here To Add To The List");
        HandleDragAndDrop(dropArea);
    }

    void DrawConfigurations()
    {
        GUILayout.Space(10);

        if (_eConfigHolder.configs.Count == 0)
        {
            GUILayout.Label("No Configs Found", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        int? removeIndex = null;
        for (int i = 0; i < _eConfigHolder.configs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _eConfigHolder.configs[i] = (ScriptableObject)EditorGUILayout.ObjectField(
                _eConfigHolder.configs[i], typeof(ScriptableObject), false);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex.HasValue)
        {
            _eConfigHolder.configs.RemoveAt(removeIndex.Value);
            EConfigManagerWindow.TriggerConfigsChanged();
        }
        EditorGUILayout.EndScrollView();
    }

    void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (!dropArea.Contains(evt.mousePosition)) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                bool changed = false;
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is ScriptableObject scriptableObject && !_eConfigHolder.configs.Contains(scriptableObject))
                    {
                        _eConfigHolder.configs.Add(scriptableObject);
                        changed = true;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(_eConfigHolder);
                    EConfigManagerWindow.TriggerConfigsChanged();
                }
            }
        }
    }
}
