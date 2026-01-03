using Player.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public sealed class SwipeDetector : MonoBehaviour, ISwipeDetector
{
   
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private bool _isSwiping = false;
    
    private float MinSwipeDistance { get; set; } = 50f; // Minimum distance in pixels to consider as a swipe


    public void DetectSwipe()
    {
        var swipeDirection = _endPosition - _startPosition;

        if (swipeDirection.magnitude < MinSwipeDistance)
        {
            OnTapEvent?.Invoke();
            return; // Not a swipe, just a tap
        }

        // Normalize to get direction
        swipeDirection.Normalize();
        
      
        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
        {
            // Horizontal swipe
            if (swipeDirection.x > 0 || swipeDirection.y > 0)
            {
                OnSwipeRight();
            }
            else
            {
                OnSwipeLeft();
            }
        }
     
    
    }

    public event UnityAction OnSwipeRightEvent;
    public event UnityAction OnSwipeLeftEvent;
    public event UnityAction OnTapEvent;

    public void OnTouchStart(Touchscreen context)
    {
        _startPosition = context.position.ReadValue();
        _isSwiping = true;
    }

    public void OnTouchEnd(Touchscreen context)
    {
        if (!_isSwiping) return;

        _endPosition = context.position.ReadValue();
        DetectSwipe();
        _isSwiping = false;
    }
    

    private void OnSwipeRight()
    {
        OnSwipeRightEvent?.Invoke();
    }
    
    private void OnSwipeLeft()
    {
       
        OnSwipeLeftEvent?.Invoke();
    }
}