
#if UNITY_EDITOR
using System.Collections.Generic;
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
        private SpriteGrid _playableGrid;
        
        private bool _isDragging;
        private bool _isErasing;
        
        // Grid editing mode
        private enum EditMode { Initial, Target, Both }
        private EditMode _editMode = EditMode.Initial;
        
        // Inactive square settings
        private Color _inactiveSquareColor = new(0.2f, 0.2f, 0.2f, 0.5f);
        private bool _showInactiveSquares = false;
        private bool _fillEmptyWithInactive = false;
        
        // Foldout states
        private bool _showLevelSettings = true;
        private bool _showGridSettings = true;
        private bool _showEditMode = true;
        private bool _showInactiveSettings = true;
        private bool _showColorPalette = true;
        
        // Color palette
        private List<Color> _colorPalette = new List<Color>
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            new(1f, 0.5f, 0f), // Orange
            Color.magenta,
            Color.cyan,
            Color.white
        };
        private TargetGrid _targetGrid;
        private int _movesAllowed;

        [MenuItem("Tools/Board/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(500, 600);
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
            
            _movesAllowed = EditorGUILayout.IntField("Allowed Moves", _movesAllowed );
            
            if (_currentLevel != null)
            {
           
                
                var playableActive = _currentLevel.GetActiveSquares(false).Count;
                var playableTotal = _currentLevel.initialSquares.Count;
                var targetActive = _currentLevel.GetActiveSquares(true).Count;
                var targetTotal = _currentLevel.targetSquares.Count;
                
                EditorGUILayout.HelpBox(
                    $"Initial - Active: {playableActive} | Total: {playableTotal}\n" +
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
            
            _playableGrid = (SpriteGrid)EditorGUILayout.ObjectField("Playable Grid", _playableGrid, typeof(PlayableGrid), true);
            _targetGrid = (TargetGrid)EditorGUILayout.ObjectField("Target Grid", _targetGrid, typeof(TargetGrid), true);
            
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
    
            var buttonsPerRow = 4;
            for (var i = 0; i < _colorPalette.Count; i += buttonsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (var j = 0; j < buttonsPerRow && (i + j) < _colorPalette.Count; j++)
                {
                    var index = i + j;
            
                    EditorGUILayout.BeginVertical();
            
                    // Color button
                    GUI.backgroundColor = _colorPalette[index];
                    if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        _currentColor = _colorPalette[index];
                    }
                    GUI.backgroundColor = Color.white;
            
                    // Small remove button below color
                    if (GUILayout.Button("×", GUILayout.Width(40), GUILayout.Height(15)))
                    {
                        _colorPalette.RemoveAt(index);
                        return; // Exit to avoid index issues
                    }
            
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
    
            EditorGUILayout.Space(5);
    
            // Current color and add button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current:", GUILayout.Width(55));
            _currentColor = EditorGUILayout.ColorField(GUIContent.none, _currentColor, false, false, false, GUILayout.Width(60), GUILayout.Height(20));
    
            if (GUILayout.Button("Add to Palette", GUILayout.Height(20)))
            {
                _colorPalette.Add(_currentColor);
            }
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
            
            var e = Event.current;
            
            for (var row = 0; row < _gridRows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (var col = 0; col < _gridColumns; col++)
                {
                    var cellRect = GUILayoutUtility.GetRect(_cellSize, _cellSize);
                    
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
                        
                       // _currentLevel.AddSquare(row, col, Color.clear,  true, isTarget);
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
            
            GUI.enabled = _currentLevel != null && _playableGrid != null;
            if (GUILayout.Button("Apply Grids to Scene", GUILayout.Height(35)))
            {
                ApplyToScene();
            }
            GUI.enabled = true;
            
            
        
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateNewLevel()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create New Level", "NewLevel", "asset", "Create a new level data file");
            if (!string.IsNullOrEmpty(path))
            {
                _currentLevel = CreateInstance<LevelData>();
                _currentLevel.rows = _gridRows;
                _currentLevel.columns = _gridColumns;
                _currentLevel.movesAllowed = _movesAllowed;
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
                _movesAllowed = _currentLevel.movesAllowed;
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
        
        private void ApplyToScene()
        {
        
            if (_fillEmptyWithInactive)
            {
                _currentLevel.FillWithInactiveSquares(_inactiveSquareColor, true, true);
                EditorUtility.SetDirty(_currentLevel);
            }
            
            _playableGrid.ClearGrid();
            _targetGrid.ClearGrid();
            
            // Create squares from level data
            var playableSquares = _currentLevel.GetAllSquares(false);
            var targetSquares = _currentLevel.GetAllSquares(true);
       
            var targetIndex = SpawnSquaresUnderGrid(_targetGrid, targetSquares);
            var playableIndex = SpawnSquaresUnderGrid(_playableGrid, playableSquares);
            
            SaveToLevel();
       
            
            var activePlayableCount = _currentLevel.GetActiveSquares(false).Count;
            EditorUtility.DisplayDialog("Success", 
                $"Applied Playable Grid: {playableIndex} total squares ({activePlayableCount} active, {playableIndex - activePlayableCount} inactive)!", 
                "OK");
            var activeTargetCount = _currentLevel.GetActiveSquares(true).Count;
            EditorUtility.DisplayDialog("Success",
                $"Applied Target Grid: {activeTargetCount} total squares ({activeTargetCount} active, {targetIndex - activeTargetCount} inactive)!", 
                "OK");
            EditorUtility.SetDirty(_playableGrid.gameObject);
        }

        private int SpawnSquaresUnderGrid(SpriteGrid grid, List<LevelData.SquareData> squares)
        {
            var index = 0;
            squares.Sort((a, b) =>
            {
                var rowCompare = a.id.row.CompareTo(b.id.row);
                return rowCompare != 0 ? rowCompare : a.id.column.CompareTo(b.id.column);
            });
            foreach (var squareData in squares)
            {
                Square.Create(new GridIndex(squareData.id.row, squareData.id.column), squareData.color, squareData.inactive, grid.transform);
                index++;
            }
            return index;
        }
     
        public void DrawSaveButton()
        {
            EditorGUILayout.Space(5);
            if (_currentLevel == null || !GUILayout.Button("Save Level Data", GUILayout.Height(30))) return;
            SaveToLevel();
            EditorUtility.DisplayDialog("Saved", "Level data saved successfully!", "OK");
        }

        private void SaveToLevel()
        {
            EditorUtility.SetDirty(_currentLevel);
            AssetDatabase.SaveAssets();
            var gridConfigField = typeof(SpriteGrid).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gridConfigField != null)
            {
                var config = (GridConfig)gridConfigField.GetValue(_playableGrid);
                config.columnsPerRow = _gridColumns;
                
            }
            _currentLevel.rows = _gridRows;
            _currentLevel.columns = _gridColumns;
            _currentLevel.movesAllowed = _movesAllowed;
        }
    }
}
#endif