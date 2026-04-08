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
        private SquareDataList _targetSquares;
        private SquareDataList _initialSquares;
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
        


        // Foldout states
        private bool _showLevelSettings = true;
        private bool _showGridSettings = true;
        private bool _showEditMode = true;
  
        private bool _showColorPalette = true;


        private TargetGrid _targetGrid;

        private int _movesForMaxStars = 5;
        private int _movesForMidStars = 7;
        private int _movesForMinStars = 8;
        private SquareFactory _squareFactory;

        private ColorPalette _colorPalette;

        [MenuItem("Tools/Board/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(500, 600);
        }


        private void OnGUI()
        {
            _colorPalette = Resources.Load<ColorPalette>("ColorPalette");
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            _initialSquares ??= new SquareDataList();
            _targetSquares ??= new SquareDataList();
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawLevelSettings();
            DrawGridSettings();
            DrawEditModeSettings();
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

            _currentLevel =
                (LevelData)EditorGUILayout.ObjectField("Current Level", _currentLevel, typeof(LevelData), false);

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

            _movesForMaxStars = EditorGUILayout.IntField("Moves for 3 Stars", _movesForMaxStars);
            _movesForMidStars = EditorGUILayout.IntField("Moves for 2 Stars", _movesForMidStars);
            _movesForMinStars = EditorGUILayout.IntField("Moves for 1 Star", _movesForMinStars);


            if (_currentLevel)
            {
                var playableActive = _currentLevel.GetActiveSquares(false).Count;
                var playableTotal = _initialSquares.Count;
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
            _playableGrid = FindAnyObjectByType<PlayableGrid>();
            _targetGrid = FindAnyObjectByType<TargetGrid>();
            if (_playableGrid == null || _targetGrid == null)
            {
                EditorGUILayout.HelpBox("Playable Grid and Target Grid references are required in the scene.",
                    MessageType.Warning);
            }

            /*       _playableGrid =
                       (SpriteGrid)EditorGUILayout.ObjectField("Playable Grid", _playableGrid, typeof(PlayableGrid), true);
                   _targetGrid = (TargetGrid)EditorGUILayout.ObjectField("Target Grid", _targetGrid, typeof(TargetGrid), true);
                   */
            _squareFactory =
                (SquareFactory)EditorGUILayout.ObjectField("Square Factory", _squareFactory, typeof(SquareFactory),
                    true);
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
            if (GUILayout.Button("Copy Initial -> Target", GUILayout.Height(25)))
            {
                _targetSquares.squares = new List<LevelData.SquareData>(_initialSquares.squares);
            }

            if (GUILayout.Button("Copy Target -> Initial", GUILayout.Height(25)))
            {
                _initialSquares.squares = new List<LevelData.SquareData>(_targetSquares.squares);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        

        private void DrawColorPalette()
        {
            _showColorPalette = EditorGUILayout.Foldout(_showColorPalette, "Color Palette", true);
            if (!_showColorPalette) return;

            EditorGUILayout.BeginVertical("box");

            const int buttonsPerRow = 4;
            for (var i = 0; i < _colorPalette.colors.Count; i += buttonsPerRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (var j = 0; j < buttonsPerRow && (i + j) < _colorPalette.colors.Count; j++)
                {
                    var index = i + j;

                    EditorGUILayout.BeginVertical();

                    // Color button
                    GUI.backgroundColor = _colorPalette.colors[index];
                    if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        _currentColor = _colorPalette.colors[index];
                    }

                    GUI.backgroundColor = Color.white;

                    // Small remove button below color
                    if (GUILayout.Button("×", GUILayout.Width(40), GUILayout.Height(15)))
                    {
                        _colorPalette.colors.RemoveAt(index);
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
            _currentColor = EditorGUILayout.ColorField(GUIContent.none, _currentColor, false, false, false,
                GUILayout.Width(60), GUILayout.Height(20));

            if (GUILayout.Button("Add to Palette", GUILayout.Height(20)))
            {
                _colorPalette.colors.Add(_currentColor);
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

                    var squareData =isTarget? _targetSquares.GetSquare(row, col) : _initialSquares.GetSquare(row, col);
                    Color cellColor;

                    if (squareData != null)
                    {
                        if (!squareData.inactive)
                        {
                            cellColor = squareData.color;
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
                    Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin),
                        new Vector3(cellRect.xMax, cellRect.yMin));
                    Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMin),
                        new Vector3(cellRect.xMin, cellRect.yMax));
                    Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMin),
                        new Vector3(cellRect.xMax, cellRect.yMax));
                    Handles.DrawLine(new Vector3(cellRect.xMin, cellRect.yMax),
                        new Vector3(cellRect.xMax, cellRect.yMax));

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
                if (EditorUtility.DisplayDialog("Clear Initial Grid",
                        "Clear all squares from initial grid?", "Yes", "No"))
                {
                    _initialSquares.Clear();
                }
            }

            if (GUILayout.Button("Clear Target", GUILayout.Height(30)))
            {
                if ( EditorUtility.DisplayDialog("Clear Target Grid",
                        "Clear all squares from target grid?", "Yes", "No"))
                {
                    _targetSquares.Clear();
                }
            }

            if (GUILayout.Button("Clear Both", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Both Grids",
                        "Clear all squares from both grids?", "Yes", "No"))
                {
                    _initialSquares.Clear();
                    _targetSquares.Clear();
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
            var path = EditorUtility.SaveFilePanelInProject("Create New Level", "NewLevel", "asset",
                "Create a new level data file");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _currentLevel = CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(_currentLevel, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _currentLevel;
        }

        private void LoadLevel()
        {
            if (_currentLevel == null)
            {
                return;
            }

            _gridRows = _currentLevel.rows;
            _gridColumns = _currentLevel.columns;
            _initialSquares.FillWithInactive(_gridRows, _gridColumns, Color.gray);
            _targetSquares.FillWithInactive(_gridRows, _gridColumns, Color.gray);
            _initialSquares = _currentLevel.initialSquares;
            _targetSquares = _currentLevel.targetSquares;
            

            _movesForMinStars = _currentLevel.movesForMinStars;
            _movesForMidStars = _currentLevel.movesForMidStars;
            _movesForMaxStars = _currentLevel.movesForMaxStars;
            Repaint();
        }

        private void PlaceSquare(int row, int col, bool isTarget)
        {
          
            if (isTarget)
            {
                _targetSquares.AddSquare(row, col, _currentColor,  false);
            }
            else
            {
                _initialSquares.AddSquare(row, col, _currentColor,  false);
            }

          
            Repaint();
        }

        private void RemoveSquare(int row, int col, bool isTarget)
        {
      

            if (isTarget)
            {
                _targetSquares.RemoveSquare(row, col);
                _targetSquares.AddSquare(row, col, _currentColor, true);
            }
            else
            {
                _initialSquares.RemoveSquare(row, col);
                _initialSquares.AddSquare(row, col, _currentColor, true);
            }
            
           
            Repaint();
        }

        private void ApplyToScene()
        {
          
            _playableGrid.ClearGrid();
            _targetGrid.ClearGrid();

            // Create squares from level data
            var playableSquares = _initialSquares.squares;
            var targetSquares = _targetSquares.squares;

            var targetIndex = SpawnSquaresUnderGrid(_targetGrid, targetSquares);
            var playableIndex = SpawnSquaresUnderGrid(_playableGrid, playableSquares);

            SaveToLevel();


            var activePlayableCount = _initialSquares.Count;
            EditorUtility.DisplayDialog("Success",
                $"Applied Playable Grid: {playableIndex} total squares ({activePlayableCount} active, {playableIndex - activePlayableCount} inactive)!",
                "OK");
            var activeTargetCount = _targetSquares.Count;
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
                _squareFactory.Create(new SquareCreationParams
                    {
                        Id = new GridIndex(squareData.id.row, squareData.id.column), Color = squareData.color,
                        Inactive = squareData.inactive,
                        Parent = grid.transform
                    }
                );
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


            _currentLevel.rows = _gridRows;
            _currentLevel.columns = _gridColumns;
            _currentLevel.initialSquares = _initialSquares;
            _currentLevel.targetSquares = _targetSquares;

            _currentLevel.movesForMaxStars = _movesForMaxStars;
            _currentLevel.movesForMidStars = _movesForMidStars;
            _currentLevel.movesForMinStars = _movesForMinStars;
        }
    }
}
#endif