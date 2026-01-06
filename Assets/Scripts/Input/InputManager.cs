
using System;
using EventBus;
using Player.Input;
using Reflex.Attributes;
using Singletons;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


namespace Input
{

    public sealed class InputManager : Singleton<InputManager>, PlayerInputActions.IMainActions,PlayerInputActions.IUIActions, IInputManager
    {
        
        public event UnityAction SwipeRight;
        public event UnityAction SwipeLeft;
     
        public event UnityAction Tap;
        public event UnityAction LeftClick;
        public event UnityAction RightClick;
        public event UnityAction UIClick;

        public PlayerInputActions PlayerInputActions { get; private set; } 

        [Inject] private readonly ISwipeDetector _swipeDetector;

private EventBinding<UIEvent> _uiEventBinding;

        private void Start()
        {
            _uiEventBinding = new EventBinding<UIEvent>(OnUIEvent);
            EventBus<UIEvent>.Register(_uiEventBinding);
            
            InitializePlayerInputActions();

            InitializeSwipeDetector();
        }
        
        public void EnableMainActions()
        {
            PlayerInputActions.Main.Enable();
            PlayerInputActions.UI.Disable();
        }
        public void EnableUIActions()
        {
            PlayerInputActions.UI.Enable();
            PlayerInputActions.Main.Disable();
        }

        private void OnUIEvent(UIEvent obj)
        {
            if (obj.Type == UIEvent.UIEventType.InGame)
            {
                EnableMainActions();
            }
            else if (obj.Type == UIEvent.UIEventType.InMenu)
            {
                EnableUIActions();
            }
        }

        private void InitializePlayerInputActions()
        {
            PlayerInputActions = new PlayerInputActions();

            InitializeMainActions();
            InitializeUIActions();
            EnableUIActions();
        }

        private void InitializeSwipeDetector()
        {
            _swipeDetector.OnSwipeRightEvent += () => SwipeLeft?.Invoke();
            _swipeDetector.OnSwipeLeftEvent += () => SwipeRight?.Invoke(); 
            _swipeDetector.OnTapEvent += () => Tap?.Invoke();
        }

        private void InitializeMainActions()
        {
            PlayerInputActions.Main.SetCallbacks(this);
        }

        private void InitializeUIActions()
        {
            PlayerInputActions.UI.SetCallbacks(this);
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
        
        public void OnUIClick(InputAction.CallbackContext context)
        {
            var currentDevice = GetCurrentInputDevice(context);
            if (currentDevice is Touchscreen t)
            {
                HandleTouchInput(context, t);
                
            }
            else if (currentDevice is Mouse && context.started)
            {
                UIClick?.Invoke();
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