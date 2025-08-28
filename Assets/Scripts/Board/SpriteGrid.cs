using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using StateMachines;

namespace Board
{
    // Simplified data structure
    public readonly struct GridIndex
    {
        public readonly int Row;
        public readonly int Column;

        public GridIndex(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString() => $"({Row}, {Column})";
    }

    // Grid configuration
    [Serializable]
    public class GridConfig
    {
        [Header("Grid Settings")]
        public Vector2 spacing = new Vector2(1f, 1f);
        [Min(1)] public int columnsPerRow = 5;
        
        [Header("Alignment")]
        public bool centerGrid = true;
        public Vector2 gridOffset = Vector2.zero;
        
        [Header("Debug")]
        public bool showGizmos = true;
    }

    // Handles grid layout and positioning
    public class GridLayout
    {
        private readonly GridConfig _config;
        private readonly Transform _gridTransform;

        public GridLayout(GridConfig config, Transform gridTransform)
        {
            _config = config;
            _gridTransform = gridTransform;
        }

        public Vector2 CalculateStartPosition(int totalSquares)
        {
            Vector2 startPos = _gridTransform.position;

            if (_config.centerGrid && totalSquares > 0)
            {
                var totalRows = Mathf.CeilToInt((float)totalSquares / _config.columnsPerRow);
                var gridWidth = (_config.columnsPerRow - 1) * _config.spacing.x;
                var gridHeight = (totalRows - 1) * _config.spacing.y;

                startPos.x -= gridWidth * 0.5f;
                startPos.y += gridHeight * 0.5f;
            }

            return startPos + _config.gridOffset;
        }

        public Vector3 CalculateGridPosition(int row, int column, Vector2 startPosition, float zPosition = 0)
        {
            return new Vector3(
                startPosition.x + (column * _config.spacing.x),
                startPosition.y - (row * _config.spacing.y),
                zPosition
            );
        }

        public Vector2 GetGridSize(int totalSquares)
        {
            if (totalSquares == 0) return Vector2.zero;

            var totalRows = Mathf.CeilToInt((float)totalSquares / _config.columnsPerRow);
            return new Vector2(
                (_config.columnsPerRow - 1) * _config.spacing.x,
                (totalRows - 1) * _config.spacing.y
            );
        }
    }

    // Manages the 2D array of squares
    public class GridData
    {
        private Square[,] _grid;
        private int _totalRows;
        private int _columnsPerRow;

        public int TotalRows => _totalRows;
        public int ColumnsPerRow => _columnsPerRow;

        public void Initialize(List<Square> squares, int columnsPerRow)
        {
            _columnsPerRow = columnsPerRow;
            _totalRows = Mathf.CeilToInt((float)squares.Count / columnsPerRow);
            _grid = new Square[_totalRows, columnsPerRow];

            for (var i = 0; i < squares.Count; i++)
            {
                var row = i / columnsPerRow;
                var column = i % columnsPerRow;
                _grid[row, column] = squares[i];
                squares[i].id = new GridIndex(row, column);
            }
        }

        public Square GetSquare(int row, int column)
        {
            if (_grid == null || row < 0 || row >= _totalRows || column < 0 || column >= _columnsPerRow)
                return null;

            return _grid[row, column];
        }

        public void SetSquare(int row, int column, Square square)
        {
            if (_grid != null && row >= 0 && row < _totalRows && column >= 0 && column < _columnsPerRow)
            {
                _grid[row, column] = square;
            }
        }

        public void UpdateWithGroup(SquareGroup group)
        {
            var topLeftRow = group.TopLeftIndex.Row;
            var topLeftColumn = group.TopLeftIndex.Column;

            if (!ValidGroupAt(topLeftRow, topLeftColumn)) return;

            SetSquare(topLeftRow, topLeftColumn, group.TopLeft);
            SetSquare(topLeftRow, topLeftColumn + 1, group.TopRight);
            SetSquare(topLeftRow + 1, topLeftColumn + 1, group.BottomRight);
            SetSquare(topLeftRow + 1, topLeftColumn, group.BottomLeft);
        }

        public bool ValidGroupAt( int row, int col)
        {
            return _grid[row, col] &&
                   _grid[row, col + 1] &&
                   _grid[row + 1, col] &&
                   _grid[row + 1, col + 1];
        }


        public string GetDebugString()
        {
            if (_grid == null) return "Grid is null";

            var gridString = "Grid Layout:\n";
            for (var row = 0; row < _totalRows; row++)
            {
                for (var col = 0; col < _columnsPerRow; col++)
                {
                    var square = _grid[row, col];
                    gridString += (square != null ? square.name : "null") + "\t";
                }
                gridString += "\n";
            }
            return gridString;
        }
    }

    // Handles input detection and conversion
    public class GridInputHandler
    {
        private readonly Camera _camera;
        private readonly LayerMask _dotLayerMask;

        public GridInputHandler(Camera camera, LayerMask dotLayerMask)
        {
            _camera = camera;
            _dotLayerMask = dotLayerMask;
        }

        public Dot GetDotAtScreenPosition(Vector2 screenPosition)
        {
            Vector2 worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapCircle(worldPosition, 0.1f, _dotLayerMask);
            return hit?.GetComponent<Dot>();
        }
    }

    // Manages dot creation and lifecycle
    public class DotManager
    {
        private List<Dot> _dots = new List<Dot>();

        public IReadOnlyList<Dot> Dots => _dots;

        public void CreateDots(List<SquareGroup> squareGroups)
        {
            ClearDots();
            
            _dots = new List<Dot>(squareGroups.Count);
            foreach (var squareGroup in squareGroups)
            {
                var dot = DotFactory.Instance.CreateDot(squareGroup);
                _dots.Add(dot);
            }
        }

        public void ResetDots(List<SquareGroup> squareGroups)
        {
            for (var i = 0; i < _dots.Count && i < squareGroups.Count; i++)
            {
                _dots[i].squareGroup = squareGroups[i];
            }
        }

        private void ClearDots()
        {
            if (_dots == null) return;
            foreach (var dot in _dots)
            {
                if (dot != null && dot.gameObject != null)
                {
                    UnityEngine.Object.Destroy(dot.gameObject);
                }
            }
        }
    }

    // State machine states
    public class IdleState : BaseState
    {
        private readonly SpriteGrid _grid;

        public IdleState(SpriteGrid grid)
        {
            _grid = grid;
        }

        public override void Enter()
        {
            Debug.Log(_grid.GetGridDebugString());
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
            Debug.Log(_grid.GetGridDebugString());
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
            await _grid.ExecuteRotation();
        }
    }

    // Main grid controller - much cleaner now
    public class SpriteGrid : MonoBehaviour
    {
        [SerializeField] private GridConfig _config = new GridConfig();

        private List<Square> _squares;
        private GridData _gridData;
        private GridLayout _gridLayout;
        private GridInputHandler _inputHandler;
        private DotManager _dotManager;
        private StateMachine _stateMachine;
        private PlayerInputActions _playerInputActions;
        private List<SquareGroup> _squareGroups;

        public Dot SelectedDot { get; private set; }
        public bool IsRotating { get; private set; }

        private void Start()
        {
            InitializeComponents();
            InitializeInput();
            InitializeStateMachine();
            InitializeGrid();
            FindGroups();
            _dotManager.CreateDots(_squareGroups);
            
            _stateMachine.SetState(new IdleState(this));
        }

        private void InitializeComponents()
        {
            _gridData = new GridData();
            _gridLayout = new GridLayout(_config, transform);
            _inputHandler = new GridInputHandler(Camera.main, LayerMask.GetMask("Dot"));
            _dotManager = new DotManager();
            _squareGroups = new List<SquareGroup>();
        }

        private void InitializeInput()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.UI.Enable();
            _playerInputActions.UI.Click.canceled += OnClick;
        }

        private void InitializeStateMachine()
        {
            var idleState = new IdleState(this);
            var selectedState = new SelectedState(this);
            var rotatingState = new RotatingState(this);

            _stateMachine = new StateMachine();
            _stateMachine.AddTransition(idleState, selectedState, new FuncPredicate(() => SelectedDot != null));
            _stateMachine.AddTransition(selectedState, idleState, new FuncPredicate(() => SelectedDot == null));
            _stateMachine.AddTransition(selectedState, rotatingState, new FuncPredicate(() => IsRotating));
            _stateMachine.AddTransition(rotatingState, selectedState, new FuncPredicate(() => !IsRotating));
        }

        private void OnClick(InputAction.CallbackContext obj)
        {
            Vector2 mousePosition = Input.mousePosition;
            var clickedDot = _inputHandler.GetDotAtScreenPosition(mousePosition);

            if (SelectedDot != null)
            {
                IsRotating = true;
            }

            SelectedDot = clickedDot;
        }

        public async Task ExecuteRotation()
        {
            if (SelectedDot?.squareGroup == null) return;

            await SelectedDot.squareGroup.RotateClockwise();
            _gridData.UpdateWithGroup(SelectedDot.squareGroup);
            FindGroups();
            IsRotating = false;
            _dotManager.ResetDots(_squareGroups);
            
            Debug.Log(GetAllGroupsDebugString());
        }

        public void FindGroups()
        {
            _squareGroups = GridGroupFinder.SplitGridIntoGroups(_gridData);
        }

        [ContextMenu("Arrange Grid")]
        public void InitializeGrid()
        {
            GetChildSquares();
            _gridData.Initialize(_squares, _config.columnsPerRow);
            
            if (_squares.Count == 0)
            {
                Debug.LogWarning("No child sprites found to arrange in grid.");
                return;
            }

            ArrangeSpritesInGrid();
        }

        private void GetChildSquares()
        {
            _squares = GetComponentsInChildren<Square>().OrderBy(s => s.name).ToList();
        }

        private void ArrangeSpritesInGrid()
        {
            if (IsRotating) return;

            var startPosition = _gridLayout.CalculateStartPosition(_squares.Count);

            for (var row = 0; row < _gridData.TotalRows; row++)
            {
                for (var column = 0; column < _gridData.ColumnsPerRow; column++)
                {
                    var square = _gridData.GetSquare(row, column);
                    if (square == null) continue;

                    var targetPosition = _gridLayout.CalculateGridPosition(row, column, startPosition, square.transform.position.z);
                    square.transform.position = targetPosition;
                }
            }
        }

        private void Update()
        {
            _stateMachine?.Update();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            InitializeGrid();
        }

        // Public interface methods
        public string GetGridDebugString() => _gridData?.GetDebugString() ?? "Grid not initialized";

        public string GetAllGroupsDebugString()
        {
            if (_squareGroups == null || _squareGroups.Count == 0)
                return "No groups found.";

            var result = "";
            for (var i = 0; i < _squareGroups.Count; i++)
            {
                var group = _squareGroups[i];
                result += $"Group {i}: TopLeft({group.TopLeft.name}), TopRight({group.TopRight.name}), " +
                         $"BottomLeft({group.BottomLeft.name}), BottomRight({group.BottomRight.name}) " +
                         $"at Index{group.TopLeftIndex}\n";
            }
            return result;
        }

        public Vector2 GetGridSize() => _gridLayout.GetGridSize(_squares?.Count ?? 0);
        

        private void OnDrawGizmos()
        {
            if (!_config.showGizmos || _squares == null || _squares.Count == 0) return;

            Gizmos.color = Color.yellow;
            var startPos = _gridLayout.CalculateStartPosition(_squares.Count);

            // Draw grid points
            for (var i = 0; i < _squares.Count; i++)
            {
                var row = i / _config.columnsPerRow;
                var column = i % _config.columnsPerRow;
                var gridPoint = _gridLayout.CalculateGridPosition(row, column, startPos, transform.position.z);
                Gizmos.DrawWireCube(gridPoint, Vector3.one * 0.2f);
            }

            // Draw grid bounds
            Gizmos.color = Color.green;
            var gridSize = GetGridSize();
            var center = new Vector3(startPos.x + gridSize.x * 0.5f, startPos.y - gridSize.y * 0.5f, transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(gridSize.x + _config.spacing.x, gridSize.y + _config.spacing.y, 0));
        }

        private void OnDestroy()
        {
            _playerInputActions.UI.Click.canceled -= OnClick;
            _playerInputActions?.Dispose();
        }
    }
}
