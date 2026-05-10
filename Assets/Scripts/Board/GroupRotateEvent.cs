using EventBus;

namespace Board
{
    public enum RotationState
    {
        Started,
        Stopped
    }
    public readonly struct GroupRotateEvent : IEvent
    {
        public readonly Dot SquareGroup;
        public readonly RotationState RotationState;
        public readonly string GridSnapshot;
        public GroupRotateEvent(Dot squareGroup, RotationState rotationState, string gridSnapshot)
        {
            SquareGroup = squareGroup;
            RotationState = rotationState;
            GridSnapshot = gridSnapshot;
        }
    }
}