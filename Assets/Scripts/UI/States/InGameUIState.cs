using EventBus;
using Levels;
using UI.UI.States;
using UnityEngine.UIElements;

namespace UI.States
{
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
}