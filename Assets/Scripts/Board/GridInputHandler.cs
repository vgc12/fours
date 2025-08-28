using UnityEngine;

namespace Board
{
    public class GridInputHandler
    {
        private readonly Camera _camera;
        private readonly LayerMask _dotLayerMask;

        public GridInputHandler(Camera camera, LayerMask dotLayerMask)
        {
            _camera = camera;
            _dotLayerMask = dotLayerMask;
        }

        public Dot GetDotAtScreenPosition(Vector2 screenPosition)
        {
            Vector2 worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapCircle(worldPosition, 0.1f, _dotLayerMask);
            return hit?.GetComponent<Dot>();
        }
    }
}