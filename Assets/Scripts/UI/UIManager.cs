using System;
using Attributes;
using DependencyInjection;
using EventBus;
using Extensions;
using Levels;
using Singletons;
using StateMachine;
using UI.States;
using UI.UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIManager : PersistentSingleton<UIManager>
    {
        private readonly StateMachine.StateMachine _stateMachine = new();
        private UIDocument _uiDocument;
        private VisualElement _root;

        private IState _mainMenuState;
        private IState _optionsState;
        private IState _levelSelectState;
        private IState _inGameState;
        private IState _levelCompleteState;
        private IState _levelFailedState;


        private EventBinding<LevelCompletedEvent> _levelCompletedBinding;

        protected override void Awake()
        {
            base.Awake();
            _uiDocument = transform.GetOrAdd<UIDocument>();
            _root = _uiDocument.rootVisualElement;
            _mainMenuState = new MainMenuState(_root.Q<VisualElement>("main-menu"), this);
            _inGameState = new InGameUIState(_root.Q<VisualElement>("in-game"), this);

            // _optionsState = new OptionsState(_root, this);
            _levelSelectState = new LevelSelectState(_root.Q<VisualElement>("level-select"), this);
            _levelCompleteState = new LevelCompleteState(_root.Q<VisualElement>("level-complete"), this);
            _levelFailedState = new LevelFailedState(_root.Q<VisualElement>("level-failed"), this);
            //_optionsState = new OptionsState(_root.Q<VisualElement>("options"), this);

            

            _stateMachine.AddState(_mainMenuState);
            _stateMachine.AddState(_inGameState);

            _stateMachine.AddState(_levelSelectState);
            _stateMachine.AddState(_levelCompleteState);
            _stateMachine.AddState(_levelFailedState);
            // _stateMachine.AddState(_optionsState);
            
            _stateMachine.SetStateAndEnter(_mainMenuState);

            _levelCompletedBinding = new EventBinding<LevelCompletedEvent>(OnLevelCompleted);
            EventBus<LevelCompletedEvent>.Register(_levelCompletedBinding);
        }

        private void OnLevelCompleted(LevelCompletedEvent obj) { SwitchToLevelComplete(); }


        private void Update() => _stateMachine.Update(); 

        private void FixedUpdate() => _stateMachine.FixedUpdate(); 

        public void SwitchToInGame() => _stateMachine.ChangeState(_inGameState); 

        public void SwitchToMainMenu() => _stateMachine.ChangeState(_mainMenuState); 

        public void SwitchToLevelComplete() => _stateMachine.ChangeState(_levelCompleteState);

        public void SwitchToLevelFailed() => _stateMachine.ChangeState(_levelFailedState);
        
        public void SwitchToLevelSelect() => _stateMachine.ChangeState(_levelSelectState);

        public void SwitchToOptions() => _stateMachine.ChangeState(_optionsState);
    }

    public sealed class LevelCompleteState : UIBaseState
    {
        private readonly ILevelManager _levelManager;
        private readonly Button _nextLevelButton;
        private readonly Button _mainMenuButton;

        public LevelCompleteState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
             RuntimeResolver.Instance.TryResolve(out _levelManager);
            _nextLevelButton = RootPageElement.Q<Button>("next-level-button");
            _mainMenuButton = RootPageElement.Q<Button>("main-menu-button");

            var currentLevelIndex = _levelManager.Levels.IndexOf(_levelManager.CurrentLevelData);
            if (currentLevelIndex + 1 >= _levelManager.Levels.Count)
                _nextLevelButton.clicked += () => _levelManager.LoadLevel(_levelManager.Levels[0]);
             else RootPageElement.Remove(_nextLevelButton);
            _mainMenuButton.clicked += uiManager.SwitchToMainMenu;
        }
    }

    public sealed class LevelFailedState : UIBaseState
    {
        public LevelFailedState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager) { }
    }
}