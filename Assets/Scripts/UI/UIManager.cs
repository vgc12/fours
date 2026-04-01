using System;
using System.Collections.Generic;
using Attributes;
using Cysharp.Threading.Tasks;
using DependencyInjection;
using EventBus;
using Levels;
using PrimeTween;
using Singletons;
using UI.States;
using UnityEngine;

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

        // State management
        private readonly StateMachine.StateMachine _stateMachine = new();
        private readonly Dictionary<UIBaseState, int> _stateHierarchy = new();
        private UIStateCollection _states;
        private UIBaseState _currentUIState;

        // Dependencies
        private ILevelManager _levelManager;
        private EventBinding<LevelCompletedEvent> _levelCompletedBinding;

        protected override void Awake()
        {
            base.Awake();
            InitializeDependencies();
            InitializeStates();
            SetupStateHierarchy();
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

        private void SetupStateHierarchy()
        {
            _stateHierarchy[_states.MainMenu] = 0;
            _stateHierarchy[_states.LevelSelect] = 1;
            _stateHierarchy[_states.Loading] = 2;
            _stateHierarchy[_states.InGame] = 3;
            _stateHierarchy[_states.Paused] = 2;
            _stateHierarchy[_states.LevelComplete] = 4;
            _stateHierarchy[_states.LevelFailed] = 4;
            // if (_states.Options != null)
            //     _stateHierarchy[_states.Options] = 1;
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
        }

        private void UnsubscribeFromEvents() { EventBus<LevelCompletedEvent>.Deregister(_levelCompletedBinding); }

        #endregion

        #region State Machine Updates

        private void Update() => _stateMachine.Update();
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
            return _stateHierarchy[newState] > _stateHierarchy[_currentUIState]
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

        public void SwitchToMainMenu() => TransitionToState(_states.MainMenu);
        public void SwitchToLevelSelect() => TransitionToState(_states.LevelSelect);
        public void SwitchToLevelComplete() => TransitionToState(_states.LevelComplete);
        public void SwitchToLevelFailed() => TransitionToState(_states.LevelFailed);
        public void SwitchToOptions() => TransitionToState(_states.Options);

        public void SwitchToPaused()
        {
            if (_states.InGame is not InGameUIState inGameState) return;
            TransitionToState(_states.Paused, 
                CreateGridExitTween(_states.Paused.RootPageElement.transform),
                CreateBackgroundExitTween(inGameState));
        }

        public void SwitchToInGame()
        {
            if (_states.InGame is not InGameUIState inGameState) return;

            TransitionToState(_states.InGame,
                CreateGridEntryTween(inGameState.GridParent.transform),
                CreateBackgroundEntryTween(inGameState));
        }

        private Tween CreateGridEntryTween(Transform gridParent)
        {
            return Tween.Position(
                gridParent.transform,
                new Vector3(0, -10),
                Vector2.zero,
                transitionDuration,
                transitionEase);
        }
        private Tween CreateGridExitTween(Transform gridParent)
        {
           return Tween.Position(
                gridParent.transform,
                Vector2.zero,
                new Vector3(0, -10),
                transitionDuration,
                transitionEase);
        }

        private Tween CreateBackgroundEntryTween(InGameUIState state)
        {
            var canvasHeight = state.BackgroundCanvas.GetComponent<RectTransform>().rect.height;
            return SlideIn(state.Background.rectTransform, new Vector2(0, -canvasHeight));
        }
        private Tween CreateBackgroundExitTween(InGameUIState state)
        {
            var canvasHeight = state.BackgroundCanvas.GetComponent<RectTransform>().rect.height;
            return SlideOut(state.Background.rectTransform, new Vector2(0, -canvasHeight));
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

        #endregion

        #region Helper Classes

        private class UIStateCollection
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
                    if (Options != null) yield return Options;
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
