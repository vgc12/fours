using Board;
using EventBus;
using Levels;
using Logging;
using UI.UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.States
{
    public sealed class LevelSelectState : UIBaseState
    {
        public LevelSelectState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
        }
    }


    public sealed class MainMenuState : UIBaseState
    {
        private readonly Button _playButton;
        private readonly Button _optionsButton;

        public MainMenuState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            _playButton = RootPageElement.Q<Button>("play-button");
            _optionsButton = RootPageElement.Q<Button>("options-button");



            _playButton.clicked +=
                UIManager.SwitchToLevelSelect;
            _optionsButton.clicked += UIManager.SwitchToOptions;
        }

        ~MainMenuState()
        {
            _playButton.clicked -= UIManager.SwitchToLevelSelect;
            _optionsButton.clicked -= UIManager.SwitchToOptions;
        }
    }

    public sealed class InGameUIState : UIBaseState
    {
        private readonly Label _movesLabel;
        private readonly EventBinding<PlayerMovedEvent> _playerMovedEventBinding;
        private readonly EventBinding<LevelLoadedEvent> _levelSelectStateBinding;

        public InGameUIState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            _movesLabel = RootPageElement.Q<Label>("moves-label");
            _playerMovedEventBinding = new EventBinding<PlayerMovedEvent>(OnPlayerMoved);
            _levelSelectStateBinding = new EventBinding<LevelLoadedEvent>(OnLevelLoaded);
            _movesLabel.text = $"Moves Remaining: {LevelManager.Instance.CurrentLevelData.movesAllowed}" ;
            EventBus<PlayerMovedEvent>.Register(_playerMovedEventBinding);
            EventBus<LevelLoadedEvent>.Register(_levelSelectStateBinding);
        }

        private void OnLevelLoaded(LevelLoadedEvent obj)
        {
            _movesLabel.text = $"Moves Remaining: {obj.LevelData.movesAllowed}";
        }

        public void OnPlayerMoved(PlayerMovedEvent evt)
        {
            _movesLabel.text = $"Moves Remaining: {evt.MovesRemaining}";
        }
        
        ~InGameUIState()
        {
            EventBus<PlayerMovedEvent>.Deregister(_playerMovedEventBinding);
            EventBus<LevelLoadedEvent>.Deregister(_levelSelectStateBinding);
        }
    }

    public class LevelLoadedEvent : IEvent
    {
        public LevelData LevelData { get; }

        public LevelLoadedEvent(LevelData levelData)
        {
            LevelData = levelData;
        }
   
    }
    public class PlayerMovedEvent : IEvent
    {
        public SpriteGrid CurrentGrid { get; }
        public int MovesRemaining { get; }

        public PlayerMovedEvent(SpriteGrid currentGrid, int movesRemaining)
        {
            CurrentGrid = currentGrid;
            MovesRemaining = movesRemaining;
        }
    }
}