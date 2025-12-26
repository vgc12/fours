using Extensions;
using UnityEngine;
using UnityEngine.UI;


namespace Board
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Square : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public GridIndex ID;
        [SerializeField] private bool inactive;

        public bool Inactive
        {
            get => inactive;
            set
            {
                inactive = value;
                spriteRenderer.enabled = !value;
            }
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


        private void Update()
        {
            gameObject.name = $"Square{ID}";
        }
        
        public static Square Create(GridIndex id, Color color, bool inactive, Transform parent)
        {
            var squareObject = new GameObject($"Square{id}", typeof(Square));
            squareObject.transform.SetParent(parent);
            squareObject.transform.localScale = Vector3.one;
            squareObject.transform.localPosition = Vector3.zero;
            squareObject.name = $"Square{ id}";
            
            var outline = new GameObject( "Outline", typeof(SpriteRenderer));
            outline.transform.SetParent( squareObject.transform);
            outline.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            outline.transform.localPosition = new Vector3(0, 0, 1);
            var outlineRenderer = outline.GetComponent<SpriteRenderer>();
            outlineRenderer.sprite = Resources.Load<Sprite>("Sprites/Square");
            outlineRenderer.color = Color.black;

            var square = squareObject.GetOrAdd<Square>();
            square.ID = id;
            square.spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Square");
            square.spriteRenderer.color = color;
            
            square.Inactive = inactive;

            return square;
        }
        
        
    }
}