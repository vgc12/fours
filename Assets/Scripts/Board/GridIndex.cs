using System;
using UnityEngine;

namespace Board
{
    [Serializable]
    public struct GridIndex : IEquatable<GridIndex>
    {
        public int row;
        public int column;

        public GridIndex(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        // Implicit conversion FROM Vector2 TO GridIndex
        // Vector2 -> GridIndex automatically
        public static implicit operator GridIndex(Vector2 vector)
        {
            return new GridIndex((int)vector.y, (int)vector.x);
        }
    
        // Implicit conversion FROM GridIndex TO Vector2
        // GridIndex -> Vector2 automatically
        public static implicit operator Vector2(GridIndex index)
        {
            return new Vector2(index.column, index.row);
        }
    
        public static implicit operator GridIndex(Vector2Int vector)
        {
            return new GridIndex(vector.y, vector.x);
        }
    
        public static implicit operator Vector2Int(GridIndex index)
        {
            return new Vector2Int(index.column, index.row);
        }

        public override string ToString() => $"({row}, {column})";
    
        // Bonus: Equality operators for convenience
        public override bool Equals(object obj)
        {
            return obj is GridIndex other && row == other.row && column == other.column;
        }
    
        public override int GetHashCode()
        {
            return (Row: row, Column: column).GetHashCode();
        }
    
        public static bool operator ==(GridIndex left, GridIndex right)
        {
            return left.row == right.row && left.column == right.column;
        }
    
        public static bool operator !=(GridIndex left, GridIndex right)
        {
            return !(left == right);
        }

        public bool Equals(GridIndex other) => row == other.row && column == other.column;
    }
}