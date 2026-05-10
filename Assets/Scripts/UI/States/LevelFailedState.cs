using UI.States;
using UnityEngine;

namespace UI
{
    public sealed class LevelFailedState : UIBaseState
    {
        public override UILayer Layer => UILayer.Modal;

        public LevelFailedState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager) { }
    }
}