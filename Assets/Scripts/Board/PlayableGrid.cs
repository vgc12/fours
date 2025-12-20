using System.Threading.Tasks;
using Board.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Board
{
    public sealed class PlayableGrid : SpriteGrid
    {
        private GridInputHandler _inputHandler;
        private DotManager _dotManager;
        private PlayerInputActions _playerInputActions;
        private CommandManager _commandManager;

        [SerializeField] private bool _enableUndo = true;
        [SerializeField] private int maxUndoHistory = 50;

        public Dot SelectedDot { get; set; }
        public Dot PreviouslySelectedDot { get; set; }
        public bool IsRotating { get; private set; }
        public bool CanUndo => _commandManager?.CanUndo ?? false;
        public bool CanRedo => _commandManager?.CanRedo ?? false;

        protected override void Start()
        {
            base.Start();
            InitializePlayableComponents();
            InitializeCommandSystem();
            InitializeInput();
            _dotManager.CreateDots(SquareGroups);
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _inputHandler = new GridInputHandler(Camera.main, LayerMask.GetMask("Dot"));
            _dotManager = new DotManager();
        }

        private void InitializePlayableComponents()
        {
         
        }

        private void InitializeCommandSystem()
        {
            if (!_enableUndo) return;
            _commandManager = new CommandManager(maxUndoHistory);

            _commandManager.OnCommandExecuted += OnCommandExecuted;
            _commandManager.OnCommandUndone += OnCommandUndone;
            _commandManager.OnCommandRedone += OnCommandRedone;
        }

        private void InitializeInput()
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.UI.Enable();
            _playerInputActions.UI.Click.canceled += OnClick;
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

            if (SelectedDot != null && SelectedDot == PreviouslySelectedDot)
            {
                await ExecuteRotation();
            }
            
         
        }

        public async Task ExecuteRotation()
        {
            if (SelectedDot?.squareGroup == null || IsRotating) return;

            var rotateCommand = new RotateGroupCommand(SelectedDot.squareGroup, GridData, RotationDirection.Clockwise);

            if (_enableUndo && _commandManager != null)
            {
                var success = await _commandManager.ExecuteCommand(rotateCommand);
                if (success)
                {
                    CompleteRotation();
                }
            }
            else
            {
                var success = await rotateCommand.Execute();
                if (success)
                {
                    CompleteRotation();
                }
            }
        }

        private void CompleteRotation()
        {
            FindGroups();
            IsRotating = false;
            _dotManager.ResetDots(SquareGroups);
            Debug.Log(GetAllGroupsDebugString());
        }

        public void SetSelectedDot(Dot dot)
        {
            SelectedDot = dot;
        }

        [ContextMenu("Undo Last Action")]
        public async Task UndoLastAction()
        {
            if (_commandManager != null)
            {
                await _commandManager.UndoLastCommand();
                FindGroups();
                _dotManager.ResetDots(SquareGroups);
            }
        }

        [ContextMenu("Redo Last Action")]
        public async Task RedoLastAction()
        {
            if (_commandManager != null)
            {
                await _commandManager.RedoLastCommand();
                FindGroups();
                _dotManager.ResetDots(SquareGroups);
            }
        }

        protected override void ArrangeSpritesInGrid()
        {
            if (IsRotating) return;
            base.ArrangeSpritesInGrid();
        }

        private void OnCommandExecuted(ICommand command)
        {
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
            _dotManager?.ClearDots();
        }

        private void OnDestroy()
        {
            if (_commandManager != null)
            {
                _commandManager.OnCommandExecuted -= OnCommandExecuted;
                _commandManager.OnCommandUndone -= OnCommandUndone;
                _commandManager.OnCommandRedone -= OnCommandRedone;
            }

            if (_playerInputActions != null)
            {
                _playerInputActions.UI.Click.canceled -= OnClick;
                _playerInputActions.UI.Disable();
                _playerInputActions.Disable();
                _playerInputActions?.Dispose();
            }
        }
    }
}