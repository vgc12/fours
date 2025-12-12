using System;
using System.Threading.Tasks;
using Board.Commands;
using UnityEngine;

namespace Board
{
    public class SelectDotCommand : BaseCommand
    {
        private readonly SpriteGrid _grid;
        private readonly Dot _dotToSelect;
        private readonly Dot _previouslySelectedDot;

        public override string Description => 
            $"Select dot {(_dotToSelect?.name ?? "none")}";

        public SelectDotCommand(SpriteGrid grid, Dot dotToSelect)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _dotToSelect = dotToSelect;
            _previouslySelectedDot = grid.SelectedDot;
        }

        public override Task<bool> Execute()
        {
            try
            {
                _grid.SelectedDot = _dotToSelect;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to execute select dot command: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public override Task<bool> Undo()
        {
            try
            {
                _grid.SelectedDot = _previouslySelectedDot;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to undo select dot command: {ex.Message}");
                return Task.FromResult(false);
            }
        }
    }
}