using System.Collections.Generic;
using Attributes;
using Board;
using Cysharp.Threading.Tasks;
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
        public LevelData CurrentLevel { get; private set; }
        public bool LoadingInProgress { get; private set; }


        [Inject] private readonly ILogger _logger;

        public List<LevelData> Levels => levels;
        public bool HasNextLevel => levels.IndexOf(CurrentLevel) < levels.Count - 1;
        public bool HasPreviousLevel => levels.IndexOf(CurrentLevel) > 0;


        [ContextMenu("Load Level")]
        public void LoadLevel(LevelData level)
        {
            if (level == null || playableGrid == null)
            {
                Debug.LogError("Missing required references!");
                return;
            }
            LoadingInProgress = true;

            playableGrid.ClearGrid();
            targetGrid.ClearGrid();

            var targetSquares = level.GetAllSquares(true);
            var initialSquares = level.GetAllSquares(false);

            playableGrid.LoadIntoGrid(initialSquares);
            targetGrid.LoadIntoGrid(targetSquares);

            playableGrid.config.columnsPerRow = level.columns;
            targetGrid.config.columnsPerRow = level.columns;
            playableGrid.Initialize();
            targetGrid.Initialize();

            CurrentLevel = level;

            var activeCount = level.GetActiveSquares(true).Count;
            _logger.Log(
                $"Loaded {targetSquares.Count} squares ({activeCount} active, {targetSquares.Count - activeCount} inactive)");
            _logger.Log(level.solutionSteps.ToString());
            EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent(level));
            LoadingInProgress = false;
        }

        public async UniTask LoadLevelAsync(LevelData level)
        {
            if (level == null || playableGrid == null)
            {
                Debug.LogError("Missing required references!");
                return;
            }
            LoadingInProgress = true;

            playableGrid.ClearGrid();
            targetGrid.ClearGrid();

            // Do heavy work on thread pool
            var (targetSquares, initialSquares) = await UniTask.RunOnThreadPool(() =>
            {
                var target = level.GetAllSquares(true);
                var initial = level.GetAllSquares(false);
                return (target, initial);
            });

            // Back on main thread for Unity calls
            playableGrid.LoadIntoGrid(initialSquares);
            targetGrid.LoadIntoGrid(targetSquares);

            playableGrid.config.columnsPerRow = level.columns;
            targetGrid.config.columnsPerRow = level.columns;
    
            playableGrid.Initialize();
            targetGrid.Initialize();

            CurrentLevel = level;

            var activeCount = level.GetActiveSquares(true).Count;
            _logger.Log($"Loaded {targetSquares.Count} squares ({activeCount} active, {targetSquares.Count - activeCount} inactive)");
            _logger.Log(level.solutionSteps.ToString());
    
            EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent(level));
            LoadingInProgress = false;
        }

        private EventBinding<GroupRotateEvent> _groupRotatedBinding;


        public LevelData NextLevel => levels[Mathf.Min(levels.IndexOf(CurrentLevel) + 1, levels.Count - 1)];

        public LevelData PreviousLevel => levels[Mathf.Max(levels.IndexOf(CurrentLevel) - 1, 0)];

        private void CheckLevelWin(GroupRotateEvent obj)
        {
            if (obj.RotationState == RotationState.Stopped && targetGrid.MatchesGrid(obj.GridSnapshot))
            {
                EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CurrentLevel = levels.Count > 0 ? levels[0] : null;
            _groupRotatedBinding = new EventBinding<GroupRotateEvent>(CheckLevelWin);
            EventBus<GroupRotateEvent>.Register(_groupRotatedBinding);
        }
    }

    public struct LevelCompletedEvent : IEvent
    {
    }
}