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
            public bool inactive;

            public SquareData(int row, int column, Color col, bool inactive = false)
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

            public SquareData Clone() { return new SquareData(id.row, id.column, color, inactive); }
        }

        public int rows = 4;
        public int columns = 4;

        public int movesForMaxStars = 5;
        public int movesForMidStars = 7;
        public int movesForMinStars = 8;
        public Solution solutionSteps;

        // Initial grid state (starting configuration)
        public SquareDataList initialSquares = new();

        // Target grid state (goal configuration)
        public SquareDataList targetSquares = new();

      

        public SquareData GetSquare(int row, int column, bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return list.GetSquare(row, column);
        }

        public bool HasSquare(int row, int column, bool isTarget)
        {
            return GetSquare(row, column, isTarget) != null;
        }

        public List<SquareData> GetAllSquares(bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return list.GetAllSquares();
        }

        public List<SquareData> GetActiveSquares(bool isTarget)
        {
            var list = isTarget ? targetSquares : initialSquares;
            return list.GetActiveSquares();
        }

        public void FillWithInactiveSquares(Color inactiveColor, bool applyToInitial, bool applyToTarget)
        {
            if (applyToInitial)
                initialSquares.FillWithInactive(rows, columns, inactiveColor);

            if (applyToTarget)
                targetSquares.FillWithInactive(rows, columns, inactiveColor);
        }

        public void CopyInitialToTarget()
        {
            targetSquares.CopyFrom(initialSquares);
        }

        public void CopyTargetToInitial()
        {
            initialSquares.CopyFrom(targetSquares);
        }
    }

    [System.Serializable]
    public class SquareDataList
    {
        public List<LevelData.SquareData> squares = new();

        public void Clear()
        {
            squares.Clear();
        }

        public void AddSquare(int row, int column, Color color, bool inactive)
        {
            squares.RemoveAll(s => s.id.row == row && s.id.column == column);
            squares.Add(new LevelData.SquareData(row, column, color, inactive));
        }

        public void RemoveSquare(int row, int column)
        {
            squares.RemoveAll(s => s.id.row == row && s.id.column == column);
        }

        public LevelData.SquareData GetSquare(int row, int column)
        {
            return squares.Find(s => s.id.row == row && s.id.column == column);
        }

        public List<LevelData.SquareData> GetAllSquares()
        {
            return new List<LevelData.SquareData>(squares);
        }

        public List<LevelData.SquareData> GetActiveSquares()
        {
            return squares.FindAll(s => !s.inactive);
        }

        public void FillWithInactive(int rows, int columns, Color inactiveColor)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    if (!squares.Exists(s => (s.id.row == row && s.id.column == col)))
                    {
                        squares.Add(new LevelData.SquareData(row, col, inactiveColor, true));
                    }
                }
            }
        }

        public void CopyFrom(SquareDataList other)
        {
            squares.Clear();
            foreach (var square in other.squares)
            {
                squares.Add(square.Clone());
            }
        }
        
        public int Count => squares.Count;
    }
}