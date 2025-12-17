using System;
using UnityEngine;

namespace Board
{
    [Serializable]
    public class GridConfig
    {
        [Header("Grid Settings")]
        public Vector2 spacing = new(1f, 1f);
        [Min(1)] public int columnsPerRow = 5;
        
        [Header("Alignment")]
        public bool centerGrid = true;
        public Vector2 gridOffset = Vector2.zero;
        
        [Header("Debug")]
        public bool showGizmos = true;
    }
}