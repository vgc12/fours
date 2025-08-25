using System.Collections;
using System.Collections.Generic;
using EventChannel;
using UnityEngine;

namespace Board
{
    public static class GridGroupFinder
    {
        /// <summary>
        /// Finds all 2x2 groups in a 2D grid and returns their center points
        /// </summary>
        /// <param name="grid">The 2D array to search</param>
        public static List<Vector2> FindAllGroupCenters(Square[,] grid)
        {
            var centerPoints = new List<Vector2>();

            var rows = grid.GetLength(0);
            var cols = grid.GetLength(1);

            // Iterate through all possible top-left corners of 2x2 groups
            for (var row = 0; row < rows - 1; row++)
            {
                for (var col = 0; col < cols - 1; col++)
                {
                    // Check if all 4 positions have valid objects
                    if (IsValidGroup(grid, row, col))
                    {
                        var topLeft = grid[row, col];
                        var bottomRight = grid[row + 1, col + 1];
                        Vector2 centerPoint = (topLeft.transform.position + bottomRight.transform.position) / 2f;
                        centerPoints.Add(centerPoint);
                    }
                }
            }

            return centerPoints;
        }

        
        public static List<SquareGroup> SplitGridIntoGroups(Square[,] grid)
        {
            var groups = new List<SquareGroup>();

            var rows = grid.GetLength(0);
            var cols = grid.GetLength(1);

            // Iterate through all possible top-left corners of 2x2 groups
            for (var row = 0; row < rows - 1; row++)
            {
                for (var col = 0; col < cols - 1; col++)
                {
                    // Check if all 4 positions have valid objects
                    if (!IsValidGroup(grid, row, col)) continue;
                    var topLeft = grid[row, col];
                    var topRight = grid[row , col + 1];
                    var bottomLeft = grid[row + 1, col];
                    var bottomRight = grid[row + 1, col + 1];
                    var group = new SquareGroup(topLeft,topRight,bottomLeft,bottomRight, new GridIndex(row, col));
                    groups.Add(group);
                }
            }

            return groups;
        }
        
     

        private static bool IsValidGroup<T>(T[,] grid, int row, int col) where T : class
        {
            return grid[row, col] != null &&
                   grid[row, col + 1] != null &&
                   grid[row + 1, col] != null &&
                   grid[row + 1, col + 1] != null;
        }


    }

 
}


internal class SquareGroupRotatedEvent : IEvent
{
  
}