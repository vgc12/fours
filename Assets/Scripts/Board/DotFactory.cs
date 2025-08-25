using Board;
using UnityEngine;
using UnityEngine.Pool;

public class DotFactory : MonoBehaviour
{
    
    public static DotFactory Instance { get; private set; }
    
    public DotConfig config;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
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
        dot.squareGroup = squareGroup;
        

        dotObject.layer = LayerMask.NameToLayer("Dot");

        var col = dotObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(.5f,.5f);
        
        return dot;
    }
    
    
    
}