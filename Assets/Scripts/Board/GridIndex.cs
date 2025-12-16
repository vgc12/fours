using UnityEngine;

public readonly struct GridIndex
{
    public readonly int Row;
    public readonly int Column;

    public GridIndex(int row, int column)
    {
        Row = row;
        Column = column;
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
        return new Vector2(index.Column, index.Row);
    }
    
    public static implicit operator GridIndex(Vector2Int vector)
    {
        return new GridIndex(vector.y, vector.x);
    }
    
    public static implicit operator Vector2Int(GridIndex index)
    {
        return new Vector2Int(index.Column, index.Row);
    }

    public override string ToString() => $"({Row}, {Column})";
    
    // Bonus: Equality operators for convenience
    public override bool Equals(object obj)
    {
        return obj is GridIndex other && Row == other.Row && Column == other.Column;
    }
    
    public override int GetHashCode()
    {
        return (Row, Column).GetHashCode();
    }
    
    public static bool operator ==(GridIndex left, GridIndex right)
    {
        return left.Row == right.Row && left.Column == right.Column;
    }
    
    public static bool operator !=(GridIndex left, GridIndex right)
    {
        return !(left == right);
    }
}