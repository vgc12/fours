using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace UI.States
{
    public sealed class MainMenuState : UIBaseState
    {
        public override UILayer Layer => UILayer.Root;

        public MainMenuState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            var buttons = RootPageElement.GetComponentsInChildren<Button>(true);
            buttons.First(b => b.name == "play-button").onClick.AddListener(() => UIManager.SwitchToLevelSelect());
            buttons.First(b => b.name == "options-button").onClick.AddListener(() => UIManager.SwitchToOptions());
        }
    }
}