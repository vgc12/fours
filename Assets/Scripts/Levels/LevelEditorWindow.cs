
#if UNITY_EDITOR
using Board;
using UnityEditor;
using UnityEngine;

namespace Levels
{
    public sealed class LevelEditorWindow : EditorWindow
    {
        private LevelData _currentLevel;
        private Vector2 _scrollPosition;
        private int _gridRows = 4;
        private int _gridColumns = 4;
        private float _cellSize = 50f;
        private Color _currentColor = Color.white;
        private GameObject _squarePrefab;
        private SpriteGrid _targetGrid;
        
        private bool _isDragging;
        private bool _isErasing;
        
        // Grid editing mode
        private enum EditMode { Initial, Target, Both }
        private EditMode _editMode = EditMode.Initial;
        
        // Inactive square settings
        private Color _inactiveSquareColor = new(0.2f, 0.2f, 0.2f, 0.5f);
        private bool _showInactiveSquares = true;
        private bool _fillEmptyWithInactive = false;
        
        // Foldout states
        private bool _showLevelSettings = true;
        private bool _showGridSettings = true;
        private bool _showEditMode = true;
        private bool _showInactiveSettings = false;
        private bool _showColorPalette = true;
        
        // Color palette
        private readonly Color[] _colorPalette = {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            new(1f, 0.5f, 0f), // Orange
            Color.magenta,
            Color.cyan,
            Color.white
        };
        
