using System.Linq;
using UI.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class LevelFailedState : UIBaseState
    {
        public override UILayer Layer => UILayer.Modal;

        public LevelFailedState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            var buttons = RootPageElement.GetComponentsInChildren<Button>(true);
            buttons.First(b => b.name == "main-menu-button").onClick.AddListener(() => UIManager.SwitchToMainMenu());
            buttons.First(b => b.name == "restart-button").onClick.AddListener(() => UIManager.ReloadLevel());
        }
    }
}