using Board;
using UnityEngine;

namespace Levels
{
      public sealed class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private GameObject squarePrefab;
        [SerializeField] private SpriteGrid spriteGrid;

        
        [ContextMenu("Load Level")]
        public void LoadLevel()
        {
            if (levelData == null || squarePrefab == null || spriteGrid == null)
            {
                Debug.LogError("Missing required references!");
                return;
            }
            
   
            
            // Clear existing squares
            while (spriteGrid.transform.childCount > 0)
            {
                DestroyImmediate(spriteGrid.transform.GetChild(0).gameObject);
            }
            
            // Create all squares (active and inactive)
            var allSquares = levelData.GetAllSquares(false);
            allSquares.Sort((a, b) =>
            {
                int rowCompare = a.id.row.CompareTo(b.id.row);
                return rowCompare != 0 ? rowCompare : a.id.column.CompareTo(b.id.column);
            });
            
            int index = 0;
            foreach (var squareData in allSquares)
            {
                GameObject square = Instantiate(squarePrefab, spriteGrid.transform);
                square.name = $"Square{index:D2}";
                
                var sq = square.GetComponent<Square>();
                sq.ID = new GridIndex(squareData.id.row, squareData.id.column);
                sq.spriteRenderer.color = squareData.color;
                sq.Inactive = squareData.inactive;
            
                index++;
            }
            
            int activeCount = levelData.GetActiveSquares(true).Count;
            Debug.Log($"Loaded {allSquares.Count} squares ({activeCount} active, {allSquares.Count - activeCount} inactive)");
        }
        
        private void Awake()
        {
            if (levelData != null)
            {
                LoadLevel();
            }
        }
    }
}