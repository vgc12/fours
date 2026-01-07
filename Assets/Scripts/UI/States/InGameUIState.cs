using EventBus;
using UnityEngine;


namespace UI.States
{
    public sealed class InGameUIState : UIBaseState
    {
        public InGameUIState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager) { }

        public override void Enter()
        {
            base.Enter();
            EventBus<UIEvent>.Raise(new UIEvent(UIEvent.UIEventType.InGame));
        }

        public override void Exit()
        {
            base.Exit();
            EventBus<UIEvent>.Raise(new UIEvent(UIEvent.UIEventType.InMenu));
        }
    }
}