using System.Linq;
using Attributes;
using DependencyInjection;
using EventBus;
using Levels;
using Singletons;
using StateMachine;
using UI.States;
using UnityEngine;
using UnityEngine.UI;


namespace UI
{
    public sealed class UIManager : PersistentSingleton<UIManager>
    {
        private readonly StateMachine.StateMachine _stateMachine = new();
        [Required, SerializeField] private GameObject mainMenu;
        [Required, SerializeField] private GameObject inGameUI;
        [Required, SerializeField] private GameObject optionsMenu;
        [Required, SerializeField] private GameObject levelSelectMenu;
        [Required, SerializeField] private GameObject levelCompleteMenu;
        [Required, SerializeField] private GameObject levelFailedMenu;

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

            _mainMenuState = new MainMenuState(mainMenu, this);
            _inGameState = new InGameUIState(inGameUI, this);

            // _optionsState = new OptionsState(_root, this);
            _levelSelectState = new LevelSelectState(levelSelectMenu, this);
            _levelCompleteState = new LevelCompleteState(levelCompleteMenu, this);
            _levelFailedState = new LevelFailedState(levelFailedMenu, this);
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
        
        
        
        public void ReloadLevel()
        {
            var levelManager = LevelManager.Instance;
            if (levelManager == null || levelManager.CurrentLevel == null)
            {
                return;
            }

            levelManager.LoadLevel(levelManager.CurrentLevel);
            SwitchToInGame();
        }
    }

    public sealed class LevelCompleteState : UIBaseState
    {
        private readonly ILevelManager _levelManager;
        private readonly Button _nextLevelButton;
        private readonly Button _mainMenuButton;

        public LevelCompleteState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            RuntimeResolver.Instance.TryResolve(out _levelManager);
            var buttons = RootPageElement.GetComponentsInChildren<Button>();
            _nextLevelButton = buttons.First(b => b.name == "next-level-button");
            _mainMenuButton = buttons.First(b => b.name == "main-menu-button");

            var nextLevel = _levelManager.NextLevel;

            if (nextLevel == _levelManager.CurrentLevel)
            {
                _nextLevelButton.gameObject.SetActive(false);
            }

            _nextLevelButton.onClick.AddListener(() =>
            {
                if (nextLevel == null)
                {
                    return;
                }

                _levelManager.LoadLevel(nextLevel);
                UIManager.SwitchToInGame();
            });
        }
    }

    public sealed class LevelFailedState : UIBaseState
    {
        public LevelFailedState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager) { }
    }
}