using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Board.Commands
{
    
    public sealed class RotateGroupCommand : BaseCommand
    {
        private readonly SquareGroup _squareGroup;
        private readonly GridData _gridData;
        private readonly RotationDirection _direction;
        private bool _wasExecuted = false;

        public override string Description => 
            $"Rotate group at {_squareGroup.TopLeftIndex} {_direction.ToString()}";

        public RotateGroupCommand(SquareGroup squareGroup, GridData gridData, RotationDirection direction)
        {
            _squareGroup = squareGroup ?? throw new ArgumentNullException(nameof(squareGroup));
            _gridData = gridData ?? throw new ArgumentNullException(nameof(gridData));
            _direction = direction;
        }

        public override async UniTask<bool> Execute()
        {
            if (_wasExecuted) return false;

            try
            {
                if (_direction == RotationDirection.Clockwise)
                {
                    await _squareGroup.RotateClockwise();
                }
                else
                {
                    await _squareGroup.RotateCounterClockwise();
                }
                
                _gridData.UpdateWithGroup(_squareGroup);
                _wasExecuted = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to execute rotate command: {ex.Message}");
                return false;
            }
        }

        public override async UniTask<bool> Undo()
        {
            if (!_wasExecuted) return false;

            try
            {
                // Undo rotation by rotating in opposite direction
                if (_direction == RotationDirection.Clockwise)
                {
                    await _squareGroup.RotateCounterClockwise();
                }
                else
                {
                    await _squareGroup.RotateClockwise();
                }
                
                _gridData.UpdateWithGroup(_squareGroup);
                _wasExecuted = false;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to undo rotate command: {ex.Message}");
                return false;
            }
        }
    }
}