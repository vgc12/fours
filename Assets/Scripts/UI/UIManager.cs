using System;
using System.Collections.Generic;
using Attributes;
using Board;
using Cysharp.Threading.Tasks;
using DependencyInjection;
using EventBus;
using Levels;
using PrimeTween;
using Reflex.Attributes;
using Singletons;
using UI.States;
using UnityEngine;
using ILogger = Logging.ILogger;

namespace UI
{
    public sealed class UIManager : PersistentSingleton<UIManager>
    {
        public enum SlideDirection { Up, Down }

        // Configuration
        [Header("UI Screens")] [Required, SerializeField]
        private GameObject mainMenu;

        [Required, SerializeField] private GameObject inGameUI;
        [Required, SerializeField] private GameObject optionsMenu;
        [Required, SerializeField] private GameObject levelSelectMenu;
        [Required, SerializeField] private GameObject levelCompleteMenu;
        [Required, SerializeField] private GameObject levelFailedMenu;
        [Required, SerializeField] private GameObject pausedMenu;
        [Required, SerializeField] private GameObject loadingMenu;

        [Header("References")] [Required, SerializeField]
        private GameObject gridsParent;

        [Required, SerializeField] private Canvas canvas;

        [Header("Animation Settings")] [SerializeField]
        private float transitionDuration = 0.5f;

        [SerializeField] private Ease transitionEase = Ease.InOutCubic;

        [Inject] private readonly ILogger _logger;

        // State management
        private readonly StateMachine.StateMachine _stateMachine = new();
        private UIStateCollection _states;
        private UIBaseState _currentUIState;
        private bool _isGroupRotating;

        // Dependencies
        private ILevelManager _levelManager;
        private EventBinding<LevelCompletedEvent> _levelCompletedBinding;
        private EventBinding<GroupRotateEvent> _groupRotateBinding;

        protected override void Awake()
        {
            base.Awake();
            InitializeDependencies();
            InitializeStates();
            RegisterStates();
            SetInitialState();
            SubscribeToEvents();
        }

        private void OnDestroy() { UnsubscribeFromEvents(); }

        #region Initialization

        private void InitializeDependencies()
        {
            PrimeTweenConfig.SetTweensCapacity(800);
            RuntimeResolver.Instance.TryResolve(out _levelManager);
        }

        private void InitializeStates()
        {
            _states = new UIStateCollection
            {
                MainMenu = new MainMenuState(mainMenu, this),
                InGame = new InGameUIState(gridsParent, inGameUI, this),
                LevelSelect = new LevelSelectState(levelSelectMenu, this),
                LevelComplete = new LevelCompleteState(levelCompleteMenu, this),
                LevelFailed = new LevelFailedState(levelFailedMenu, this),
                Loading = new LoadingState(loadingMenu, this),
                Paused = new PausedState(pausedMenu, this)
                // Options = new OptionsState(optionsMenu, this)
            };
        }

        private void RegisterStates()
        {
            foreach (var state in _states.AllStates)
            {
                if (state != null)
                    _stateMachine.AddState(state);
            }
        }

        private void SetInitialState()
        {
            _currentUIState = _states.MainMenu;
            _stateMachine.SetStateAndEnter(_currentUIState);
        }

        private void SubscribeToEvents()
        {
            _levelCompletedBinding = new EventBinding<LevelCompletedEvent>(OnLevelCompleted);
            EventBus<LevelCompletedEvent>.Register(_levelCompletedBinding);

            _groupRotateBinding = new EventBinding<GroupRotateEvent>(OnGroupRotated);
            EventBus<GroupRotateEvent>.Register(_groupRotateBinding);
        }


        private void UnsubscribeFromEvents() { EventBus<LevelCompletedEvent>.Deregister(_levelCompletedBinding); }

        #endregion

        #region State Machine Updates

        private void Update() { _stateMachine.Update(); }

        private void FixedUpdate() => _stateMachine.FixedUpdate();

        #endregion

        #region Transition Logic

        public void TransitionToState(UIBaseState newState, params Tween?[] extraTweens)
        {
            if (_currentUIState == newState) return;


            var direction = DetermineSlideDirection(newState);
            PerformTransition(newState, direction, extraTweens);
        }

        private SlideDirection DetermineSlideDirection(UIBaseState newState)
        {
            return newState.Layer > _currentUIState.Layer
                ? SlideDirection.Up
                : SlideDirection.Down;
        }

        private void PerformTransition(UIBaseState newState, SlideDirection direction, Tween?[] extraTweens)
        {
            var screenHeight = canvas.GetComponent<RectTransform>().rect.height;
            var startPosition = direction == SlideDirection.Up
                ? new Vector2(0, -screenHeight)
                : new Vector2(0, screenHeight);

            newState.RootPageElement.transform.SetAsLastSibling();

            var sequence = Sequence.Create()
                                   .Group(SlideOut(_currentUIState, -startPosition))
                                   .Group(SlideIn(newState, startPosition)
                                       .OnComplete(() => CompleteTransition(newState)));

            AddExtraTweens(sequence, extraTweens);
        }

        private void CompleteTransition(UIBaseState newState)
        {
            _currentUIState = newState;
            _stateMachine.ChangeState(newState);
        }

        private void AddExtraTweens(Sequence sequence, Tween?[] extraTweens)
        {
            foreach (var tween in extraTweens)
            {
                if (tween.HasValue)
                    sequence.Group(tween.Value);
            }
        }

        #endregion

        #region Animation Helpers

