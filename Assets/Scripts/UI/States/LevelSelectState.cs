using System.Linq;
using Cysharp.Threading.Tasks;
using DependencyInjection;
using Levels;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI.States
{
    public sealed class LevelSelectState : UIBaseState
    {

        public override UILayer Layer => UILayer.SubMenu;

        private readonly ILevelManager _levelManager;
        private readonly Button _backButton;
        public LevelSelectState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            RuntimeResolver.Instance.TryResolve( out _levelManager);
            Assert.IsNotNull(_levelManager, "LevelSelectState: LevelManager dependency could not be resolved.");
            var levels = _levelManager.Levels;
            var buttonContainer = RootPageElement.GetComponentInChildren<GridLayoutGroup>();
            var buttons = RootPageElement.GetComponentsInChildren<Button>(true);
            var buttonTemplate = buttons.First(b => b.name == "level-button");
            _backButton = buttons.First(b => b.name == "back-button");
            Assert.IsNotNull(buttonTemplate, "LevelSelectState: ButtonTemplate dependency could not be resolved.");
            Assert.IsNotNull(_backButton, "LevelSelectState: Back Button dependency could not be resolved.s");
            _backButton.onClick.AddListener( () =>
            {
                UIManager.SwitchToMainMenu();
            });
   
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                
                var button = Object.Instantiate(buttonTemplate, buttonContainer.transform);
                button.gameObject.SetActive(true);
                button.onClick.AddListener( () => UIManager.LoadLevel(level).Forget());
                button.GetComponentInChildren<TMP_Text>().text = level.name;

            }
            
            buttonTemplate.gameObject.SetActive(false);
        }

   
        
    }
}