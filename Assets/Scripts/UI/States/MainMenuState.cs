using Logging;
using UI.UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.States
{
    public sealed class MainMenuState : UIBaseState
    {
        private readonly Button _playButton;
        private readonly Button _optionsButton;

        public MainMenuState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            _playButton = RootPageElement.Q<Button>("play-button");
            _optionsButton = RootPageElement.Q<Button>("options-button");



            _playButton.clicked +=
                UIManager.SwitchToLevelSelect;
            _optionsButton.clicked += UIManager.SwitchToOptions;
        }

        ~MainMenuState()
        {
            _playButton.clicked -= UIManager.SwitchToLevelSelect;
            _optionsButton.clicked -= UIManager.SwitchToOptions;
        }
    }
}