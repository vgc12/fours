using UnityEngine;

namespace Board
{
    public sealed class TargetGrid : SpriteGrid
    {
        
        // Override to prevent gizmos on target grid if desired
        protected override void OnDrawGizmos()
        {
            // Optionally draw different colored gizmos for target grid
            if (!config.showGizmos || Squares == null || Squares.Count == 0) return;
            
            Gizmos.color = Color.cyan; // Different color for target
            var startPos = GridLayout.CalculateStartPosition(Squares.Count);

            for (var i = 0; i < Squares.Count; i++)
            {
                var row = i / config.columnsPerRow;
                var column = i % config.columnsPerRow;
                var gridPoint = GridLayout.CalculateGridPosition(row, column, startPos, transform.position.z);
                Gizmos.DrawWireSphere(gridPoint, 0.15f); // Different shape for target
            }
        }
    }
}