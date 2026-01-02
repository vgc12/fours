using Input;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Input
{
    public interface IInputManager
    {
        event UnityAction SwipeRight;
        event UnityAction SwipeLeft;
        
        event UnityAction Tap;
        
        event UnityAction LeftClick;
        event UnityAction RightClick;
        
        public PlayerInputActions PlayerInputActions { get; }
    }
}