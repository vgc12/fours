using System.Runtime.InteropServices;
using Singletons;
using UnityEngine;
using UnityEngine.Pool;

namespace Board
{

    public sealed class DotFactory : Singleton<DotFactory>
    {
    
        public DotConfig config;
        
        public Dot CreateDot( SquareGroup squareGroup)
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