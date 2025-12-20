using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Board
{
    // Base class with shared grid functionality
    public abstract class SpriteGrid : MonoBehaviour
    {
        [SerializeField] protected GridConfig config = new();

        protected List<Square> Squares;
        protected GridData GridData;
        protected GridLayout GridLayout;
        protected List<SquareGroup> SquareGroups;

        protected virtual void Start()
        {
            InitializeComponents();
            InitializeGrid();
            FindGroups();
        }

        protected virtual void InitializeComponents()
        {
            GridData = new GridData();
            GridLayout = new GridLayout(config, transform);
            SquareGroups = new List<SquareGroup>();
        }

        public void FindGroups()
        {
            SquareGroups = GridGroupFinder.SplitGridIntoGroups(GridData);
        }

        [ContextMenu("Arrange Grid")]
        public void InitializeGrid()
        {
            GetChildSquares();
            if (GridData == null) GridData = new GridData();
            GridData.Initialize(Squares, config.columnsPerRow);

            if (Squares.Count == 0)
            {
                Debug.LogWarning("No child sprites found to arrange in grid.");
                return;
            }

            ArrangeSpritesInGrid();
        }

        protected void GetChildSquares()
        {
            Squares = GetComponentsInChildren<Square>().OrderBy(s => s.name).ToList();
        }

        protected virtual void ArrangeSpritesInGrid()
        {
            var startPosition = GridLayout.CalculateStartPosition(Squares.Count);

            for (var row = 0; row < GridData.TotalRows; row++)
            {
                for (var column = 0; column < GridData.ColumnsPerRow; column++)
                {
                    var square = GridData.GetSquare(row, column);
                    if (square == null) continue;

                    var targetPosition =
                        GridLayout.CalculateGridPosition(row, column, startPosition, square.transform.position.z);
                    square.transform.position = targetPosition;
                }
            }
        }

        public void ClearGrid()
        {
            Squares?.Clear();

            while (transform.childCount > 0)
            {
                var child = transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }

            GridData = null;
            SquareGroups?.Clear();
        }

        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            InitializeComponents();
            InitializeGrid();
        }

        public string GetGridDebugString() => GridData?.ToString() ?? "Grid not initialized";

        public string GetAllGroupsDebugString()
        {
            if (SquareGroups == null || SquareGroups.Count == 0)
                return "No groups found.";

            var result = "";
            for (var i = 0; i < SquareGroups.Count; i++)
            {
                var group = SquareGroups[i];
                result += $"Group {i}: TopLeft({group.TopLeft.name}), TopRight({group.TopRight.name}), " +
                          $"BottomLeft({group.BottomLeft.name}), BottomRight({group.BottomRight.name}) " +
                          $"at Index{group.TopLeftIndex}\n";
            }

            return result;
        }

        public Vector2 GetGridSize() => GridLayout.GetGridSize(Squares?.Count ?? 0);

        protected virtual void OnDrawGizmos()
        {
            if (!config.showGizmos || Squares == null || Squares.Count == 0) return;

            Gizmos.color = Color.yellow;
            var startPos = GridLayout.CalculateStartPosition(Squares.Count);

            for (var i = 0; i < Squares.Count; i++)
            {
                var row = i / config.columnsPerRow;
                var column = i % config.columnsPerRow;
                var gridPoint = GridLayout.CalculateGridPosition(row, column, startPos, transform.position.z);
                Gizmos.DrawWireCube(gridPoint, Vector3.one * 0.2f);
            }

            Gizmos.color = Color.green;
            var gridSize = GetGridSize();
            var center = new Vector3(startPos.x + gridSize.x * 0.5f, startPos.y - gridSize.y * 0.5f,
                transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(gridSize.x + config.spacing.x, gridSize.y + config.spacing.y, 0));
        }
    }

    // Interactive grid for player

    // Non-interactive target/reference grid
}