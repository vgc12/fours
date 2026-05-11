using Cysharp.Threading.Tasks;
using UI.States;

namespace UI
{
    public sealed class UITransitionBuilder
    {
        private readonly UIManager _ui;
        private readonly UIBaseState _target;
        private readonly UIBaseState _origin;
        private SlideFrom _grid;
        private SlideFrom _background;
        private bool _hasRun;

        internal UITransitionBuilder(UIManager ui, UIBaseState target, UIBaseState origin,
                                     SlideFrom grid = SlideFrom.None,
                                     SlideFrom background = SlideFrom.None)
        {
            _ui = ui;
            _target = target;
            _origin = origin;
            _grid = grid;
            _background = background;
            AutoRun().Forget();
        }

        public UITransitionBuilder From(UIBaseState origin,
                                        SlideFrom grid = SlideFrom.None,
                                        SlideFrom background = SlideFrom.None)
        {
            if (origin == _origin)
            {
                _grid = grid;
                _background = background;
            }
            return this;
        }

        public void Run()
        {
            if (_hasRun) return;
            _hasRun = true;
            _ui.ExecuteTransition(_target, _grid, _background);
        }

        internal void Cancel() => _hasRun = true;

        private async UniTaskVoid AutoRun()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            Run();
        }
    }
}