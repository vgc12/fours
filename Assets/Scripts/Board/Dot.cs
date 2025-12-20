using UnityEngine;

namespace Board
{
    public sealed class Dot : MonoBehaviour
    {

        public DotConfig config;
    

        private SpriteRenderer _spriteRenderer;
    

        public SquareGroup squareGroup;



        
    
        
        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = config.sprite;
            _initialScale = transform.localScale;
        }

        private void Update()
        {
            if (squareGroup != null){
                transform.position = squareGroup.CenterPoint;
            }
        }

        private Vector3 _initialScale;

        public void OnSelect()
        {
            squareGroup.TopLeft.transform.localScale = new(1.1f, 1.1f, 1.1f);
            squareGroup.TopRight.transform.localScale = new(1.1f, 1.1f, 1.1f);
            squareGroup.BottomLeft.transform.localScale = new(1.1f, 1.1f, 1.1f);
            squareGroup.BottomRight.transform.localScale = new(1.1f, 1.1f, 1.1f);
        }
        
        public void OnDeselect()
        {
            squareGroup.TopLeft.transform.localScale = _initialScale;
            squareGroup.TopRight.transform.localScale = _initialScale;
            squareGroup.BottomLeft.transform.localScale = _initialScale;
            squareGroup.BottomRight.transform.localScale = _initialScale;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 20f);
        
        }

 
    }
}
