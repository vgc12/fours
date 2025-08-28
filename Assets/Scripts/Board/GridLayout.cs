using UnityEngine;

namespace Board
{
    public class GridLayout
    {
        private readonly GridConfig _config;
        private readonly Transform _gridTransform;

        public GridLayout(GridConfig config, Transform gridTransform)
        {
            _config = config;
            _gridTransform = gridTransform;
        }

        public Vector2 CalculateStartPosition(int totalSquares)
        {
            Vector2 startPos = _gridTransform.position;

            if (_config.centerGrid && totalSquares > 0)
            {
                var totalRows = Mathf.CeilToInt((float)totalSquares / _config.columnsPerRow);
                var gridWidth = (_config.columnsPerRow - 1) * _config.spacing.x;
                var gridHeight = (totalRows - 1) * _config.spacing.y;

                startPos.x -= gridWidth * 0.5f;
                startPos.y += gridHeight * 0.5f;
            }

            return startPos + _config.gridOffset;
        }

        public Vector3 CalculateGridPosition(int row, int column, Vector2 startPosition, float zPosition = 0)
        {
            return new Vector3(
                startPosition.x + (column * _config.spacing.x),
                startPosition.y - (row * _config.spacing.y),
                zPosition
            );
        }

        public Vector2 GetGridSize(int totalSquares)
        {
            if (totalSquares == 0) return Vector2.zero;

            var totalRows = Mathf.CeilToInt((float)totalSquares / _config.columnsPerRow);
            return new Vector2(
                (_config.columnsPerRow - 1) * _config.spacing.x,
                (totalRows - 1) * _config.spacing.y
            );
        }
    }
}