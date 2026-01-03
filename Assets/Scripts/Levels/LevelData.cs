

using System.Collections.Generic;
using Board;
using UnityEngine;

namespace Levels
{
    
    [CreateAssetMenu(fileName = "NewLevel", menuName = "Board/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        [System.Serializable]
        public class SquareData
        {
            public GridIndex id;
            public Color color;
            public bool inactive ;
            
            public SquareData (int row, int column, Color col, bool inactive = false)
            {
                id = new GridIndex(row, column);
                color = col;
                this.inactive = inactive;
            }
            
            public SquareData(GridIndex id, Color col, bool inactive = false)
            {
                this.id = id;
                color = col;
                this.inactive = inactive;
            }
            
            public SquareData Clone()
            {
                return new SquareData(id.row, id.column, color, inactive);
            }
        }
        
        
        public int rows = 4;
        public int columns = 4;
        public int movesAllowed = 10;
        public int movesForMaxStars = 5;
        public int movesForMidStars = 7;
        public int movesForMinStars = 8;
        
        // Initial grid state (starting configuration)
        public List<SquareData> initialSquares = new();
        
        // Target grid state (goal configuration)
        public List<SquareData> targetSquares = new();
        
        public void Clear(bool clearInitial = true, bool clearTarget = true)
        {
            if (clearInitial) initialSquares.Clear();
            if (clearTarget) targetSquares.Clear();
        }
        
        public void AddSquare(int row, int column, Color color, bool inactive, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            list.RemoveAll(s => s.id.row == row && s.id.column == column);
            list.Add(new SquareData(row, column, color, inactive));
        }
        
        public void RemoveSquare(int row, int column, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            list.RemoveAll(s => s.id.row == row && s.id.column == column);
        }
        
        public SquareData GetSquare(int row, int column, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return list.Find(s => s.id.row == row && s.id.column == column);
        }
        
        public bool HasSquare(int row, int column, bool isTarget)
        {
            return GetSquare(row, column, isTarget) != null;
        }
        
        public List<SquareData> GetAllSquares(bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return new List<SquareData>(list);
        }
        
        public List<SquareData> GetActiveSquares(bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return list.FindAll(s => !s.inactive);
        }
        
        public void FillWithInactiveSquares(Color inactiveColor, bool applyToInitial, bool applyToTarget)
        {
            if (applyToInitial)
                FillListWithInactive(initialSquares, inactiveColor);
            
            if (applyToTarget)
                FillListWithInactive(targetSquares, inactiveColor);
        }
        
        private void FillListWithInactive(List<SquareData> list, Color inactiveColor)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    if (!list.Exists(s => (s.id.row == row && s.id.column == col))) 
                    {
                        list.Add(new SquareData(row, col, inactiveColor, true) );
                    }
                }
            }
        }
        
        public void CopyInitialToTarget()
        {
            targetSquares.Clear();
            foreach (var square in initialSquares)
            {
                targetSquares.Add(square.Clone());
            }
        }
        
        public void CopyTargetToInitial()
        {
            initialSquares.Clear();
            foreach (var square in targetSquares)
            {
                initialSquares.Add(square.Clone());
            }
        }
    }
}