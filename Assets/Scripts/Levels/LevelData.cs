// LevelData.cs - Enhanced with initial and target grid states

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
            public GridIndex Id { get; }
            public Color color;
            public bool inactive ;
            
            public SquareData (int row, int column, Color col, bool inactive = false)
            {
                Id = new GridIndex(row, column);
                color = col;
                this.inactive = inactive;
            }
            
            public SquareData(GridIndex id, Color col, bool inactive = false)
            {
                Id = id;
                color = col;
                this.inactive = inactive;
            }
            
            public SquareData Clone()
            {
                return new SquareData(Id.Row, Id.Column, color, inactive);
            }
        }
        
        public int rows = 4;
        public int columns = 4;
        
        // Initial grid state (starting configuration)
        public List<SquareData> initialSquares = new List<SquareData>();
        
        // Target grid state (goal configuration)
        public List<SquareData> targetSquares = new List<SquareData>();
        
        public void Clear(bool clearInitial = true, bool clearTarget = true)
        {
            if (clearInitial) initialSquares.Clear();
            if (clearTarget) targetSquares.Clear();
        }
        
        public void AddSquare(int row, int column, Color color, bool inactive, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            list.RemoveAll(s => s.Id.Row == row && s.Id.Column == column);
            list.Add(new SquareData(row, column, color, inactive));
        }
        
        public void RemoveSquare(int row, int column, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            list.RemoveAll(s => s.Id.Row == row && s.Id.Column == column);
        }
        
        public SquareData GetSquare(int row, int column, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return list.Find(s => s.Id.Row == row && s.Id.Column == column);
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
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (!list.Exists(s => (s.Id.Row == row && s.Id.Column == col) || s.inactive)) 
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