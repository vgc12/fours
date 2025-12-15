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
        
        // Inactive square settings
        private Color _inactiveSquareColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private bool _showInactiveSquares = true;
        private bool _fillEmptyWithInactive = true;
        
        // Color palette
        private readonly Color[] _colorPalette = {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            new Color(1f, 0.5f, 0f), // Orange
            Color.magenta,
            Color.cyan,
            Color.white
        };
        
        [MenuItem("Tools/Board/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
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
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            DrawToolbar();
            EditorGUILayout.Space(10);
            DrawInactiveSquareSettings();
            EditorGUILayout.Space(10);
            DrawColorPalette();
            EditorGUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawGrid();
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            DrawActions();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            _currentLevel = (LevelData)EditorGUILayout.ObjectField("Current Level", _currentLevel, typeof(LevelData), false);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New Level"))
            {
                CreateNewLevel();
            }
            if (GUILayout.Button("Load Level") && _currentLevel != null)
            {
                LoadLevel();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            _gridRows = EditorGUILayout.IntSlider("Rows", _gridRows, 2, 10);
            _gridColumns = EditorGUILayout.IntSlider("Columns", _gridColumns, 2, 10);
            _cellSize = EditorGUILayout.Slider("Cell Size", _cellSize, 30f, 100f);
            
            EditorGUILayout.Space(5);
            
            _squarePrefab = (GameObject)EditorGUILayout.ObjectField("Square Prefab", _squarePrefab, typeof(GameObject), false);
            _targetGrid = (SpriteGrid)EditorGUILayout.ObjectField("Target Grid", _targetGrid, typeof(SpriteGrid), true);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawInactiveSquareSettings()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Inactive Square Settings", EditorStyles.boldLabel);
            
            _fillEmptyWithInactive = EditorGUILayout.Toggle("Fill Empty with Inactive", _fillEmptyWithInactive);
            _showInactiveSquares = EditorGUILayout.Toggle("Show Inactive Squares", _showInactiveSquares);
            _inactiveSquareColor = EditorGUILayout.ColorField("Inactive Square Color", _inactiveSquareColor);
            
            EditorGUILayout.HelpBox(
                "Inactive squares maintain grid structure but don't participate in gameplay. " +
                "Enable 'Fill Empty with Inactive' to automatically create them when applying to scene.",
                MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawColorPalette()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Color Palette", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < _colorPalette.Length; i++)
            {
                GUI.backgroundColor = _colorPalette[i];
                if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                {
                    _currentColor = _colorPalette[i];
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Color:");
            GUI.backgroundColor = _currentColor;
            EditorGUILayout.ColorField(GUIContent.none, _currentColor, false, false, false, GUILayout.Width(60), GUILayout.Height(20));
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGrid()
        {
            if (_currentLevel == null) return;
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Grid Canvas", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Left Click: Place active square | Right Click: Erase | Empty cells will be filled with inactive squares", MessageType.Info);
            
            Event e = Event.current;
            
            for (int row = 0; row < _gridRows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < _gridColumns; col++)
                {
                    Rect cellRect = GUILayoutUtility.GetRect(_cellSize, _cellSize);
                    
                    var squareData = _currentLevel.GetSquare(row, col);
                    Color cellColor;
                    
                    if (squareData != null)
                    {
                        if (squareData.isActive)
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
                        // Empty cell - will become inactive when applied
                        cellColor = new Color(0.25f, 0.25f, 0.25f);
                    }
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw X pattern for inactive or empty squares when shown
                    if (squareData != null && !squareData.isActive && _showInactiveSquares)
                    {
                        Handles.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                        Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin), new Vector3(cellRect.xMax, cellRect.yMax));
                        Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMin), new Vector3(cellRect.xMin, cellRect.yMax));
                    }
                    else if (squareData == null && _fillEmptyWithInactive)
                    {
                        // Show preview of where inactive squares will be placed
                        Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                        Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin), new Vector3(cellRect.xMax, cellRect.yMax));
                        Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMin), new Vector3(cellRect.xMin, cellRect.yMax));
                    }
                    
                    GUI.Box(cellRect, "");
                    
                    // Handle mouse input
                    if (!cellRect.Contains(e.mousePosition)) continue;
                    if (e.type != EventType.MouseDown)
                    {
                        if (e.type != EventType.MouseDrag || !_isDragging) continue;
                        if (_isErasing)
                            RemoveSquare(row, col);
                        else
                            PlaceSquare(row, col);
                        e.Use();
                    }
                    else
                    {
                        if (e.button == 0) // Left click
                        {
                            _isDragging = true;
                            _isErasing = false;
                            PlaceSquare(row, col);
                            e.Use();
                        }
                        else if (e.button == 1) // Right click
                        {
                            _isDragging = true;
                            _isErasing = true;
                            RemoveSquare(row, col);
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
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Grid"))
            {
                if (_currentLevel != null && EditorUtility.DisplayDialog("Clear Grid", "Clear all squares from the grid?", "Yes", "No"))
                {
                    _currentLevel.Clear();
                    EditorUtility.SetDirty(_currentLevel);
                }
            }
            
            if (GUILayout.Button("Apply to Scene") && _currentLevel != null && _targetGrid != null)
            {
                ApplyToScene();
            }
            EditorGUILayout.EndHorizontal();
            
            if (_currentLevel != null)
            {
                int activeCount = _currentLevel.GetActiveSquares().Count;
                int totalCount = _currentLevel.squares.Count;
                EditorGUILayout.HelpBox($"Active squares: {activeCount} | Total squares: {totalCount}", MessageType.Info);
            }
            
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
        
        private void PlaceSquare(int row, int col)
        {
            if (_currentLevel != null)
            {
                _currentLevel.AddSquare(row, col, _currentColor, true); // Always create active squares
                EditorUtility.SetDirty(_currentLevel);
                Repaint();
            }
        }
        
        private void RemoveSquare(int row, int col)
        {
            if (_currentLevel != null)
            {
                _currentLevel.RemoveSquare(row, col);
                EditorUtility.SetDirty(_currentLevel);
                Repaint();
            }
        }
        
        private void ApplyToScene()
        {
            if (_squarePrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Square Prefab first!", "OK");
                return;
            }
            
            // Fill empty positions with inactive squares if enabled
            if (_fillEmptyWithInactive)
            {
                _currentLevel.FillWithInactiveSquares(_inactiveSquareColor);
                EditorUtility.SetDirty(_currentLevel);
            }
            
            // Clear existing squares
            while (_targetGrid.transform.childCount > 0)
            {
                DestroyImmediate(_targetGrid.transform.GetChild(0).gameObject);
            }
            
            // Create squares from level data (including inactive ones)
            // Sort by row then column to maintain order
            var allSquares = _currentLevel.GetAllSquares();
            allSquares.Sort((a, b) =>
            {
                int rowCompare = a.row.CompareTo(b.row);
                return rowCompare != 0 ? rowCompare : a.column.CompareTo(b.column);
            });
            
            int index = 0;
            foreach (var squareData in allSquares)
            {
                GameObject square = (GameObject)PrefabUtility.InstantiatePrefab(_squarePrefab, _targetGrid.transform);
                square.name = $"Square{index:D2}";
                
                var spriteRenderer = square.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = squareData.color;
                    
                    // Make inactive squares semi-transparent
                    if (!squareData.isActive)
                    {
                        Color color = spriteRenderer.color;
                        color.a = 0.3f;
                        spriteRenderer.color = color;
                    }
                }
                
                // Optionally tag or add component to mark inactive squares
                if (!squareData.isActive)
                {
                    square.tag = "Inactive"; // You can use this in your game logic
                    
                    // Or add a custom component to track inactive state
                    var squareComponent = square.GetComponent<Square>();
                    if (squareComponent != null)
                    {
                        // You could add an "isActive" field to your Square class
                        // squareComponent.isActive = false;
                    }
                }
                
                index++;
            }
            
            // Update grid config to match level
            var gridConfigField = typeof(SpriteGrid).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gridConfigField != null)
            {
                var config = (GridConfig)gridConfigField.GetValue(_targetGrid);
                config.columnsPerRow = _gridColumns;
            }
            
            int activeCount = _currentLevel.GetActiveSquares().Count;
            EditorUtility.DisplayDialog("Success", 
                $"Created {index} total squares ({activeCount} active, {index - activeCount} inactive) in the scene!", 
                "OK");
            EditorUtility.SetDirty(_targetGrid.gameObject);
        }
    }
}
#endif