
using System;
using Player.Input;
using Reflex.Attributes;
using Singletons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


namespace Input
{

    public sealed class InputManager : Singleton<InputManager>, PlayerInputActions.IMainActions, IInputManager
    {
        
        public event UnityAction SwipeRight;
        public event UnityAction SwipeLeft;
     
        public event UnityAction Tap;
        public event UnityAction LeftClick;
        public event UnityAction RightClick;
        public PlayerInputActions PlayerInputActions { get; private set; } 

        [Inject] private readonly ISwipeDetector _swipeDetector;



        private void Start() {             
            
            EnableActions();
            
            _swipeDetector.OnSwipeRightEvent += () => SwipeLeft?.Invoke();
            _swipeDetector.OnSwipeLeftEvent += () => SwipeRight?.Invoke(); 
            _swipeDetector.OnTapEvent += () => Tap?.Invoke();
        }

        private void EnableActions()
        {
            PlayerInputActions = new PlayerInputActions();
            PlayerInputActions.Main.Enable();
            PlayerInputActions.Main.SetCallbacks(this);
        }


        public static InputDevice GetCurrentInputDevice(InputAction.CallbackContext context)
        {
            return context.action.activeControl.device;
        }
        
        public void OnClick(InputAction.CallbackContext context)
        {
            
            var currentDevice = GetCurrentInputDevice(context);
            if (currentDevice is Touchscreen t)
            {
                HandleTouchInput(context, t);
                
            }
            else if (currentDevice is Mouse && context.started)
            {
                LeftClick?.Invoke();
            }

           
        }


        private void HandleTouchInput(InputAction.CallbackContext context, Touchscreen t)
        {
            if(context.started){
                _swipeDetector.OnTouchStart(t);
            }
            else if(context.canceled){
                _swipeDetector.OnTouchEnd(t);
                    
            }
        }


        public void OnRightClick(InputAction.CallbackContext context)
        {
            var currentDevice = GetCurrentInputDevice(context);

            if (currentDevice is Mouse && context.started)
            {
                RightClick?.Invoke();
            }

        
        }
    }
}