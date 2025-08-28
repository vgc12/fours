namespace Board
{
    public readonly struct GridIndex
    {
        public readonly int Row;
        public readonly int Column;

        public GridIndex(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString() => $"({Row}, {Column})";
    }
}