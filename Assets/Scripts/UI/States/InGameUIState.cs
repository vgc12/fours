using DependencyInjection;
using EventBus;
using Levels;
using TMPro;
using UnityEngine;


namespace UI.States
{
    public sealed class InGameUIState : UIBaseState
    {
        private readonly TMP_Text _movesLabel;
        private readonly EventBinding<PlayerMovedEvent> _playerMovedEventBinding;
        private readonly EventBinding<LevelLoadedEvent> _levelSelectStateBinding;
        private readonly LevelManager _levelManager;

        public InGameUIState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {

            _movesLabel = rootElement.gameObject.GetComponentInChildren<TMP_Text>();
            _playerMovedEventBinding = new EventBinding<PlayerMovedEvent>(OnPlayerMoved);
            _levelSelectStateBinding = new EventBinding<LevelLoadedEvent>(OnLevelLoaded);
            RuntimeResolver.Instance.TryResolve(out _levelManager);
            if (_levelManager.CurrentLevel)
            {
                _movesLabel.text = $"Moves Remaining: {_levelManager.CurrentLevel.movesAllowed}";
            }

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