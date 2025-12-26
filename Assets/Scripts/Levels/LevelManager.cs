using System.Collections.Generic;
using Attributes;
using Board;
using Singletons;
using UI.States;
using UnityEngine;

namespace Levels
{
    public sealed class LevelManager : PersistentSingleton<LevelManager>
    {
        [SerializeField] private List<LevelData> levels;
        [Required, SerializeField] private SpriteGrid playableGrid;
        [Required, SerializeField] private SpriteGrid targetGrid;
        public LevelData CurrentLevelData { get; private set; }


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

            LoadIntoGrid(initialSquares, playableGrid);
            LoadIntoGrid(targetSquares, targetGrid);

            playableGrid.Initialize();
            targetGrid.Initialize();


            var activeCount = level.GetActiveSquares(true).Count;
            Debug.Log(
                $"Loaded {targetSquares.Count} squares ({activeCount} active, {targetSquares.Count - activeCount} inactive)");
            EventBus.EventBus<LevelLoadedEvent>.Raise(new LevelLoadedEvent(level));

        }

        public static void LoadIntoGrid(List<LevelData.SquareData> squares, SpriteGrid grid)
        {
            squares.Sort((a, b) =>
            {
                var rowCompare = a.id.row.CompareTo(b.id.row);
                return rowCompare != 0 ? rowCompare : a.id.column.CompareTo(b.id.column);
            });


            foreach (var squareData in squares)
            {
                Square.Create(new GridIndex(squareData.id.row, squareData.id.column), squareData.color,
                    squareData.inactive, grid.transform);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CurrentLevelData = levels.Count > 0 ? levels[0] : null;
 
        }
    }
    
    
}