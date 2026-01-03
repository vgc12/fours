using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Board.Commands
{
    public sealed class SelectDotCommand : BaseCommand
    {
        private readonly PlayableGrid _grid;
        private readonly Dot _dotToSelect;
        private readonly Dot _previouslySelectedDot;

        public override string Description => 
            $"Select dot {(_dotToSelect?.name ?? "none")}";

        public SelectDotCommand(PlayableGrid grid, Dot dotToSelect)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _dotToSelect = dotToSelect;
            _previouslySelectedDot = grid.SelectedDot;
        }

        public override async UniTask<bool> Execute()
        {
            
            try
            {
            
                
                _grid.PreviouslySelectedDot = _previouslySelectedDot;
             
        

                _grid.SelectedDot = _dotToSelect;
                if (_grid.PreviouslySelectedDot != null && _grid.PreviouslySelectedDot != _dotToSelect  && _grid.PreviouslySelectedDot.SquareGroup.Selected)
                {
                   await _grid.PreviouslySelectedDot.SquareGroup.Deselect();
                }
                if (!_grid.SelectedDot.SquareGroup.Selected)
                {
                    _grid.SelectedDot.SquareGroup.Select();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to execute select dot command: {ex.Message}");
                return false;
            }
        }
  
        public override async UniTask<bool> Undo()
        {
            try
            {
                _grid.SelectedDot = _previouslySelectedDot;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to undo select dot command: {ex.Message}");
                return false;
            }
        }
    }
}