using System.Collections.Generic;
using Attributes;
using Board;
using EventBus;
using Reflex.Attributes;
using Singletons;
using UI.States;
using UnityEngine;
using ILogger = Logging.ILogger;

namespace Levels
{
    public sealed class LevelManager : PersistentSingleton<LevelManager>, ILevelManager
    {
        [ScriptableObjectDropdown, SerializeField]
        private List<LevelData> levels;

        [Required, SerializeField] private SpriteGrid playableGrid;
        [Required, SerializeField] private SpriteGrid targetGrid;
        public LevelData CurrentLevelData { get; private set; }


        [Inject] private readonly ILogger _logger;

        public List<LevelData> Levels => levels;


        [ContextMenu("Load Level")]
        public void LoadLevel(LevelData level)
        {
            if (level == null || playableGrid == null)
            {
                Debug.LogError("Missing required references!");
                return;
            }

            playableGrid.ClearGrid();
            targetGrid.ClearGrid();

            var targetSquares = level.GetAllSquares(true);
            var initialSquares = level.GetAllSquares(false);

            playableGrid.LoadIntoGrid(initialSquares);
            targetGrid.LoadIntoGrid(targetSquares);

            playableGrid.Initialize();
            targetGrid.Initialize();


            var activeCount = level.GetActiveSquares(true).Count;
            _logger.Log(
                $"Loaded {targetSquares.Count} squares ({activeCount} active, {targetSquares.Count - activeCount} inactive)");
            EventBus.EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent(level));
        }


        private EventBinding<GroupRotatedEvent> _groupRotatedBinding;

        
        private void CheckLevelWin(GroupRotatedEvent obj)
        {
            if (targetGrid.MatchesGrid(obj.GridSnapshot))
            {
                EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CurrentLevelData = levels.Count > 0 ? levels[0] : null;
            _groupRotatedBinding = new EventBinding<GroupRotatedEvent>(CheckLevelWin);
            EventBus<GroupRotatedEvent>.Register(_groupRotatedBinding);
        }
    }

    public struct LevelCompletedEvent : IEvent
    {
    }
}