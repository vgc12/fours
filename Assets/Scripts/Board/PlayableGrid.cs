using System.Threading.Tasks;
using Board.Commands;
using EventBus;
using UI.States;
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
        private int _movesRemaining = 10;
        private string _gridBeforeMoveSnapshot = string.Empty;

        [SerializeField] private bool _enableUndo = true;
        [SerializeField] private int maxUndoHistory = 50;

        public Dot SelectedDot { get; set; }
        public Dot PreviouslySelectedDot { get; set; }
        public bool IsRotating { get; private set; }
        public bool CanUndo => _commandManager?.CanUndo ?? false;
        public bool CanRedo => _commandManager?.CanRedo ?? false;
        public int MovesRemaining => _movesRemaining;

        protected override void Start()
        {
            base.Start();
            InitializeCommandSystem();
            InitializeInput();
         
        }
         

        public override void Initialize()
        {
            base.Initialize();
            _dotManager.CreateDots(SquareGroups);
        }
        
        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _inputHandler = new GridInputHandler(Camera.main, LayerMask.GetMask("Dot"));
            _dotManager = new DotManager();
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
            
            Vector2 mousePosition = Pointer.current.position.value;
            var clickedDot = _inputHandler.GetDotAtScreenPosition(mousePosition);

            if (_enableUndo && _commandManager != null && clickedDot != null)
            {
                Logger.Log("Clicked on dot");
                
                // Check if this is a NEW group selection (not clicking the same dot again)
                bool isNewGroupSelection = SelectedDot == null || 
                    (clickedDot != SelectedDot && clickedDot.squareGroup != SelectedDot.squareGroup);
                
                var selectCommand = new SelectDotCommand(this, clickedDot);
                await _commandManager.ExecuteCommand(selectCommand);
                
                // If a new group was just selected, take a snapshot and decrement moves
                if (isNewGroupSelection && SelectedDot != null && PreviouslySelectedDot != null && _gridBeforeMoveSnapshot != GetGridStateSnapshot())
                {
                    _gridBeforeMoveSnapshot = GetGridStateSnapshot();
                    Logger.Log("Snapshot taken for new group selection");
                    DecrementMoves("Selected new group");
                }
            }
            
            // If clicking the same dot again, rotate
            if (SelectedDot != null && SelectedDot == PreviouslySelectedDot)
            {
                Logger.Log("Clicked on dot but now rotating");
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
            Logger.Log(GetAllGroupsDebugString());
        }

        private void DecrementMoves(string reason)
        {
            if (_movesRemaining <= 0) return;
            _movesRemaining--;
     
            EventBus.EventBus<PlayerMovedEvent>.Raise(new PlayerMovedEvent(GetGridStateSnapshot(), _movesRemaining));
            Logger.Log($"Move used: {reason}. Moves remaining: {_movesRemaining}");
                
            if (_movesRemaining == 0)
            {
                Logger.Log("No moves remaining!");
                // You can add game over logic here
                EventBus<LevelLostEvent>.Raise(new());
            }
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
            Logger.Log($"Command executed: {command.Description}");
        }

        private void OnCommandUndone(ICommand command)
        {
            Logger.Log($"Command undone: {command.Description}");
        }

        private void OnCommandRedone(ICommand command)
        {
            Logger.Log($"Command redone: {command.Description}");
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

    public struct LevelLostEvent : IEvent
    {
         
    }
}