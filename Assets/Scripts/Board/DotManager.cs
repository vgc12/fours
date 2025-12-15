using System.Collections.Generic;

namespace Board
{
    public sealed class DotManager
    {
        private List<Dot> _dots = new List<Dot>();

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

        private void ClearDots()
        {
            if (_dots == null) return;
            foreach (var dot in _dots)
            {
                if (dot != null && dot.gameObject != null)
                {
                    UnityEngine.Object.Destroy(dot.gameObject);
                }
            }
        }
    }
}