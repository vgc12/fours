using System.Linq;
using DependencyInjection;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI.States
{
    public sealed class LevelSelectState : UIBaseState
    {
        
        private readonly ILevelManager _levelManager;
        public LevelSelectState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            RuntimeResolver.Instance.TryResolve( out _levelManager);
            Assert.IsNotNull(_levelManager, "LevelSelectState: LevelManager dependency could not be resolved.");
            var levels = _levelManager.Levels;
            var buttonContainer = RootPageElement.GetComponentInChildren<GridLayoutGroup>();
            var buttonTemplate = RootPageElement.GetComponentInChildren<Button>();
   
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                
                var button = Object.Instantiate(buttonTemplate, buttonContainer.transform);
                button.gameObject.SetActive(true);
                button.onClick.AddListener( () =>
                {
                    _levelManager.LoadLevel(level);
                    UIManager.SwitchToInGame();
                });
                button.GetComponentInChildren<TMP_Text>().text = level.name;

            }
            
            buttonTemplate.gameObject.SetActive(false);
        }
    }
}