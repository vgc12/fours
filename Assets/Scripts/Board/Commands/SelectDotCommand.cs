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
                // Scale previous squares back to normal
            

                _grid.PreviouslySelectedDot = _previouslySelectedDot;
                _grid.SelectedDot = _dotToSelect;
        
                
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