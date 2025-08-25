using System;
using System.Linq;
using Board;
using UnityEngine;
using UnityEngine.UI;


public class Dot : MonoBehaviour
{

    public DotConfig config;
    

    private SpriteRenderer _spriteRenderer;
    

    public SquareGroup squareGroup;



        
    
        
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = config.sprite;
    }

    private void Update()
    {
        if (squareGroup != null){
            transform.position = squareGroup.CenterPoint;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 20f);
        
    }

 
}
