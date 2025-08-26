using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Board;
using EventChannel;
using StateMachines;
using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : BaseState
{
    private readonly SpriteGrid grid;

    public IdleState(SpriteGrid grid)
    {
        this.grid = grid;
    }

    public override void Enter()
    {
        grid.ArrangeGrid();
        grid.PrintGrid();
    }
}

public class SelectedState : BaseState
{
    private readonly SpriteGrid _grid;

    public SelectedState(SpriteGrid grid)
    {
        _grid = grid;
    }

    public override void Enter()
    {
       
        _grid.PrintGrid();
    }

    public override void Update()
    {
    }
}

public readonly struct GridIndex
{
    public readonly int Row;
    public readonly int Column;

    public GridIndex(int row, int column)
    {
        Row = row;
        Column = column;
    }
}


public class RotatingState : BaseState
{
    private readonly SpriteGrid _grid;

    public RotatingState(SpriteGrid grid)
    {
        _grid = grid;
    }

    public override async void Enter()
    {
        await _grid.selectedDot.squareGroup.RotateClockwise();

        _grid.UpdateGridWithGroup(_grid.selectedDot.squareGroup);

        _grid.ArrangeGrid();
        _grid.FindGroups();

        _grid.rotating = false;
        
      

       _grid.PrintAllGroups();
    }
}


namespace Board
{
    public class SpriteGrid : MonoBehaviour
    {
        [Header("Grid Settings")] [SerializeField]
        private Vector2 spacing = new Vector2(1f, 1f);

        [Min(1)] [SerializeField] private int columnsPerRow = 5;


        [Header("Alignment")] [SerializeField] private bool centerGrid = true;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;

        [Header("Debug")] [SerializeField] private bool showGizmos = true;

        private List<Square> _squares;

        private Square[,] _grid;

        private List<SquareGroup> _squareGroups;

        private List<Dot> _dots;

        private int _totalRows;

        public bool rotating;

        private StateMachine _stateMachine;

        public Dot selectedDot;


        private PlayerInputActions _playerInputActions;


        private EventBinding<SquareGroupRotatedEvent> _rotatedEvent;

        private void Start()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.UI.Enable();
            _playerInputActions.UI.Click.canceled += OnClick;

            //_rotatedEvent = new EventBinding<SquareGroupRotatedEvent>(() => rotating = false)

            var idleState = new IdleState(this);
            var selectedState = new SelectedState(this);
            var rotatingState = new RotatingState(this);

            _stateMachine = new StateMachine();
            _stateMachine.AddTransition(idleState, selectedState, new FuncPredicate(() => selectedDot));

            _stateMachine.AddTransition(selectedState, idleState, new FuncPredicate(() => !selectedDot));
            _stateMachine.AddTransition(selectedState, rotatingState, new FuncPredicate(() => rotating));

            _stateMachine.AddTransition(rotatingState, selectedState, new FuncPredicate(() => !rotating));

            _squareGroups = new List<SquareGroup>();


          

            ArrangeGrid();
            FindGroups();
            InitializeDots();


            _stateMachine.SetState(idleState);
        }
        
        public void GetChildSquares()
        {
            _squares = GetComponentsInChildren<Square>().OrderBy(s => s.name).ToList();
            
        }
        
        public void InitializeDots()
        {
            if (_dots != null)
            {
                foreach (var dot in _dots)
                {
                    
                    Destroy(dot.gameObject);
                    
                }
            }

            _dots = new List<Dot>(20);
            foreach (var squareGroup in _squareGroups)
            {
                var dot = DotFactory.Instance.CreateDot(squareGroup);
                _dots.Add(dot);
            }
        }


        public void FindGroups()
        {
            _squareGroups = GridGroupFinder.SplitGridIntoGroups(_grid);
        }

        private void OnClick(InputAction.CallbackContext obj)
        {
        
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var hit = Physics2D.OverlapCircle(worldPosition, .1f, LayerMask.GetMask("Dot"));

            if (selectedDot)
            {
                rotating = true;
            }

            selectedDot = hit != null ? hit.GetComponent<Dot>() : null;
        }


        private void Update()
        {
            _stateMachine.Update();
        }

        private void OnValidate()
        {
            PrintGrid();
            if (Application.isPlaying) return;
            ArrangeGrid();
        }

        [ContextMenu("Arrange Grid")]
        public void ArrangeGrid()
        {
            GetChildSquares();
            CreateGridArray();

            if (_squares.Count == 0)
            {
                Debug.LogWarning("No child sprites found to arrange in grid.");
                return;
            }

            ArrangeSpritesInGrid();
        }

