

using System;
using Attributes;
using DependencyInjection;
using EventBus;
using Extensions;
using Levels;
using Singletons;
using StateMachine;
using UI.States;
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
        

        protected override void Awake()
        {
            base.Awake();
            _uiDocument = transform.GetOrAdd<UIDocument>();
            _root = _uiDocument.rootVisualElement;
            _mainMenuState = new MainMenuState(_root.Q<VisualElement>("main-menu"), this);
            _inGameState = new InGameUIState(_root.Q<VisualElement>("in-game"), this);
             
            // _optionsState = new OptionsState(_root, this);
            _levelSelectState = new LevelSelectState(_root.Q<VisualElement>("level-select"), this);

            _stateMachine.AddState(_mainMenuState);
            _stateMachine.AddState(_inGameState);
              
            _stateMachine.SetStateAndEnter( _mainMenuState);
 
        }

        
        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
        
        public void SwitchToInGame()
        {
            _stateMachine.ChangeState(_inGameState);
        }
        
        public void SwitchToMainMenu()
        {
            _stateMachine.ChangeState(_mainMenuState);
        }
        
        public void SwitchToLevelSelect()
        {
           // _stateMachine.ChangeState(_levelSelectState);
           _stateMachine.ChangeState(_inGameState);
           LevelManager.instance.LoadLevel(LevelManager.instance.CurrentLevelData);
        }

        public void SwitchToOptions()
        {
            _stateMachine.ChangeState(_optionsState);
        }
    }
}
