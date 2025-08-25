using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Board;
using StateMachines;
using UnityEditor.Experimental.GraphView;
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
    }

    public override void Update()
    {
        base.Update();
        grid.ArrangeGrid();
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
      
   
    }

    public override void Update()
    {
      
        _grid.ArrangeGrid();
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
       // await _grid.selectedDot.squareGroup.RotateAsync(SpriteGrid.RotationDirection.Clockwise);
        //_grid.RotateGroup(_grid.selectedDot.squareGroup.TopLeftIndex.Row , _grid.selectedDot.squareGroup.TopLeftIndex.Column,_grid.selectedDot.transform.position, SpriteGrid.RotationDirection.Clockwise);
        _grid.FindGroups();
        _grid.UpdateGridWithGroup(_grid.selectedDot.squareGroup, _grid.selectedDot.squareGroup.TopLeftIndex.Row, _grid.selectedDot.squareGroup.TopLeftIndex.Column);
        _grid.rotating = false;
    }
    
    
    
}


namespace Board
{
    public class SpriteGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Vector2 spacing = new Vector2(1f, 1f);
        
        [Min(1)]
        [SerializeField] private int columnsPerRow = 5;

    
        [Header("Alignment")]
        [SerializeField] private bool centerGrid = true;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;
    
        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;

        private Square[] _squares;
       
        private Square[,] _grid;
        
        private List<SquareGroup> _squareGroups;

        private List<Dot> _dots;
        
        private int _totalRows;

        public bool rotating;
        
        private StateMachine _stateMachine;
        
        public Dot selectedDot;
        
   

        public PlayerInputActions PlayerInputActions;
        
        
        void Start()
        {
            PlayerInputActions = new PlayerInputActions();
            PlayerInputActions.UI.Enable();
            PlayerInputActions.UI.Click.performed += OnClick;
            
            var idleState = new IdleState(this);
            var selectedState = new SelectedState(this);
            var rotatingState = new RotatingState(this);
            
            _stateMachine = new StateMachine();
            _stateMachine.AddTransition(idleState, selectedState, new FuncPredicate(() => selectedDot));
            
            _stateMachine.AddTransition(selectedState, idleState, new FuncPredicate(() => !selectedDot));
            _stateMachine.AddTransition(selectedState, rotatingState, new FuncPredicate(() => rotating));
            
            _stateMachine.AddTransition(rotatingState, selectedState, new FuncPredicate(() => !rotating ));
            
            _squareGroups = new List<SquareGroup>();
            
            
            _squares = GetComponentsInChildren<Square>();
            
            ArrangeGrid();
            FindGroups();
            InitializeDots();
          
            _stateMachine.SetState(idleState);
            
        }

        private void InitializeDots()
        {
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
            if (!obj.performed) return;
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var hit = Physics2D.OverlapCircle(worldPosition, .1f, LayerMask.GetMask("Dot"));

            if (selectedDot != null)
            {
                rotating = true;
             
            }
            
            selectedDot = hit != null ? hit.GetComponent<Dot>() : null;
        }

  
        void Update()
        {
            _stateMachine.Update();
        
        }
        
        void OnValidate()
        {   
            PrintGrid();
            if (Application.isPlaying) return;
            ArrangeGrid();
                
         
        }
    
        [ContextMenu("Arrange Grid")]
        public void ArrangeGrid()
        {
      
        
            CreateGridArray();
        
            if (_squares.Length == 0)
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
            
            sg.TopLeft.transform.localScale.Set(scale,scale,scale);
            sg.TopRight.transform.localScale.Set(scale,scale,scale);
            sg.BottomLeft.transform.localScale.Set(scale,scale,scale);
            sg.BottomRight.transform.localScale.Set(scale,scale,scale);
            
        }
        

