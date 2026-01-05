using UnityEngine;

namespace Board
{
    [CreateAssetMenu(fileName = "Dot Factory", menuName = "Factories/Dot Factory")]
    public sealed class DotFactory : ScriptableObject, IFactory<Dot, SquareGroup>
    {
    
        public DotConfig config;
        
        public Dot Create(SquareGroup squareGroup)
        {
            var dotObject = new GameObject("Dot")
            {
                transform =
                {
                    position = squareGroup.CenterPoint,
                }
            };
        
            var spriteRenderer = dotObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = config.sprite;
        
            var dot = dotObject.AddComponent<Dot>();
            dot.config = config;
            dot.SquareGroup = squareGroup;
            
        

            dotObject.layer = LayerMask.NameToLayer("Dot");

            var col = dotObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(.5f,.5f);
        
            return dot;
        }
    
    
    
    }
}