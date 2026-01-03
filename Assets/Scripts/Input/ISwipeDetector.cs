using UnityEngine.Events;
using UnityEngine.InputSystem;

public interface ISwipeDetector
{
    void OnTouchStart(Touchscreen context);
    void OnTouchEnd(Touchscreen context);
    event UnityAction OnSwipeRightEvent;
    event UnityAction OnSwipeLeftEvent;
    event UnityAction OnTapEvent;
    
}