        public void ScaleSelectedGroup(float scale, float offset = .2f)
        {
            if (selectedDot == null) return;
            var sg = selectedDot.squareGroup;

            sg.TopLeft.transform.localScale.Set(scale, scale, scale);
            sg.TopRight.transform.localScale.Set(scale, scale, scale);
            sg.BottomLeft.transform.localScale.Set(scale, scale, scale);
            sg.BottomRight.transform.localScale.Set(scale, scale, scale);
        }


        public void CreateGridArray()
        {
            _totalRows = Mathf.CeilToInt((float)_squares.Count / columnsPerRow);
            _grid = new Square[_totalRows, columnsPerRow];

            for (var i = 0; i < _squares.Count; i++)
            {
                var row = i / columnsPerRow;
                var column = i % columnsPerRow;
                _grid[row, column] = _squares[i];
                _squares[i].id = new(row, column);
            }
            GetChildSquares();
        }


        public void RotateGroup(int row, int column, Vector3 centerPosition, RotationDirection direction)
        {
            if (_grid == null) return;

            if (row < 0 || row >= _totalRows - 1 ||
                column < 0 || column >= columnsPerRow - 1)
            {
                Debug.LogWarning("Invalid top-left corner for rotation.");
                return;
            }

            // Store references to the squares in the 2x2 block
            var topLeft = _grid[row, column];
            var topRight = _grid[row, column + 1];
            var bottomRight = _grid[row + 1, column + 1];
            var bottomLeft = _grid[row + 1, column];


            StartCoroutine(RotateSpritesInGrid(centerPosition,
                new[] { topLeft.transform, topRight.transform, bottomRight.transform, bottomLeft.transform }, 200));


            if (direction == RotationDirection.Clockwise)
            {
                _grid[row, column] = topRight;
                _grid[row, column + 1] = bottomRight;
                _grid[row + 1, column + 1] = bottomLeft;
                _grid[row + 1, column] = topLeft;
            }
            else
            {
                _grid[row, column] = bottomLeft;
                _grid[row, column + 1] = topLeft;
                _grid[row + 1, column + 1] = topRight;
                _grid[row + 1, column] = bottomRight;
            }

            ArrangeSpritesInGrid();
        }


        public void UpdateGridWithGroup(SquareGroup group)
        {
            if (_grid == null) return;

            var topLeftRow = group.TopLeftIndex.Row;
            var topLeftColumn = group.TopLeftIndex.Column;

            if (topLeftRow < 0 || topLeftRow >= _totalRows - 1 ||
                topLeftColumn < 0 || topLeftColumn >= columnsPerRow - 1)
            {
                Debug.LogWarning("Invalid top-left corner for rotation.");
                return;
            }

            _grid[topLeftRow, topLeftColumn] = group.TopLeft;
            _grid[topLeftRow, topLeftColumn + 1] = group.TopRight;
            _grid[topLeftRow + 1, topLeftColumn + 1] = group.BottomRight;
            _grid[topLeftRow + 1, topLeftColumn] = group.BottomLeft;
        }


        public enum RotationDirection
        {
            Clockwise = -1,
            CounterClockwise = 1
        }

        private IEnumerator RotateSpritesInGrid(Vector3 rotationPoint, Transform[] transforms, float speed,
            RotationDirection direction = RotationDirection.Clockwise)
        {
            var totalDegrees = 90f * (int)direction;

            var rotatedDegrees = 0f;

            rotating = true;
            while (Mathf.Abs(rotatedDegrees) < Mathf.Abs(totalDegrees))
            {
                var rotationThisFrame = speed * Time.deltaTime * (int)direction;

                // Make sure we don't overshoot
                if (Mathf.Abs(rotatedDegrees + rotationThisFrame) > Mathf.Abs(totalDegrees))
                {
                    rotationThisFrame = totalDegrees - rotatedDegrees;
                }

                foreach (var t in transforms)
                {
                    t.RotateAround(rotationPoint, Vector3.forward, rotationThisFrame);
                }

                rotatedDegrees += rotationThisFrame;

                yield return null;
            }

            rotating = false;
        }

        public Square GetSquareAt(int row, int column)
        {
            if (_grid == null || row < 0 || row >= _totalRows || column < 0 || column >= columnsPerRow)
            {
                return null;
            }

            return _grid[row, column];
        }

