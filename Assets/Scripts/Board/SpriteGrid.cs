using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Levels;
using Logging;
using Reflex.Attributes;
using UnityEngine;
using ILogger = Logging.ILogger;

namespace Board
{
    // Base class with shared grid functionality
    public abstract class SpriteGrid : MonoBehaviour
    {
        [SerializeField] public GridConfig config = new();

        [Inject] protected readonly FoursLogger Logger;

        protected List<Square> Squares;
        protected GridData GridData;
        protected GridLayout GridLayout;
        protected List<SquareGroup> SquareGroups;
        [SerializeField] protected SquareFactory squareSquareFactory;

        // Total width and height properties
        public float Width
        {
            get
            {
                if (Squares == null || Squares.Count == 0) return 0f;
                var gridSize = GridLayout.GetGridSize(Squares.Count);
                return gridSize.x;
            }
        }

        public float Height
        {
            get
            {
                if (Squares == null || Squares.Count == 0) return 0f;
                var gridSize = GridLayout.GetGridSize(Squares.Count);
                return gridSize.y;
            }
        }

        protected virtual void Start() { Initialize(); }

        public virtual void Initialize()
        {
            InitializeComponents();
            InitializeGrid();
            FindGroups();
        }

        public bool MatchesGrid(string otherGridSnapshot) => otherGridSnapshot == CurrentGridStateSnapshot;


        protected virtual void InitializeComponents()
        {
            GridData = new GridData();
            GridLayout = new GridLayout(config, transform);
            SquareGroups = new List<SquareGroup>();
        }

        public void FindGroups() { SquareGroups = GridGroupFinder.SplitGridIntoGroups(GridData); }

        [ContextMenu("Arrange Grid")]
        public void InitializeGrid()
        {
            Squares = ChildSquares.ToList();
            if (GridData == null) GridData = new GridData();
            GridData.Initialize(Squares, config.columnsPerRow);

            if (Squares.Count == 0)
            {
                Debug.LogWarning("No child sprites found to arrange in grid.");
                return;
            }


            ArrangeSpritesInGrid();
            // await ScaleGridToFit( marginPercent: .2f, ct: destroyCancellationToken);
            // ArrangeSpritesInGrid();
        }

        private async UniTask ScaleGridToFit(
            float maxScale = 1f, float minScale = 0.1f, float marginPercent = 0.05f, CancellationToken ct = default
        )
        {
            float scaleStep = 0.01f;

            if (Squares == null || Squares.Count == 0 || !Application.isPlaying)
                return;
            // Shrink phase
            while (!AllSquaresInBounds(marginPercent) && transform.localScale.x > minScale &&
                   !ct.IsCancellationRequested)
            {
                transform.localScale -= Vector3.one * scaleStep;
                await UniTask.Yield(ct);
            }

            // Grow phase
            while (transform.localScale.x < maxScale && !ct.IsCancellationRequested)
            {
                Vector3 testScale = transform.localScale + Vector3.one * scaleStep;
                transform.localScale = testScale;
                await UniTask.Yield(ct);

                if (!AllSquaresInBounds(marginPercent))
                {
                    transform.localScale -= Vector3.one * scaleStep;
                    break;
                }
            }

            await UniTask.Yield(ct);
        }

        private bool AllSquaresInBounds(float marginPercent)
        {
            foreach (var square in Squares)
            {
                if (square == null || square.transform == null)
                    continue;
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(square.transform.position);

                // Check if within bounds with margin
                if (viewportPos.x < marginPercent || viewportPos.x > 1 - marginPercent ||
                    viewportPos.y < marginPercent || viewportPos.y > 1 - marginPercent ||
                    viewportPos.z < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public void LoadIntoGrid(List<LevelData.SquareData> squares)
        {
            squares.Sort((a, b) =>
            {
                var rowCompare = a.id.row.CompareTo(b.id.row);
                return rowCompare != 0 ? rowCompare : a.id.column.CompareTo(b.id.column);
            });


            foreach (var squareData in squares)
            {
                var squareParams = new SquareCreationParams()
                {
                    Color = squareData.color,
                    Inactive = squareData.inactive, Parent = transform,
                    Id = new GridIndex(squareData.id.row, squareData.id.column)
                };
                squareSquareFactory.Create(squareParams);
            }
        }

        private IEnumerable<Square> ChildSquares
        {
            get { return GetComponentsInChildren<Square>().OrderBy(s => s.name); }
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
        
        protected string CurrentGridStateSnapshot
        {
            get
            {
                // Create a snapshot of the current grid state by capturing positions of all squares
                var snapshot = new StringBuilder();

                foreach (var group in SquareGroups)
                {
                    if (group == null || group.AnyAreNull) continue;
                    var squares = new[]
                    {
                        group.TopLeft,
                        group.TopRight,
                        group.BottomLeft,
                        group.BottomRight
                    };
                    foreach (var square in squares)
                    {
                        if (square != null)
                        {
                            snapshot.Append($"{square.SpriteRenderer.color}");
                        }
                    }
                }

                return snapshot.ToString();
            }
        }

        public void ClearGrid()
        {
            SquareGroups?.Clear();
            Squares?.Clear();
            var gos = GetComponentsInChildren<Transform>().Where(t => t != transform).Select(t => t.gameObject)
                                                          .ToList();
            gos.ForEach(DestroyImmediate);
            GridData = null;
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
}