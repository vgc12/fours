using Board;
using UnityEngine;

namespace Levels
{
      public sealed class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private GameObject squarePrefab;
        [SerializeField] private SpriteGrid spriteGrid;
        [SerializeField] private bool fillEmptyWithInactive = true;
        [SerializeField] private Color inactiveSquareColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        
        [ContextMenu("Load Level")]
        public void LoadLevel()
        {
            if (levelData == null || squarePrefab == null || spriteGrid == null)
            {
                Debug.LogError("Missing required references!");
                return;
            }
            
            // Fill empty positions with inactive squares
            if (fillEmptyWithInactive)
            {
                levelData.FillWithInactiveSquares(inactiveSquareColor, false,false);
            }
            
            // Clear existing squares
            while (spriteGrid.transform.childCount > 0)
            {
                DestroyImmediate(spriteGrid.transform.GetChild(0).gameObject);
            }
            
            // Create all squares (active and inactive)
            var allSquares = levelData.GetAllSquares(true);
            allSquares.Sort((a, b) =>
            {
                int rowCompare = a.Id.Row.CompareTo(b.Id.Row);
                return rowCompare != 0 ? rowCompare : a.Id.Column.CompareTo(b.Id.Column);
            });
            
            int index = 0;
            foreach (var squareData in allSquares)
            {
                GameObject square = Instantiate(squarePrefab, spriteGrid.transform);
                square.name = $"Square{index:D2}";
                
                var sq = square.GetComponent<Square>();
                sq.ID = new GridIndex(squareData.Id.Row, squareData.Id.Column);
                sq.spriteRenderer.color = squareData.color;
                sq.Inactive = !squareData.inactive;
            
                index++;
            }
            
            int activeCount = levelData.GetActiveSquares(true).Count;
            Debug.Log($"Loaded {allSquares.Count} squares ({activeCount} active, {allSquares.Count - activeCount} inactive)");
        }
        
        private void Start()
        {
            if (levelData != null)
            {
                LoadLevel();
            }
        }
    }
}