        private void CreateGridArray()
        {
            _totalRows = Mathf.CeilToInt((float)_squares.Length / columnsPerRow);
            _grid = new Square[_totalRows, columnsPerRow];
        
            for (var i = 0; i < _squares.Length; i++)
            {
                var row = i / columnsPerRow;
                var column = i % columnsPerRow;
                _grid[row, column] = _squares[i];
            }
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
            Square topLeft = _grid[row, column];
            Square topRight = _grid[row, column + 1];
            Square bottomRight = _grid[row + 1, column + 1];
            Square bottomLeft = _grid[row + 1, column];
            
       
            StartCoroutine(RotateSpritesInGrid(centerPosition, new [] { topLeft.transform, topRight.transform, bottomRight.transform, bottomLeft.transform },  200));


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


        
        
        
        public void UpdateGridWithGroup(SquareGroup group, int topLeftRow, int topLeftColumn)
        {
            if (_grid == null) return;
        
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
        
        
        public enum RotationDirection { Clockwise = -1, CounterClockwise = 1 }

        private IEnumerator RotateSpritesInGrid(Vector3 rotationPoint, Transform[] transforms, float speed, RotationDirection direction = RotationDirection.Clockwise)
        {
            var totalDegrees = 90f * (int)direction;
    
            float rotatedDegrees = 0f;
    
            rotating = true;
            while (Mathf.Abs(rotatedDegrees) < Mathf.Abs(totalDegrees))
            {
                float rotationThisFrame = speed * Time.deltaTime * (int)direction;

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
            Vector2 startPosition = CalculateStartPosition();
        
            if(rotating) return;

            for (var row = 0; row < _grid.GetLength(0); row++)
            {
                for (var column = 0; column < _grid.GetLength(1); column++)
                {
                    Square square = _grid[row, column];
                    if (square == null) continue;
                    
                    Vector3 targetPosition = new Vector3(
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
        
            string gridString = "Grid Layout:\n";
            for (int row = 0; row < _totalRows; row++)
            {
                for (int col = 0; col < columnsPerRow; col++)
                {
                    Square square = _grid[row, col];
                    gridString += (square != null ? square.name : "null") + "\t";
                }
                gridString += "\n";
            }
            Debug.Log(gridString);
        }
        private Vector2 CalculateStartPosition()
        {
            Vector2 startPos = transform.position;
        
            if (centerGrid && _squares.Length > 0)
            {
                int totalRows = Mathf.CeilToInt((float)_squares.Length / columnsPerRow);
      
            
                float gridWidth = (columnsPerRow - 1) * spacing.x;
                float gridHeight = (totalRows - 1) * spacing.y;
            
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
       
            if (_squares.Length == 0) return Vector2.zero;
        
            int totalRows = Mathf.CeilToInt((float)_squares.Length / columnsPerRow);
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

            for (int i = 0; i < _squareGroups.Count; i++)
            {
                var group = _squareGroups[i];
                Debug.Log($"Group {i}: TopLeft({group.TopLeft.name}), TopRight({group.TopRight.name}), BottomLeft({group.BottomLeft.name}), BottomRight({group.BottomRight.name}) at Index({group.TopLeftIndex.Row}, {group.TopLeftIndex.Column})");
            }
        }
        
        public void DebugGrid()
        {
        
            Debug.Log($"Total Squares: {_grid.Length}, Total Rows: {_totalRows}, Columns Per Row: {columnsPerRow}");

            for (int row = 0; row < _totalRows; row++)
            {
                for (int column = 0; column < columnsPerRow; column++)
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
        
           
            if (_squares.Length == 0) return;
        
            // Draw grid preview
            Gizmos.color = Color.yellow;
            Vector2 startPos = CalculateStartPosition();
        
            int totalRows = Mathf.CeilToInt((float)_squares.Length / columnsPerRow);
        
            // Draw grid points
            for (int i = 0; i < _squares.Length; i++)
            {
                int row = i / columnsPerRow;
                int column = i % columnsPerRow;
            
                Vector3 gridPoint = new Vector3(
                    startPos.x + (column * spacing.x),
                    startPos.y - (row * spacing.y),
                    transform.position.z
                );
            
                Gizmos.DrawWireCube(gridPoint, Vector3.one * 0.2f);
            }
        
    
            Gizmos.color = Color.green;
            Vector2 gridSize = GetGridSize();
            Vector3 center = new Vector3(startPos.x + gridSize.x * 0.5f, startPos.y - gridSize.y * 0.5f, transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(gridSize.x + spacing.x, gridSize.y + spacing.y, 0));
        }
    }
}