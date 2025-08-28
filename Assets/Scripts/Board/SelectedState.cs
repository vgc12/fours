using StateMachines;
using UnityEngine;

namespace Board
{
    public class SelectedState : BaseState
    {
        private readonly SpriteGrid _grid;

        public SelectedState(SpriteGrid grid)
        {
            _grid = grid;
        }

        public override void Enter()
        {
            Debug.Log(_grid.GetGridDebugString());
        }
    }
}