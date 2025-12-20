using StateMachine;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace UI.UI.States
{
    public abstract class UIBaseState : BaseState
    {
        protected readonly VisualElement RootPageElement;
 
        protected readonly UIManager UIManager;

        protected UIBaseState(VisualElement rootElement, UIManager uiManager)
        {
            RootPageElement = rootElement;
    
            UIManager = uiManager;
       
        }

        public bool IsActive { get; protected set; }

        public virtual bool CanExit { get; protected set; } = true;

        public override void Enter()
        {
            IsActive = true;
            RootPageElement.style.display = DisplayStyle.Flex;
            ChangeMouseState();
            
        }


        public override void Exit()
        {
            IsActive = false;
            RootPageElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     Called on enter, used to change whether the mouse is locked or not
        /// </summary>
        protected virtual void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }

        protected static void LockCursorAndHideMouse()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected static void UnlockCursorAndShowMouse()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    
}