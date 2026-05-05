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
        private enum EditMode { Initial, Target, Both, Rotate }

        private EditMode _editMode = EditMode.Initial;


        // Rotation tracking
        private List<Solution.SolutionData> _rotationHistory = new();
        private bool _showRotationTools = true;
        private int _scrambleMoves = 10;

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
            if (_editMode == EditMode.Rotate)
                DrawRotationTools();
            else
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
                "Both: Edit both grids simultaneously\n" +
                "Rotate: Click 2x2 groups to rotate (LClick=CW, RClick=CCW)",
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

            if (_editMode == EditMode.Rotate)
            {
                EditorGUILayout.HelpBox("Left Click: Rotate CW | Right Click: Rotate CCW", MessageType.Info);
                DrawRotatableGrid();
            }
            else
            {
                EditorGUILayout.HelpBox("Left Click: Place | Right Click: Erase | Drag to paint", MessageType.Info);

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
                else
                {
                    DrawSingleGrid(true, "Target Grid (Goal State)");
                }
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

        private void DrawRotationTools()
        {
            _showRotationTools = EditorGUILayout.Foldout(_showRotationTools, "Rotation Tools", true);
            if (!_showRotationTools) return;

            EditorGUILayout.BeginVertical("box");

            // Scramble section
            EditorGUILayout.LabelField("Auto-Scramble", EditorStyles.boldLabel);
            _scrambleMoves = EditorGUILayout.IntSlider("Scramble Moves", _scrambleMoves, 1, 50);

            if (GUILayout.Button("Scramble Initial from Target", GUILayout.Height(25)))
            {
                ScrambleInitialFromTarget();
            }

            EditorGUILayout.Space(5);

            // History section
            EditorGUILayout.LabelField($"Rotation History: {_rotationHistory.Count} moves", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Undo Last Rotation", GUILayout.Height(25)) && _rotationHistory.Count > 0)
            {
                UndoLastRotation();
            }

            if (GUILayout.Button("Clear History", GUILayout.Height(25)))
            {
                _rotationHistory.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save History as Solution", GUILayout.Height(25)) && _rotationHistory.Count > 0)
            {
                SaveRotationHistoryAsSolution();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawRotatableGrid()
        {
            EditorGUILayout.BeginVertical();

            var labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Initial Grid (Rotate Mode)", labelStyle);
            EditorGUILayout.Space(3);

            var e = Event.current;

            for (var row = 0; row < _gridRows; row++)
            {
                EditorGUILayout.BeginHorizontal();

                for (var col = 0; col < _gridColumns; col++)
                {
                    var cellRect = GUILayoutUtility.GetRect(_cellSize, _cellSize);

                    var squareData = _initialSquares.GetSquare(row, col);
                    Color cellColor;

                    if (squareData != null)
                        cellColor = squareData.inactive ? new Color(0.3f, 0.3f, 0.3f) : squareData.color;
                    else
                        cellColor = new Color(0.25f, 0.25f, 0.25f);

                    EditorGUI.DrawRect(cellRect, cellColor);

                    // Highlight valid 2x2 group top-left corners
                    if (IsValidGroupTopLeft(row, col, _initialSquares))
                    {
                        // Draw a small indicator at the center of the 2x2 group
                        var dotRect = new Rect(
                            cellRect.xMax - 4,
                            cellRect.yMax - 4,
                            8, 8);
                        EditorGUI.DrawRect(dotRect, new Color(1f, 1f, 1f, 0.6f));
                    }

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

                    // Handle rotation clicks
                    if (!cellRect.Contains(e.mousePosition)) continue;
                    if (e.type != EventType.MouseDown) continue;

                    // Find the nearest valid 2x2 group top-left for this cell
                    var groupTopLeft = FindNearestGroupTopLeft(row, col);
                    if (groupTopLeft.x < 0) continue;

                    if (e.button == 0) // Left click = clockwise
                    {
                        RotateGroup(_initialSquares, groupTopLeft.x, groupTopLeft.y, RotationDirection.Clockwise);
                        RecordRotation(groupTopLeft.x, groupTopLeft.y, RotationDirection.Clockwise);
                        e.Use();
                    }
                    else if (e.button == 1) // Right click = counter-clockwise
                    {
                        RotateGroup(_initialSquares, groupTopLeft.x, groupTopLeft.y, RotationDirection.CounterClockwise);
                        RecordRotation(groupTopLeft.x, groupTopLeft.y, RotationDirection.CounterClockwise);
                        e.Use();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private bool IsValidGroupTopLeft(int row, int col, SquareDataList list)
        {
            if (row + 1 >= _gridRows || col + 1 >= _gridColumns) return false;

            var tl = list.GetSquare(row, col);
            var tr = list.GetSquare(row, col + 1);
            var bl = list.GetSquare(row + 1, col);
            var br = list.GetSquare(row + 1, col + 1);

            return tl != null && !tl.inactive
                && tr != null && !tr.inactive
                && bl != null && !bl.inactive
                && br != null && !br.inactive;
        }

        private Vector2Int FindNearestGroupTopLeft(int row, int col)
        {
            // Check all possible 2x2 groups this cell could be part of
            // Priority: the group where this cell is top-left, then top-right, bottom-left, bottom-right
            if (IsValidGroupTopLeft(row, col, _initialSquares)) return new Vector2Int(row, col);
            if (col > 0 && IsValidGroupTopLeft(row, col - 1, _initialSquares)) return new Vector2Int(row, col - 1);
            if (row > 0 && IsValidGroupTopLeft(row - 1, col, _initialSquares)) return new Vector2Int(row - 1, col);
            if (row > 0 && col > 0 && IsValidGroupTopLeft(row - 1, col - 1, _initialSquares)) return new Vector2Int(row - 1, col - 1);

            return new Vector2Int(-1, -1); // No valid group
        }

        private void RotateGroup(SquareDataList list, int topLeftRow, int topLeftCol, RotationDirection direction)
        {
            var tl = list.GetSquare(topLeftRow, topLeftCol);
            var tr = list.GetSquare(topLeftRow, topLeftCol + 1);
            var bl = list.GetSquare(topLeftRow + 1, topLeftCol);
            var br = list.GetSquare(topLeftRow + 1, topLeftCol + 1);

            if (tl == null || tr == null || bl == null || br == null) return;

            if (direction == RotationDirection.Clockwise)
            {
                // CW: TL←BL, TR←TL, BR←TR, BL←BR
                (tl.color, tr.color, br.color, bl.color) = (bl.color, tl.color, tr.color, br.color);
            }
            else
            {
                // CCW: TL←TR, TR←BR, BR←BL, BL←TL
                (tl.color, tr.color, br.color, bl.color) = (tr.color, br.color, bl.color, tl.color);
            }

            Repaint();
        }

        private void RecordRotation(int topLeftRow, int topLeftCol, RotationDirection direction)
        {
            var group = GetGroupEnumFromPosition(topLeftRow, topLeftCol);
            _rotationHistory.Add(new Solution.SolutionData
            {
                group = group,
                rotationDirection = direction,
                times = 1
            });
        }

        private Solution.Group GetGroupEnumFromPosition(int row, int col)
        {
            // Map grid position to Solution.Group based on relative position
            var maxRow = _gridRows - 2; // max valid top-left row
            var maxCol = _gridColumns - 2; // max valid top-left col

            var isTop = row == 0;
            var isBottom = row == maxRow;
            var isLeft = col == 0;
            var isRight = col == maxCol;
            var isMiddleRow = row > 0 && row < maxRow;
            var isMiddleCol = col > 0 && col < maxCol;

            if (isTop && isLeft) return Solution.Group.TopLeft;
            if (isTop && isRight) return Solution.Group.TopRight;
            if (isBottom && isLeft) return Solution.Group.BottomLeft;
            if (isBottom && isRight) return Solution.Group.BottomRight;
            if (isTop && isMiddleCol) return Solution.Group.TopMiddle;
            if (isBottom && isMiddleCol) return Solution.Group.BottomMiddle;
            if (isMiddleRow && isLeft) return Solution.Group.LeftMiddle;
            if (isMiddleRow && isRight) return Solution.Group.RightMiddle;
            return Solution.Group.Center;
        }

        private void ScrambleInitialFromTarget()
        {
            _initialSquares.CopyFrom(_targetSquares);
            _rotationHistory.Clear();

            // Collect all valid group positions
            var validGroups = new List<Vector2Int>();
            for (var row = 0; row < _gridRows - 1; row++)
            {
                for (var col = 0; col < _gridColumns - 1; col++)
                {
                    if (IsValidGroupTopLeft(row, col, _initialSquares))
                        validGroups.Add(new Vector2Int(row, col));
                }
            }

            if (validGroups.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No valid 2x2 groups found to scramble.", "OK");
                return;
            }

            // Apply random rotations
            for (var i = 0; i < _scrambleMoves; i++)
            {
                var groupIdx = Random.Range(0, validGroups.Count);
                var pos = validGroups[groupIdx];
                var direction = Random.Range(0, 2) == 0 ? RotationDirection.Clockwise : RotationDirection.CounterClockwise;

                RotateGroup(_initialSquares, pos.x, pos.y, direction);
                RecordRotation(pos.x, pos.y, direction);
            }

            Repaint();
        }

        private void UndoLastRotation()
        {
            if (_rotationHistory.Count == 0) return;

            var last = _rotationHistory[_rotationHistory.Count - 1];
            _rotationHistory.RemoveAt(_rotationHistory.Count - 1);

            // Find the position from the group enum and reverse the rotation
            var pos = GetPositionFromGroupEnum(last.group);
            if (pos.x < 0) return;

            var reverseDir = last.rotationDirection == RotationDirection.Clockwise
                ? RotationDirection.CounterClockwise
                : RotationDirection.Clockwise;

            RotateGroup(_initialSquares, pos.x, pos.y, reverseDir);
        }

        private Vector2Int GetPositionFromGroupEnum(Solution.Group group)
        {
            var maxRow = _gridRows - 2;
            var maxCol = _gridColumns - 2;
            var midRow = maxRow / 2;
            var midCol = maxCol / 2;

            return group switch
            {
                Solution.Group.TopLeft => new Vector2Int(0, 0),
                Solution.Group.TopRight => new Vector2Int(0, maxCol),
                Solution.Group.BottomLeft => new Vector2Int(maxRow, 0),
                Solution.Group.BottomRight => new Vector2Int(maxRow, maxCol),
                Solution.Group.TopMiddle => new Vector2Int(0, midCol),
                Solution.Group.BottomMiddle => new Vector2Int(maxRow, midCol),
                Solution.Group.LeftMiddle => new Vector2Int(midRow, 0),
                Solution.Group.RightMiddle => new Vector2Int(midRow, maxCol),
                Solution.Group.Center => new Vector2Int(midRow, midCol),
                _ => new Vector2Int(-1, -1)
            };
        }

        private void SaveRotationHistoryAsSolution()
        {
            // The solution is the inverse of the scramble (reverse order, opposite directions)
            var solutionSteps = new List<Solution.SolutionData>();
            for (var i = _rotationHistory.Count - 1; i >= 0; i--)
            {
                var step = _rotationHistory[i];
                solutionSteps.Add(new Solution.SolutionData
                {
                    group = step.group,
                    rotationDirection = step.rotationDirection == RotationDirection.Clockwise
                        ? RotationDirection.CounterClockwise
                        : RotationDirection.Clockwise,
                    times = step.times
                });
            }

            _currentLevel.solutionSteps = new Solution { steps = solutionSteps };
            EditorUtility.SetDirty(_currentLevel);
            EditorUtility.DisplayDialog("Saved",
                $"Solution saved with {solutionSteps.Count} steps.", "OK");
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