        private Tween SlideIn(UIBaseState state, Vector2 startValue)
        {
            state.RootPageElement.SetActive(true);
            var rectTransform = state.RootPageElement.GetComponent<RectTransform>();
            return SlideIn(rectTransform, startValue);
        }

        private Tween SlideIn(RectTransform rectTransform, Vector2 startValue)
        {
            rectTransform.gameObject.SetActive(true);
            rectTransform.anchoredPosition = startValue;
            return Tween.UIAnchoredPosition(rectTransform, Vector2.zero, transitionDuration, transitionEase);
        }

        private Tween SlideOut(UIBaseState state, Vector2 endValue)
        {
            var rectTransform = state.RootPageElement.GetComponent<RectTransform>();
            return SlideOut(rectTransform, endValue);
        }

        private Tween SlideOut(RectTransform rectTransform, Vector2 endValue)
        {
            return Tween.UIAnchoredPosition(rectTransform, Vector2.zero, endValue, transitionDuration, transitionEase);
        }

        #endregion

        #region Public API - State Switching

        public UITransitionBuilder SwitchToMainMenu()
            => new(this, _states.MainMenu, _currentUIState);

        public UITransitionBuilder SwitchToLevelSelect()
            => new(this, _states.LevelSelect, _currentUIState);

        public UITransitionBuilder SwitchToLevelFailed()
            => new(this, _states.LevelFailed, _currentUIState);

        public UITransitionBuilder SwitchToOptions()
        {
            var builder = new UITransitionBuilder(this, _states.Options, _currentUIState);
            if (_states.Options == null) builder.Cancel();
            return builder;
        }

        public UITransitionBuilder SwitchToInGame()
            => new(this, _states.InGame, _currentUIState, SlideFrom.Below, SlideFrom.Below);

        public UITransitionBuilder SwitchToPaused()
        {
            var builder = new UITransitionBuilder(this, _states.Paused, _currentUIState,
                                                  SlideFrom.Below, SlideFrom.Below);
            if (_isGroupRotating) builder.Cancel();
            return builder;
        }

        public UITransitionBuilder SwitchToLevelComplete()
        {
            var builder = new UITransitionBuilder(this, _states.LevelComplete, _currentUIState,
                                                  SlideFrom.Above, SlideFrom.Above);
            if (_isGroupRotating) builder.Cancel();
            return builder;
        }

        internal void ExecuteTransition(UIBaseState target, SlideFrom grid, SlideFrom background)
        {
            if (target == null || _currentUIState == target) return;

            var extras = new List<Tween?>();

            if (_states.InGame is InGameUIState inGameState)
            {
                var enteringInGame = target == _states.InGame;
                var leavingInGame = _currentUIState == _states.InGame;

                if (grid != SlideFrom.None && (enteringInGame || leavingInGame))
                    extras.Add(BuildGridTween(inGameState.GridParent.transform, grid, enteringInGame));

                if (background != SlideFrom.None && (enteringInGame || leavingInGame))
                    extras.Add(BuildBackgroundTween(inGameState, background, enteringInGame));
            }

            TransitionToState(target, extras.ToArray());
        }

        private Tween BuildGridTween(Transform gridTransform, SlideFrom direction, bool entering)
        {
            var offPos = new Vector3(0, direction == SlideFrom.Above ? 10f : -10f);
            var start = entering ? offPos : Vector3.zero;
            var end = entering ? Vector3.zero : offPos;
            return Tween.Position(gridTransform, start, end, transitionDuration, transitionEase);
        }

        private Tween BuildBackgroundTween(InGameUIState state, SlideFrom direction, bool entering)
        {
            var canvasHeight = state.BackgroundCanvas.GetComponent<RectTransform>().rect.height;
            var offPos = new Vector2(0, direction == SlideFrom.Above ? canvasHeight : -canvasHeight);
            return entering
                ? SlideIn(state.Background.rectTransform, offPos)
                : SlideOut(state.Background.rectTransform, offPos);
        }

        #endregion

        #region Level Management

        public async void ReloadLevel()
        {
            if (_levelManager?.CurrentLevel != null)
                await LoadLevel(_levelManager.CurrentLevel);
        }

        public async UniTask LoadLevel(LevelData level)
        {
            TransitionToState(_states.Loading);
            await _levelManager.LoadLevelAsync(level);
        }

        #endregion

        #region Event Handlers

        private void OnLevelCompleted(LevelCompletedEvent evt) => SwitchToLevelComplete();

        private void OnGroupRotated(GroupRotateEvent evt) =>
            _isGroupRotating = evt.RotationState == RotationState.Started;

        #endregion

        #region Helper Classes

        private sealed class UIStateCollection
        {
            public UIBaseState MainMenu { get; init; }
            public UIBaseState InGame { get; init; }
            public UIBaseState Options { get; init; }
            public UIBaseState LevelSelect { get; init; }
            public UIBaseState LevelComplete { get; init; }
            public UIBaseState LevelFailed { get; init; }
            public UIBaseState Paused { get; init; }
            public UIBaseState Loading { get; init; }

            public IEnumerable<UIBaseState> AllStates
            {
                get
                {
                    yield return MainMenu;
                    yield return InGame;
                    if (Options != null)
                        yield return Options; // remove this null check when there is actually an options screen
                    yield return LevelSelect;
                    yield return LevelComplete;
                    yield return LevelFailed;
                    yield return Paused;
                    yield return Loading;
                }
            }

            #endregion
        }
    }
}