using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Board
{
    public sealed class DotManager
    {
        private List<Dot> _dots = new();

        public IReadOnlyList<Dot> Dots => _dots;

        public void CreateDots(List<SquareGroup> squareGroups)
        {
            ClearDots();
            
            _dots = new List<Dot>(squareGroups.Count);
            foreach (var squareGroup in squareGroups)
            {
                var dot = DotFactory.Instance.CreateDot(squareGroup);
                _dots.Add(dot);
            }
        }

        public void ResetDots(List<SquareGroup> squareGroups)
        {
            for (var i = 0; i < _dots.Count && i < squareGroups.Count; i++)
            {
                _dots[i].squareGroup = squareGroups[i];
            }
        }
        

        
        public void ClearDots()
        {
            _dots ??= new List<Dot>(Object.FindObjectsByType<Dot>(sortMode: FindObjectsSortMode.None));
            foreach (var dot in _dots)
            {
                if (dot != null && dot.gameObject != null)
                {
                    Object.Destroy(dot.gameObject);
                }
            }
        }
    }
}