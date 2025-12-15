using System.Collections.Generic;
using EventBus;

namespace Board
{
    public static class GridGroupFinder
    {
        
        public static List<SquareGroup> SplitGridIntoGroups(GridData grid)
        {
            var groups = new List<SquareGroup>();

            var rows = grid.TotalRows;
            var cols = grid.ColumnsPerRow;

            // Iterate through all possible top-left corners of 2x2 groups
            for (var row = 0; row < rows - 1; row++)
            {
                for (var col = 0; col < cols - 1; col++)
                {
                    // Check if all 4 positions have valid objects
                    if (!grid.ValidGroupAt(row, col)) continue;
                    var topLeft = grid.GetSquare(row, col);
                    var topRight = grid.GetSquare(row , col + 1);
                    var bottomLeft = grid.GetSquare(row + 1, col);
                    var bottomRight = grid.GetSquare(row + 1, col + 1);
                    var group = new SquareGroup(topLeft,topRight,bottomLeft,bottomRight, new GridIndex(row, col));
                    groups.Add(group);
                }
            }

            return groups;
        }
        
     

      

    }


    internal class SquareGroupRotatedEvent : IEvent
    {
  
    }
}