        private void ArrangeSpritesInGrid()
        {
            var startPosition = CalculateStartPosition();

            if (rotating) return;

            for (var row = 0; row < _grid.GetLength(0); row++)
            {
                for (var column = 0; column < _grid.GetLength(1); column++)
                {
                    var square = _grid[row, column];
                    if (square == null) continue;

                    var targetPosition = new Vector3(
                        startPosition.x + (column * spacing.x),
                        startPosition.y - (row * spacing.y),
                        square.transform.position.z
                    );

                    square.transform.position = targetPosition;
                }
            }
        }

        public void PrintGrid()
        {
            if (_grid == null) return;

            var gridString = "Grid Layout:\n";
            for (var row = 0; row < _totalRows; row++)
            {
                for (var col = 0; col < columnsPerRow; col++)
                {
                    var square = _grid[row, col];
                    gridString += (square != null ? square.name : "null") + "\t";
                }

                gridString += "\n";
            }

            Debug.Log(gridString);
        }

        private Vector2 CalculateStartPosition()
        {
            Vector2 startPos = transform.position;

            if (centerGrid && _squares.Count > 0)
            {
                var totalRows = Mathf.CeilToInt((float)_squares.Count / columnsPerRow);


                var gridWidth = (columnsPerRow - 1) * spacing.x;
                var gridHeight = (totalRows - 1) * spacing.y;

                // Center the grid
                startPos.x -= gridWidth * 0.5f;
                startPos.y += gridHeight * 0.5f;
            }

            // Apply offset
            startPos += gridOffset;

            return startPos;
        }

        // Utility methods for runtime control
        public void SetSpacing(Vector2 newSpacing)
        {
            spacing = newSpacing;
            ArrangeGrid();
        }

        public void SetColumnsPerRow(int columns)
        {
            columnsPerRow = Mathf.Max(1, columns);
            ArrangeGrid();
        }

        public void SetGridOffset(Vector2 offset)
        {
            gridOffset = offset;
            ArrangeGrid();
        }

        public void ToggleCenterGrid(bool center)
        {
            centerGrid = center;
            ArrangeGrid();
        }


        public Vector2 GetGridSize()
        {
            if (_squares.Count == 0) return Vector2.zero;

            var totalRows = Mathf.CeilToInt((float)_squares.Count / columnsPerRow);
            return new Vector2(
                (columnsPerRow - 1) * spacing.x,
                (totalRows - 1) * spacing.y
            );
        }


        public void PrintAllGroups()
        {
            if (_squareGroups == null || _squareGroups.Count == 0)
            {
                Debug.Log("No groups found.");
                return;
            }

            for (var i = 0; i < _squareGroups.Count; i++)
            {
                var group = _squareGroups[i];
                Debug.Log(
                    $"Group {i}: TopLeft({group.TopLeft.name}), TopRight({group.TopRight.name}), BottomLeft({group.BottomLeft.name}), BottomRight({group.BottomRight.name}) at Index({group.TopLeftIndex.Row}, {group.TopLeftIndex.Column})");
            }
        }

        public void DebugGrid()
        {
            Debug.Log($"Total Squares: {_grid.Length}, Total Rows: {_totalRows}, Columns Per Row: {columnsPerRow}");

            for (var row = 0; row < _totalRows; row++)
            {
                for (var column = 0; column < columnsPerRow; column++)
                {
                    var square = GetSquareAt(row, column);
                    if (square != null)
                    {
                        Debug.Log($"Square at ({row}, {column}): {square.name}");
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;


            if (_squares.Count == 0) return;

            // Draw grid preview
            Gizmos.color = Color.yellow;
            var startPos = CalculateStartPosition();

            var totalRows = Mathf.CeilToInt((float)_squares.Count / columnsPerRow);

            // Draw grid points
            for (var i = 0; i < _squares.Count; i++)
            {
                var row = i / columnsPerRow;
                var column = i % columnsPerRow;

                var gridPoint = new Vector3(
                    startPos.x + (column * spacing.x),
                    startPos.y - (row * spacing.y),
                    transform.position.z
                );

                Gizmos.DrawWireCube(gridPoint, Vector3.one * 0.2f);
            }


            Gizmos.color = Color.green;
            var gridSize = GetGridSize();
            var center = new Vector3(startPos.x + gridSize.x * 0.5f, startPos.y - gridSize.y * 0.5f,
                transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(gridSize.x + spacing.x, gridSize.y + spacing.y, 0));
        }

    
    }
}