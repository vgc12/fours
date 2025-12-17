using UnityEngine;
using UnityEngine.UI;


namespace Board
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Square : MonoBehaviour
    {
        [SerializeField] private Color color;
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
    }
}