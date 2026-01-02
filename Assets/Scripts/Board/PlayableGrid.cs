using System.Threading.Tasks;
using Board.Commands;
using EventBus;
using Player.Input;
using Reflex.Attributes;
using UI.States;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Board
{
    
    
    
    public sealed class PlayableGrid : SpriteGrid
    {
        private GridInputHandler _inputHandler;
        private DotManager _dotManager;
        [Inject]
        private IInputManager _inputManager;
        private CommandManager _commandManager;
        private int _movesRemaining = 10;
        private string _gridBeforeMoveSnapshot = string.Empty;

        [SerializeField] private bool enableUndo = true;
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
            _gridBeforeMoveSnapshot = GetGridStateSnapshot();
         
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
            if (!enableUndo) return;
            _commandManager = new CommandManager(maxUndoHistory);

            _commandManager.OnCommandExecuted += OnCommandExecuted;
            _commandManager.OnCommandUndone += OnCommandUndone;
            _commandManager.OnCommandRedone += OnCommandRedone;
        }

        private void InitializeInput()
        {
            _inputManager.Tap += async () =>
            {
                await ExecuteSelect();
            };

            _inputManager.Tap += async () => await ExecuteSelect();
            
            _inputManager.LeftClick += async() =>
            {
                await ExecuteSelect();
                TryRotate(RotationDirection.CounterClockwise);
            };
            
            _inputManager.RightClick += () => TryRotate(RotationDirection.Clockwise);
            
            _inputManager.SwipeLeft += () => TryRotate(RotationDirection.CounterClockwise);
            
            _inputManager.SwipeRight += () => TryRotate(RotationDirection.Clockwise);
            
        }

     

        public async void TryRotate(RotationDirection direction)
        {
            if (SelectedDot == null || SelectedDot != PreviouslySelectedDot)
            {
                return;
            }

            Logger.Log("Clicked on dot but now rotating");
            await ExecuteRotation(direction);
        }

        private async Task ExecuteSelect()
        {
            var mousePosition = Pointer.current.position.value;
            var clickedDot = _inputHandler.GetDotAtScreenPosition(mousePosition);

            if (enableUndo && _commandManager != null && clickedDot != null)
            {
                Logger.Log("Clicked on dot");
                
                // Check if this is a NEW group selection (not clicking the same dot again)
                var isNewGroupSelection = SelectedDot == null || 
                                          (clickedDot != SelectedDot && clickedDot.SquareGroup != SelectedDot.SquareGroup);
                
                var selectCommand = new SelectDotCommand(this, clickedDot);
                await _commandManager.ExecuteCommand(selectCommand);
                var currentGridSnapshot = GetGridStateSnapshot();
                // If a new group was just selected, take a snapshot and decrement moves
                if (isNewGroupSelection && SelectedDot != null && PreviouslySelectedDot != null && _gridBeforeMoveSnapshot != currentGridSnapshot)
                {
                    _gridBeforeMoveSnapshot = GetGridStateSnapshot();
                    Logger.Log("Snapshot taken for new group selection");
                    DecrementMoves("Selected new group");
                }
            }
        }

        public async Task ExecuteRotation(RotationDirection direction)
        {
            if (SelectedDot?.SquareGroup == null || IsRotating) return;

            var rotateCommand = new RotateGroupCommand(SelectedDot.SquareGroup, GridData, direction);

            if (enableUndo && _commandManager != null)
            {
                IsRotating = true;
                var success = await _commandManager.ExecuteCommand(rotateCommand);
                if (success)
                {
                    CompleteRotation();
                }
            }
    
            EventBus<GroupRotatedEvent>.Raise(new GroupRotatedEvent( SelectedDot,GetGridStateSnapshot()));
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
     
            EventBus<PlayerMovedEvent>.Raise(new PlayerMovedEvent(GetGridStateSnapshot(), _movesRemaining));
            Logger.Log($"Move used: {reason}. Moves remaining: {_movesRemaining}");

            if (_movesRemaining != 0)
            {
                return;
            }

            Logger.Log("No moves remaining!");
            // You can add game over logic here
            EventBus<LevelLostEvent>.Raise(new LevelLostEvent());

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
            
            
         
        }
    }
}