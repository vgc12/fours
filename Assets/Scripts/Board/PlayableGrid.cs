using System;
using System.Threading.Tasks;
using Attributes;
using Board.Commands;
using Cysharp.Threading.Tasks;
using EventBus;
using Levels;
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
        [SerializeField, Required] private DotManager dotManager;
        [Inject]
        private IInputManager _inputManager;
        [Inject]
        private readonly ILevelManager _levelManager;
        private CommandManager _commandManager;
        private int _rotationsMade = 0;
        private string _gridBeforeMoveSnapshot = string.Empty;

        [SerializeField] private bool enableUndo = true;
        [SerializeField] private int maxUndoHistory = 50;

        public Dot SelectedDot { get; set; }
        public Dot PreviouslySelectedDot { get; set; }
        public bool IsRotating { get; private set; }
        public bool CanUndo => _commandManager?.CanUndo ?? false;
        public bool CanRedo => _commandManager?.CanRedo ?? false;
        public int RotationsMade => _rotationsMade;

        private EventBinding<LevelLoadedEvent> _levelLoadedEvent;
        
        protected override void Start()
        {
            base.Start();
            InitializeCommandSystem();
            InitializeInput();
            _gridBeforeMoveSnapshot = CurrentGridStateSnapshot;
            
            _levelLoadedEvent = new EventBinding<LevelLoadedEvent>(OnLevelLoaded);
            EventBus<LevelLoadedEvent>.Register(_levelLoadedEvent);
    

        }

        private void OnLevelLoaded(LevelLoadedEvent obj)
        {
            _rotationsMade = 0;
        }

        public override void Initialize()
        {
            base.Initialize();
            dotManager.CreateDots(SquareGroups);
        }
        
        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _inputHandler = new GridInputHandler(Camera.main, LayerMask.GetMask("Dot"));
       
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
                await TryRotate(RotationDirection.CounterClockwise);
            };
            
            _inputManager.RightClick += async () =>
            {
                await ExecuteSelect();
                await TryRotate(RotationDirection.Clockwise);
            };
            
            _inputManager.SwipeLeft += async () => await TryRotate(RotationDirection.CounterClockwise);
            
            _inputManager.SwipeRight += async () => await TryRotate(RotationDirection.Clockwise);
            
        }


        private async UniTask TryRotate(RotationDirection direction)
        {
            if (SelectedDot == null || SelectedDot != PreviouslySelectedDot || IsRotating)
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

            if (enableUndo && _commandManager != null && clickedDot != null && !IsRotating)
            {
                Logger.Log("Clicked on dot");
                
                var selectCommand = new SelectDotCommand(this, clickedDot);
                await _commandManager.ExecuteCommand(selectCommand);
            }
        }

        private async Task ExecuteRotation(RotationDirection direction)
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
    
            EventBus<GroupRotatedEvent>.Raise(new GroupRotatedEvent( SelectedDot, CurrentGridStateSnapshot));
        }

        private void CompleteRotation()
        {
            FindGroups();
            dotManager.ResetDots(SquareGroups);
            Logger.Log(GetAllGroupsDebugString());
            _rotationsMade++;
            IsRotating = false;
        }
        

        [ContextMenu("Undo Last Action")]
        public async Task UndoLastAction()
        {
            if (_commandManager != null)
            {
                await _commandManager.UndoLastCommand();
                FindGroups();
                dotManager.ResetDots(SquareGroups);
            }
        }

        [ContextMenu("Redo Last Action")]
        public async Task RedoLastAction()
        {
            if (_commandManager != null)
            {
                await _commandManager.RedoLastCommand();
                FindGroups();
                dotManager.ResetDots(SquareGroups);
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
            dotManager?.ClearDots();
        }

        private void OnDestroy()
        {
            EventBus<LevelLoadedEvent>.Deregister(_levelLoadedEvent);
            
            if (_commandManager == null)
            {
                return;
            }

            _commandManager.OnCommandExecuted -= OnCommandExecuted;
            _commandManager.OnCommandUndone -= OnCommandUndone;
            _commandManager.OnCommandRedone -= OnCommandRedone;




        }
    }
}