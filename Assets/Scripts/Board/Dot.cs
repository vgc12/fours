using UnityEngine;

namespace Board
{
    public sealed class Dot : MonoBehaviour
    {
        public DotConfig config;


        private SpriteRenderer _spriteRenderer;


        private SquareGroup _squareGroup;
        public SquareGroup SquareGroup 
        { 
            get => _squareGroup;
            set
            {
                _squareGroup = value;
                _squareGroup.AttachedDot = this;
            }
        }


        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = config.sprite;
        }

        private void Update()
        {
            if (SquareGroup != null)
            {
                transform.position = SquareGroup.CenterPoint;
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 20f);
        }
    }
}