        [MenuItem("Tools/Board/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(500, 600);
        }
        
        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab Square");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _squarePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            DrawLevelSettings();
            DrawGridSettings();
            DrawEditModeSettings();
            DrawInactiveSquareSettings();
            DrawColorPalette();
            DrawGrids();
            DrawActions();
            
            DrawSaveButton();
            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawLevelSettings()
        {
            _showLevelSettings = EditorGUILayout.Foldout(_showLevelSettings, "Level Settings", true);
            if (!_showLevelSettings) return;
            
            EditorGUILayout.BeginVertical("box");
            
            _currentLevel = (LevelData)EditorGUILayout.ObjectField("Current Level", _currentLevel, typeof(LevelData), false);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New Level", GUILayout.Height(25)))
            {
                CreateNewLevel();
            }
            if (GUILayout.Button("Load Level", GUILayout.Height(25)) && _currentLevel != null)
            {
                LoadLevel();
            }
            EditorGUILayout.EndHorizontal();
            
            if (_currentLevel != null)
            {
                int initialActive = _currentLevel.GetActiveSquares(false).Count;
                int initialTotal = _currentLevel.initialSquares.Count;
                int targetActive = _currentLevel.GetActiveSquares(true).Count;
                int targetTotal = _currentLevel.targetSquares.Count;
                
                EditorGUILayout.HelpBox(
                    $"Initial - Active: {initialActive} | Total: {initialTotal}\n" +
                    $"Target - Active: {targetActive} | Total: {targetTotal}", 
                    MessageType.None);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawGridSettings()
        {
            _showGridSettings = EditorGUILayout.Foldout(_showGridSettings, "Grid Settings", true);
            if (!_showGridSettings) return;
            
            EditorGUILayout.BeginVertical("box");
            
            _gridRows = EditorGUILayout.IntSlider("Rows", _gridRows, 2, 10);
            _gridColumns = EditorGUILayout.IntSlider("Columns", _gridColumns, 2, 10);
            _cellSize = EditorGUILayout.Slider("Cell Size", _cellSize, 25f, 60f);
            
            EditorGUILayout.Space(3);
            
            _squarePrefab = (GameObject)EditorGUILayout.ObjectField("Square Prefab", _squarePrefab, typeof(GameObject), false);
            _targetGrid = (SpriteGrid)EditorGUILayout.ObjectField("Target Grid", _targetGrid, typeof(SpriteGrid), true);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawEditModeSettings()
        {
            _showEditMode = EditorGUILayout.Foldout(_showEditMode, "Edit Mode", true);
            if (!_showEditMode) return;
            
            EditorGUILayout.BeginVertical("box");
            
            _editMode = (EditMode)EditorGUILayout.EnumPopup("Editing", _editMode);
            
            EditorGUILayout.HelpBox(
                "Initial: Starting grid state\n" +
                "Target: Goal grid state\n" +
                "Both: Edit both grids simultaneously",
                MessageType.Info);
            
            // Copy buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Initial → Target", GUILayout.Height(25)))
            {
                if (_currentLevel != null)
                {
                    _currentLevel.CopyInitialToTarget();
                    EditorUtility.SetDirty(_currentLevel);
                }
            }
            if (GUILayout.Button("Copy Target → Initial", GUILayout.Height(25)))
            {
                if (_currentLevel != null)
                {
                    _currentLevel.CopyTargetToInitial();
                    EditorUtility.SetDirty(_currentLevel);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawInactiveSquareSettings()
        {
            _showInactiveSettings = EditorGUILayout.Foldout(_showInactiveSettings, "Inactive Square Settings", true);
            if (!_showInactiveSettings) return;
            
            EditorGUILayout.BeginVertical("box");
            
            _fillEmptyWithInactive = EditorGUILayout.Toggle("Fill Empty with Inactive", _fillEmptyWithInactive);
            _showInactiveSquares = EditorGUILayout.Toggle("Show Inactive Squares", _showInactiveSquares);
            _inactiveSquareColor = EditorGUILayout.ColorField("Inactive Color", _inactiveSquareColor);
            
            EditorGUILayout.HelpBox(
                "Inactive squares maintain grid structure but don't participate in gameplay.",
                MessageType.Info);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawColorPalette()
        {
            _showColorPalette = EditorGUILayout.Foldout(_showColorPalette, "Color Palette", true);
            if (!_showColorPalette) return;
            
            EditorGUILayout.BeginVertical("box");
            
            int buttonsPerRow = 4;
            for (int i = 0; i < _colorPalette.Length; i += buttonsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < buttonsPerRow && (i + j) < _colorPalette.Length; j++)
                {
                    int index = i + j;
                    GUI.backgroundColor = _colorPalette[index];
                    if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        _currentColor = _colorPalette[index];
                    }
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current:", GUILayout.Width(55));
            _currentColor = EditorGUILayout.ColorField(GUIContent.none, _currentColor, false, false, false, GUILayout.Width(60), GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawGrids()
        {
            if (!_currentLevel) return;
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Grid Canvas", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Left Click: Place | Right Click: Erase | Drag to paint", MessageType.Info);
            
            // Draw grids based on edit mode
            if (_editMode == EditMode.Both)
            {
                EditorGUILayout.BeginHorizontal();
                DrawSingleGrid(false, "Initial Grid");
                GUILayout.Space(10);
                DrawSingleGrid(true, "Target Grid");
                EditorGUILayout.EndHorizontal();
            }
            else if (_editMode == EditMode.Initial)
            {
                DrawSingleGrid(false, "Initial Grid (Starting State)");
            }
            else // Target
            {
                DrawSingleGrid(true, "Target Grid (Goal State)");
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawSingleGrid(bool isTarget, string label)
        {
            EditorGUILayout.BeginVertical();
            
            // Label with background
            var labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(label, labelStyle);
            EditorGUILayout.Space(3);
            
            Event e = Event.current;
            
            for (int row = 0; row < _gridRows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < _gridColumns; col++)
                {
                    Rect cellRect = GUILayoutUtility.GetRect(_cellSize, _cellSize);
                    
                    var squareData = _currentLevel.GetSquare(row, col, isTarget);
                    Color cellColor;
                    
                    if (squareData != null)
                    {
                        if (!squareData.inactive)
                        {
                            cellColor = squareData.color;
                        }
                        else if (_showInactiveSquares)
                        {
                            cellColor = _inactiveSquareColor;
                        }
                        else
                        {
                            cellColor = new Color(0.3f, 0.3f, 0.3f);
                        }
                    }
                    else
                    {
                        cellColor = new Color(0.25f, 0.25f, 0.25f);
                    }
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw border
                    Handles.color = new Color(0.15f, 0.15f, 0.15f);
                    Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin), new Vector3(cellRect.xMax, cellRect.yMin));
                    Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin), new Vector3(cellRect.xMin, cellRect.yMax));
                    Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMin), new Vector3(cellRect.xMax, cellRect.yMax));
                    Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMax), new Vector3(cellRect.xMax, cellRect.yMax));
                    
                    // Handle mouse input
                    if (!cellRect.Contains(e.mousePosition)) continue;
                    if (e.type != EventType.MouseDown)
                    {
                        if (e.type != EventType.MouseDrag || !_isDragging) continue;
                        if (_isErasing)
                            RemoveSquare(row, col, isTarget);
                        else
                            PlaceSquare(row, col, isTarget);
                        e.Use();
                    }
                    else
                    {
                        if (e.button == 0) // Left click
                        {
                            _isDragging = true;
                            _isErasing = false;
                            PlaceSquare(row, col, isTarget);
                            e.Use();
                        }
                        else if (e.button == 1) // Right click
                        {
                            _isDragging = true;
                            _isErasing = true;
                            RemoveSquare(row, col, isTarget);
                            e.Use();
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Initial", GUILayout.Height(30)))
            {
                if (_currentLevel && EditorUtility.DisplayDialog("Clear Initial Grid", "Clear all squares from initial grid?", "Yes", "No"))
                {
                    _currentLevel.Clear(true, false);
                    EditorUtility.SetDirty(_currentLevel);
                }
            }
            
            if (GUILayout.Button("Clear Target", GUILayout.Height(30)))
            {
                if (_currentLevel && EditorUtility.DisplayDialog("Clear Target Grid", "Clear all squares from target grid?", "Yes", "No"))
                {
                    _currentLevel.Clear(false);
                    EditorUtility.SetDirty(_currentLevel);
                }
            }
            
            if (GUILayout.Button("Clear Both", GUILayout.Height(30)))
            {
                if (_currentLevel && EditorUtility.DisplayDialog("Clear Both Grids", "Clear all squares from both grids?", "Yes", "No"))
                {
                    _currentLevel.Clear();
                    EditorUtility.SetDirty(_currentLevel);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            GUI.enabled = _currentLevel != null && _targetGrid != null;
            if (GUILayout.Button("Apply Initial Grid to Scene", GUILayout.Height(35)))
            {
                ApplyToScene(false);
            }
            GUI.enabled = true;
            
            
        
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateNewLevel()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Level", "NewLevel", "asset", "Create a new level data file");
            if (!string.IsNullOrEmpty(path))
            {
                _currentLevel = CreateInstance<LevelData>();
                _currentLevel.rows = _gridRows;
                _currentLevel.columns = _gridColumns;
                AssetDatabase.CreateAsset(_currentLevel, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = _currentLevel;
            }
        }
        
        private void LoadLevel()
        {
            if (_currentLevel != null)
            {
                _gridRows = _currentLevel.rows;
                _gridColumns = _currentLevel.columns;
            }
        }
        
        private void PlaceSquare(int row, int col, bool isTarget)
        {
            if (_currentLevel)
            {
                _currentLevel.AddSquare(row, col, _currentColor, false, isTarget);
                EditorUtility.SetDirty(_currentLevel);
                Repaint();
            }
        }
        
        private void RemoveSquare(int row, int col, bool isTarget)
        {
            if (_currentLevel)
            {
                _currentLevel.RemoveSquare(row, col, isTarget);
                EditorUtility.SetDirty(_currentLevel);
                Repaint();
            }
        }
        
        private void ApplyToScene(bool useTarget)
        {
            if (!_squarePrefab)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Square Prefab first!", "OK");
                return;
            }
            
            if (_fillEmptyWithInactive)
            {
                _currentLevel.FillWithInactiveSquares(_inactiveSquareColor, !useTarget, useTarget);
                EditorUtility.SetDirty(_currentLevel);
            }
            
            // Clear existing squares
            while (_targetGrid.transform.childCount > 0)
            {
                DestroyImmediate(_targetGrid.transform.GetChild(0).gameObject);
            }
            
            // Create squares from level data
            var allSquares = _currentLevel.GetAllSquares(useTarget);
            allSquares.Sort((a, b) =>
            {
                int rowCompare = a.id.row.CompareTo(b.id.row);
                return rowCompare != 0 ? rowCompare : a.id.column.CompareTo(b.id.column);
            });
            
            int index = 0;
            foreach (var squareData in allSquares)
            {
                GameObject square = (GameObject)PrefabUtility.InstantiatePrefab(_squarePrefab, _targetGrid.transform);
                square.name = $"Square{index:D2}";
                
                var sq = square.GetComponent<Square>();
         
                sq.GetComponent<SpriteRenderer>().color = squareData.color;
                
                sq.Inactive = squareData.inactive;
                
                sq.ID = new GridIndex(squareData.id.row, squareData.id.column);
                
             
                index++;
            }
            
            // Update grid config
            var gridConfigField = typeof(SpriteGrid).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gridConfigField != null)
            {
                var config = (GridConfig)gridConfigField.GetValue(_targetGrid);
                config.columnsPerRow = _gridColumns;
                
            }
            
            int activeCount = _currentLevel.GetActiveSquares(useTarget).Count;
            string gridType = useTarget ? "Target" : "Initial";
            EditorUtility.DisplayDialog("Success", 
                $"Applied {gridType} Grid: {index} total squares ({activeCount} active, {index - activeCount} inactive)!", 
                "OK");
            EditorUtility.SetDirty(_targetGrid.gameObject);
        }
     
        public void DrawSaveButton()
        {
            EditorGUILayout.Space(5);
            if (_currentLevel == null || !GUILayout.Button("Save Level Data", GUILayout.Height(30))) return;
            EditorUtility.SetDirty(_currentLevel);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Saved", "Level data saved successfully!", "OK");
            _currentLevel.rows = _gridRows;
            _currentLevel.columns = _gridColumns;
        }
    }
}
#endif