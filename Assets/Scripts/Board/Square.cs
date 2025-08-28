using UnityEngine;
using UnityEngine.UI;


namespace Board
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Square : MonoBehaviour
    {
        [SerializeField] private Color color;
        public SpriteRenderer spriteRenderer;
        public GridIndex id;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
        
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color =  new Color(Random.Range(0.1f, 1.0f), Random.Range(0.1f, 1.0f), Random.Range(0.1f, 1.0f), 1f);
        

         
        
        

        }
    
    


        private void Update()
        {
            gameObject.name = $"Square{id}";
        }
    }
}
