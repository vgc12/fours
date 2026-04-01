using DependencyInjection;
using Levels;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;

namespace UI.States
{
    public sealed class LoadingState : UIBaseState
    {
        public readonly RectTransform RotatingElement;
        private Tween _rotationTween;
        private readonly LevelManager _levelManager;
        private float _loadingTime;

        public LoadingState(GameObject rootElement, UIManager uiManager) : base(rootElement, uiManager)
        {
            RotatingElement = rootElement.transform.Find("rotating-element").GetComponent<RectTransform>();

            RuntimeResolver.Instance.TryResolve(out _levelManager);
            Assert.IsNotNull(_levelManager, "Level manager is null in loading state.");
        }

        public override void Enter()
        {
            base.Enter();
            _loadingTime = 0;
            _rotationTween = Tween.LocalRotation(RotatingElement,
                new Vector3(0, 0, 180), duration: .5f, cycles: -1, ease: Ease.InOutCubic);
        }

        public override void Update()
        {
            base.Update();
            _loadingTime += Time.deltaTime;
            if (_levelManager.LoadingInProgress || _loadingTime < 2f) return;
            _rotationTween.Stop();
            UIManager.SwitchToInGame();
        }
    }
}