using Logging;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;


namespace UI.States
{
    public sealed class MainMenuState : UIBaseState
    {
        public override UILayer Layer => UILayer.Root;

        public MainMenuState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {

        }

    }
}