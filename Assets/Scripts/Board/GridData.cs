using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public sealed class GridData
    {
        private Square[,] _grid;
        private int _totalRows;
        private int _columnsPerRow;

        public int TotalRows => _totalRows;
        public int ColumnsPerRow => _columnsPerRow;

        public void Initialize(List<Square> squares, int columnsPerRow)
        {
            _columnsPerRow = columnsPerRow;
            _totalRows = Mathf.CeilToInt((float)squares.Count / columnsPerRow);
            _grid = new Square[_totalRows, columnsPerRow];

            for (var i = 0; i < squares.Count; i++)
            {
                var row = i / columnsPerRow;
                var column = i % columnsPerRow;
                _grid[row, column] = squares[i];
                //squares[i].ID = new GridIndex(row, column);
            }
        }

        public Square GetSquare(int row, int column)
        {
            if (_grid == null || row < 0 || row >= _totalRows || column < 0 || column >= _columnsPerRow)
                return null;

            return _grid[row, column];
        }

        public void SetSquare(int row, int column, Square square)
        {
            if (_grid != null && row >= 0 && row < _totalRows && column >= 0 && column < _columnsPerRow)
            {
                _grid[row, column] = square;
            }
        }

        public void UpdateWithGroup(SquareGroup group)
        {
            var topLeftRow = group.TopLeftIndex.row;
            var topLeftColumn = group.TopLeftIndex.column;

            if (!ValidGroupAt(topLeftRow, topLeftColumn)) return;

            SetSquare(topLeftRow, topLeftColumn, group.TopLeft);
            SetSquare(topLeftRow, topLeftColumn + 1, group.TopRight);
            SetSquare(topLeftRow + 1, topLeftColumn + 1, group.BottomRight);
            SetSquare(topLeftRow + 1, topLeftColumn, group.BottomLeft);
        }

        public bool ValidGroupAt( int row, int col)
        {
            return _grid[row, col] &&
                   _grid[row, col + 1] &&
                   _grid[row + 1, col] &&
                   _grid[row + 1, col + 1];
        }


        public override string ToString()
        {
            if (_grid == null) return "Grid is null";

            var gridString = "Grid Layout:\n";
            for (var row = 0; row < _totalRows; row++)
            {
                for (var col = 0; col < _columnsPerRow; col++)
                {
                    var square = _grid[row, col];
                    gridString += (square != null ? square.name : "null") + "\t";
                }
                gridString += "\n";
            }
            return gridString;
        }
    }
}