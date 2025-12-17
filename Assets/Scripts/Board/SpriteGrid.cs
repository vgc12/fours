using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Board.Commands;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Board
{
    public sealed class SpriteGrid : MonoBehaviour
    {
        [SerializeField] private GridConfig config = new();

        private List<Square> _squares;
        private GridData _gridData;
        private GridLayout _gridLayout;
        private GridInputHandler _inputHandler;
        private DotManager _dotManager;
        private PlayerInputActions _playerInputActions;
        private List<SquareGroup> _squareGroups;

        public Dot SelectedDot { get; set; }
        public bool IsRotating { get; private set; }

        private bool _enableUndo = true;

        private CommandManager _commandManager;

        [SerializeField] private int maxUndoHistory = 50;

        private void InitializeCommandSystem()
        {
            if (!_enableUndo) return;
            _commandManager = new CommandManager(maxUndoHistory);


            _commandManager.OnCommandExecuted += OnCommandExecuted;
            _commandManager.OnCommandUndone += OnCommandUndone;
            _commandManager.OnCommandRedone += OnCommandRedone;
        }


        public async Task ExecuteRotation()
        {
            if (SelectedDot?.squareGroup == null) return;

            var rotateCommand = new RotateGroupCommand(SelectedDot.squareGroup, _gridData, RotationDirection.Clockwise);

            if (_enableUndo && _commandManager != null)
            {
                var success = await _commandManager.ExecuteCommand(rotateCommand);
                if (success)
                {
                    FindGroups();
                    IsRotating = false;
                    _dotManager.ResetDots(_squareGroups);
                    Debug.Log(GetAllGroupsDebugString());
                }
            }
            else
            {
                // Fallback to direct execution if undo is disabled
                var success = await rotateCommand.Execute();
                if (success)
                {
                    FindGroups();
                    IsRotating = false;
                    _dotManager.ResetDots(_squareGroups);
                    Debug.Log(GetAllGroupsDebugString());
                }
            }
        }


        public void SetSelectedDot(Dot dot)
        {
            SelectedDot = dot;
        }


        private async void OnClick(InputAction.CallbackContext obj)
        {
            Vector2 mousePosition = Input.mousePosition;
            var clickedDot = _inputHandler.GetDotAtScreenPosition(mousePosition);


            var selectCommand = new SelectDotCommand(this, clickedDot);

            if (_enableUndo && _commandManager != null)
            {
                await _commandManager.ExecuteCommand(selectCommand);
            }
            else
            {
                await selectCommand.Execute();
            }

            if (SelectedDot != null)
            {
                await ExecuteRotation();
            }
        }

        // Public methods for undo/redo functionality
        [ContextMenu("Undo Last Action")]
        public async Task UndoLastAction()
        {
            if (_commandManager != null)
            {
                await _commandManager.UndoLastCommand();
                FindGroups();
                _dotManager.ResetDots(_squareGroups);
            }
        }

        [ContextMenu("Redo Last Action")]
        public async Task RedoLastAction()
        {
            if (_commandManager != null)
            {
                await _commandManager.RedoLastCommand();
                FindGroups();
                _dotManager.ResetDots(_squareGroups);
            }
        }

        public bool CanUndo => _commandManager?.CanUndo ?? false;
        public bool CanRedo => _commandManager?.CanRedo ?? false;


        private void OnCommandExecuted(ICommand command)
        {
            // Update UI, play sounds, etc.
            Debug.Log($"Command executed: {command.Description}");
        }

        private void OnCommandUndone(ICommand command)
        {
            Debug.Log($"Command undone: {command.Description}");
        }

        private void OnCommandRedone(ICommand command)
        {
            Debug.Log($"Command redone: {command.Description}");
        }


        private void OnDisable()
        {
            _dotManager.ClearDots();
        }


        private void OnDestroy()
        {
            if (_commandManager != null)
            {
                _commandManager.OnCommandExecuted -= OnCommandExecuted;
                _commandManager.OnCommandUndone -= OnCommandUndone;
                _commandManager.OnCommandRedone -= OnCommandRedone;
            }


            _playerInputActions.UI.Click.canceled -= OnClick;
            _playerInputActions.UI.Disable();
            _playerInputActions.Disable();
            _playerInputActions?.Dispose();
        }

        private void Start()
        {
            InitializeGridSystem();
        }

        private void InitializeGridSystem()
        {
            InitializeComponents();
            InitializeCommandSystem();
            InitializeInput();
            InitializeGrid();
            FindGroups();
            _dotManager.CreateDots(_squareGroups);
        }

        private void InitializeComponents()
        {
            _gridData = new GridData();
            _gridLayout = new GridLayout(config, transform);
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


        public void FindGroups()
        {
            _squareGroups = GridGroupFinder.SplitGridIntoGroups(_gridData);
        }

        [ContextMenu("Arrange Grid")]
        public void InitializeGrid()
        {
            GetChildSquares();
            if (_gridData == null) _gridData = new GridData();
            _gridData.Initialize(_squares, config.columnsPerRow);

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

                    var targetPosition =
                        _gridLayout.CalculateGridPosition(row, column, startPosition, square.transform.position.z);
                    square.transform.position = targetPosition;
                }
            }
        }


        private void OnValidate()
        {
            if (Application.isPlaying) return;

            InitializeComponents();
            InitializeGrid();
        }


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
            if (!config.showGizmos || _squares == null || _squares.Count == 0) return;

            Gizmos.color = Color.yellow;
            var startPos = _gridLayout.CalculateStartPosition(_squares.Count);

            // Draw grid points
            for (var i = 0; i < _squares.Count; i++)
            {
                var row = i / config.columnsPerRow;
                var column = i % config.columnsPerRow;
                var gridPoint = _gridLayout.CalculateGridPosition(row, column, startPos, transform.position.z);
                Gizmos.DrawWireCube(gridPoint, Vector3.one * 0.2f);
            }

            // Draw grid bounds
            Gizmos.color = Color.green;
            var gridSize = GetGridSize();
            var center = new Vector3(startPos.x + gridSize.x * 0.5f, startPos.y - gridSize.y * 0.5f,
                transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(gridSize.x + config.spacing.x, gridSize.y + config.spacing.y, 0));
        }
    }
}