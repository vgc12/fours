using Cysharp.Threading.Tasks;
using PrimeTween;
using Singletons;
using UnityEngine;

namespace Board
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Square : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer { get;  set; }
        public SpriteRenderer OutlineRenderer { get; set; }
        public SpriteRenderer HighlightRenderer { get; set; }

        
        public GridIndex Id { get; private set; }

        public SquareConfig squareConfig;
        
        [SerializeField] private bool inactive;

        public bool Inactive
        {
            get => inactive;
            private set
            {
                inactive = value;
                if (SpriteRenderer != null)
                    SpriteRenderer.enabled = !value;
            }
        }

        private void Awake() { SpriteRenderer = GetComponent<SpriteRenderer>(); }

        public void Initialize(GridIndex id, Color color, bool inact)
        {
            Id = id;
            gameObject.name = $"Square{id}";
            SpriteRenderer.color = color;
            Inactive = inact;
        }


  

        public async UniTask Select()
        {
            HighlightRenderer.enabled = true;
            HighlightRenderer.color = new Color(1, 1, 1, 0);
            HighlightRenderer.sortingOrder = SpriteRenderer.sortingOrder + 1;
            await Tween.Color(HighlightRenderer, squareConfig.selectTween);
        }

        public async UniTask Deselect()
        {
            await Tween.Color(HighlightRenderer, squareConfig.deselectTween);

            HighlightRenderer.enabled = false;
        }
        
     

    }
}
