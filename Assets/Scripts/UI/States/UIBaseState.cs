using StateMachine;
using UnityEngine;
using Cursor = UnityEngine.Cursor;

namespace UI.States
{
    public abstract class UIBaseState : BaseState
    {
        protected readonly GameObject RootPageElement;
 
        protected readonly UIManager UIManager;

        protected UIBaseState(GameObject rootElement, UIManager uiManager)
        {
            RootPageElement = rootElement;

            UIManager = uiManager;
            
            rootElement.SetActive(false);
            
        }

        public bool IsActive { get; protected set; }

        public virtual bool CanExit { get; protected set; } = true;

        public override void Enter()
        {
            IsActive = true;
            RootPageElement.SetActive(true);
            ChangeMouseState();
            
        }


        public override void Exit()
        {
            IsActive = false;
            RootPageElement.SetActive(false);
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