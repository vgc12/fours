using System.Linq;
using DependencyInjection;
using EventBus;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.States
{
    public sealed class InGameUIState : UIBaseState
    {
        public readonly GameObject GridParent;
        private readonly TMP_Text _levelText;
        private readonly ILevelManager _levelManager;

        public override UILayer Layer => UILayer.Gameplay;

        public InGameUIState(GameObject gridsObject, GameObject rootElement, UIManager uiManager) : base(rootElement,
            uiManager)
        {
            GridParent = gridsObject;
            Background = GridParent.GetComponentInChildren<Image>();
            BackgroundCanvas = GridParent.GetComponentInChildren<Canvas>();
            _levelText = rootElement.GetComponentInChildren<TMP_Text>();
            RuntimeResolver.Instance.TryResolve(out _levelManager);

            var buttons = RootPageElement.GetComponentsInChildren<Button>(true);
            buttons.First(b => b.name == "pause-button").onClick.AddListener(() => UIManager.SwitchToPaused());
        }

        public Image Background { get;  private set; }
        public Canvas BackgroundCanvas {get; private set;}
    

        public override void Enter()
        {
            base.Enter();
            EventBus<UIEvent>.Raise(new UIEvent(UIEvent.UIEventType.InGame));
            _levelText.text = $"Level {_levelManager.CurrentLevel.name}";
        }

        public override void Exit()
        {
            base.Exit();
            EventBus<UIEvent>.Raise(new UIEvent(UIEvent.UIEventType.InMenu));
        }
    }
}