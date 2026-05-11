using System.Linq;
using DependencyInjection;
using Levels;
using UI.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class LevelCompleteState : UIBaseState
    {
        public override UILayer Layer => UILayer.Modal;

        private readonly ILevelManager _levelManager;
        private readonly Button _nextLevelButton;
        private readonly Button _mainMenuButton;

        public LevelCompleteState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            RuntimeResolver.Instance.TryResolve(out _levelManager);
            var buttons = RootPageElement.GetComponentsInChildren<Button>();
            _nextLevelButton = buttons.First(b => b.name == "next-level-button");
            _mainMenuButton = buttons.First(b => b.name == "main-menu-button");


            if (_levelManager.NextLevel == _levelManager.CurrentLevel)
            {
                _nextLevelButton.gameObject.SetActive(false);
            }

            _nextLevelButton.onClick.AddListener(() =>
            {
                var nextLevel = _levelManager.NextLevel;
                if (nextLevel == null)
                {
                    return;
                }

                _levelManager.LoadLevel(nextLevel);
                UIManager.SwitchToInGame().From(this, SlideFrom.Above, SlideFrom.Above);
            });

            _mainMenuButton.onClick.AddListener(() => UIManager.SwitchToMainMenu());
        }
    }
}