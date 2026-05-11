using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.States
{
    public sealed class PausedState : UIBaseState
    {
        public override UILayer Layer => UILayer.Overlay;

        public PausedState(GameObject pausedMenu, UIManager uiManager) : base(pausedMenu, uiManager)
        {
            var buttons = RootPageElement.GetComponentsInChildren<Button>(true);
            buttons.First(b => b.name == "resume-button").onClick.AddListener(() => UIManager.SwitchToInGame());
            buttons.First(b => b.name == "options-button").onClick.AddListener(() => UIManager.SwitchToOptions());
            buttons.First(b => b.name == "main-menu-button").onClick.AddListener(() => UIManager.SwitchToMainMenu());
            buttons.First(b => b.name == "restart-button").onClick.AddListener(() => UIManager.ReloadLevel());
        }

    }
}