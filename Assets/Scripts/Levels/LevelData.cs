// LevelData.cs - ScriptableObject to store level configurations

using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
      [CreateAssetMenu(fileName = "NewLevel", menuName = "Board/Level Data")]
    public class LevelData : ScriptableObject
    {
        [System.Serializable]
        public class SquareData
        {
            public int row;
            public int column;
            public Color color;
            public bool isActive = true; // Whether this square participates in gameplay
            
            public SquareData(int r, int c, Color col, bool active = true)
            {
                row = r;
                column = c;
                color = col;
                isActive = active;
            }
        }
        
        public int rows = 4;
        public int columns = 4;
        public List<SquareData> squares = new List<SquareData>();
        
        public void Clear()
        {
            squares.Clear();
        }
        
        public void AddSquare(int row, int column, Color color, bool isActive = true)
        {
            squares.RemoveAll(s => s.row == row && s.column == column);
            squares.Add(new SquareData(row, column, color, isActive));
        }
        
        public void RemoveSquare(int row, int column)
        {
            squares.RemoveAll(s => s.row == row && s.column == column);
        }
        
        public SquareData GetSquare(int row, int column)
        {
            return squares.Find(s => s.row == row && s.column == column);
        }
        
        public bool HasSquare(int row, int column)
        {
            return GetSquare(row, column) != null;
        }
        
        // Get all squares including inactive ones (for grid creation)
        public List<SquareData> GetAllSquares()
        {
            return new List<SquareData>(squares);
        }
        
        // Get only active squares (for gameplay logic)
        public List<SquareData> GetActiveSquares()
        {
            return squares.FindAll(s => s.isActive);
        }
        
        // Fill empty positions with inactive squares to maintain rectangular grid
        public void FillWithInactiveSquares(Color inactiveColor)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (!HasSquare(row, col))
                    {
                        AddSquare(row, col, inactiveColor, false);
                    }
                }
            }
        }
    }
}
