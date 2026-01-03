using Cysharp.Threading.Tasks;
using Extensions;
using PrimeTween;
using UnityEngine;

namespace Board
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Square : MonoBehaviour
    {
        private static Sprite _cachedSquareSprite;
        
        public SpriteRenderer SpriteRenderer { get; private set; }
        public SpriteRenderer OutlineRenderer { get; private set; }
        public SpriteRenderer HighlightRenderer { get; private set; }
        
        public GridIndex Id { get; private set; }
        
        [SerializeField] private bool inactive;
        public bool Inactive
        {
            get => inactive;
            set
            {
                inactive = value;
                if (SpriteRenderer != null)
                    SpriteRenderer.enabled = !value;
            }
        }

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(GridIndex id, Color color, bool inactive)
        {
            Id = id;
            gameObject.name = $"Square{id}";
            SpriteRenderer.color = color;
            Inactive = inactive;
        }

        public static Square Create(GridIndex id, Color color, bool inactive, Transform parent)
        {
            // Cache sprite on first use
            if (_cachedSquareSprite == null)
            {
                _cachedSquareSprite = Resources.Load<Sprite>("Sprites/Square");
                if (_cachedSquareSprite == null)
                {
                    Debug.LogError("Failed to load Sprites/Square");
                    return null;
                }
            }

            // Create main square object
            var squareObject = new GameObject($"Square{id}", typeof(Square));
            squareObject.transform.SetParent(parent, false);
            
            var square = squareObject.GetOrAdd<Square>();
            square.SpriteRenderer.sprite = _cachedSquareSprite;

            // Create outline
            square.OutlineRenderer = CreateChildRenderer("Outline", squareObject.transform, 
                _cachedSquareSprite, Color.black, new Vector3(1.1f, 1.1f, 1f), 1f);

            // Create highlight
            square.HighlightRenderer = CreateChildRenderer("Highlight", squareObject.transform,
                _cachedSquareSprite, new Color(1f, 1f, 1f, 0.5f), Vector3.one, 1f);
            square.HighlightRenderer.enabled = false;

            square.Initialize(id, color, inactive);
            
            return square;
        }

        private static SpriteRenderer CreateChildRenderer(string name, Transform parent, 
            Sprite sprite, Color color, Vector3 scale, float zOffset)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer));
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = scale;
            obj.transform.localPosition = new Vector3(0, 0, zOffset);
            
            var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            
            return renderer;
        }

        public void Select()
        {
            HighlightRenderer.enabled = true; 
            HighlightRenderer.color = new Color(1,1,1,0);
            HighlightRenderer.sortingOrder = SpriteRenderer.sortingOrder + 1;
            Tween.Color(HighlightRenderer, new Color(1,1,1,0.5f), duration: 0.1f, ease: Ease.InOutCubic);
            
        }
        public async UniTask Deselect()
        {

            await Tween.Color(HighlightRenderer, new Color(1, 1, 1, 0f), duration: 0.1f, ease: Ease.InOutCubic);
      
            HighlightRenderer.enabled = false;
        }
    }
}