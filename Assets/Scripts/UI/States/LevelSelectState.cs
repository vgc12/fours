using DependencyInjection;
using Levels;
using UI.UI.States;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UI.States
{
    public sealed class LevelSelectState : UIBaseState
    {
        
        private readonly ILevelManager _levelManager;
        public LevelSelectState(VisualElement rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            RuntimeResolver.Instance.TryResolve( out _levelManager);
            Assert.IsNotNull(_levelManager, "LevelSelectState: LevelManager dependency could not be resolved.");
            var levels = _levelManager.Levels;
            var buttonContainer = RootPageElement.Q<ScrollView>("level-container");
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                var button = new Button
                {
                    dataSource = level,
                    text = (i+1).ToString(),
                };
                button.AddToClassList("level-button");
                button.clicked += () =>
                {
                    _levelManager.LoadLevel(level);
                    UIManager.SwitchToInGame();
                };
                buttonContainer.Add(button);
            }
        }
    }
}